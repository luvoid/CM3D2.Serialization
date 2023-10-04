using CM3D2.Serialization.SourceGenerators.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;

namespace CM3D2.Serialization.SourceGenerators
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class CM3D2SerializableAnalyzer : DiagnosticAnalyzer
	{
		const string k_Category = "CM3D2.Serialization.ICM3D2Serializable";

		private static readonly DiagnosticDescriptor NonSerializableField = new(
			id: "CM3D2Serialization001",
			title: "Non-Serializable Field",
			messageFormat: "The field '{0}' is of type '{1}' which cannot be serialized",
			category: k_Category,
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true
		);

		private static readonly DiagnosticDescriptor MissingFieldReadWrite = new(
			id: "CM3D2Serialization002",
			title: "Missing Field Read / Write",
			messageFormat: "The field '{0}' has no read/write",
			category: k_Category,
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true
		);

		private static readonly DiagnosticDescriptor FieldReadWriteOutOfOrder = new(
			id: "CM3D2Serialization003",
			title: "Field Read / Write out of Order",
			messageFormat: "Expected read/write of field '{0}' to come after '{1}'",
			category: k_Category,
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true
		);

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
			NonSerializableField,
			MissingFieldReadWrite,
			FieldReadWriteOutOfOrder
		);

		private readonly ConcurrentBag<IFieldSymbol> nonSerializableFields = new();

		public override void Initialize(AnalysisContext context)
		{
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
			context.EnableConcurrentExecution();

			context.RegisterSymbolAction(AnalyzeFieldSymbol, SymbolKind.Field);
			context.RegisterSemanticModelAction(AnalyzeSemanticModel);

			context.RegisterCodeBlockStartAction<SyntaxKind>(StartAnalyzeCodeBlock);
		}

		private void AnalyzeFieldSymbol(SymbolAnalysisContext context)
		{
			var fieldSymbol = context.Symbol as IFieldSymbol;
			if (fieldSymbol.IsConst) return;
			if (fieldSymbol.IsStatic) return;
			if (fieldSymbol.IsDeepSerialized()) return;
			if (fieldSymbol.Type.IsCM3D2Serializable()) return;
			nonSerializableFields.Add(fieldSymbol);
		}

		private void AnalyzeSemanticModel(SemanticModelAnalysisContext context)
		{
			while (nonSerializableFields.TryTake(out var fieldSymbol))
			{
				var location = fieldSymbol.Locations.Single();
				var position = location.SourceSpan.Start;
				var diagnostic = Diagnostic.Create(NonSerializableField, location,
					fieldSymbol.CanBeReferencedByName ? fieldSymbol.Name : fieldSymbol.ToMinimalDisplayString(context.SemanticModel, position, SymbolDisplayFormat.CSharpErrorMessageFormat),
					fieldSymbol.Type.ToMinimalDisplayString(context.SemanticModel, position, SymbolDisplayFormat.CSharpErrorMessageFormat)
				);
				context.ReportDiagnostic(diagnostic);
			}
		}

		public void StartAnalyzeCodeBlock(CodeBlockStartAnalysisContext<SyntaxKind> context)
		{
			if (context.OwningSymbol is not IMethodSymbol methodSymbol
				|| (methodSymbol.Name != "ReadWith" && methodSymbol.Name != "WriteWith" && !methodSymbol.Name.StartsWith("CM3D2.Serialization.ICM3D2Serializable."))
				|| methodSymbol.ContainingType == null
				|| !methodSymbol.ContainingType.ImplementsAnyOf("ICM3D2Serializable", "ICM3D2SerializableInstance")
			)
				return;

			ReadWriteWithAnalyzer analyzer = new(context, methodSymbol);
		}

		private class ReadWriteWithAnalyzer
		{
			private readonly IMethodSymbol methodSymbol;
			private readonly INamedTypeSymbol containingType;
			private readonly ReadOnlyCollection<MemberPath> serializableFields;
			private readonly Stack<MemberPath> serializedFields;
			private readonly Dictionary<ILocalSymbol, MemberPath> resolvedVariables = new(SymbolEqualityComparer.Default);
			private readonly Dictionary<ILocalSymbol, List<(MemberPath, Location)>> unresolvedArgumentPaths = new(SymbolEqualityComparer.Default);

			public ReadWriteWithAnalyzer(in CodeBlockStartAnalysisContext<SyntaxKind> context, IMethodSymbol methodSymbol)
			{
				this.methodSymbol = methodSymbol;
				this.containingType = methodSymbol.ContainingType;

				context.RegisterSyntaxNodeAction(AnalyzeVariableDeclaratorSyntax, SyntaxKind.VariableDeclarator);
				context.RegisterSyntaxNodeAction(AnalyzeForEachStatementSyntax, SyntaxKind.ForEachVariableStatement);
				context.RegisterSyntaxNodeAction(AnalyzeInvocationExpressionSyntax, SyntaxKind.InvocationExpression);
				serializableFields = GetSerializableFields(containingType);
				serializedFields = new();

				Debug.WriteLine("Serializable Fields:");
				Debug.Indent();
				foreach (MemberPath fieldPath in serializableFields)
				{
					Debug.WriteLine(fieldPath);
				}
				Debug.Unindent();
			}

			private ReadOnlyCollection<MemberPath> GetSerializableFields(INamedTypeSymbol enclosingType)
			{
				Dictionary<INamedTypeSymbol, ReadOnlyCollection<IFieldSymbol>> cachedSerializableFields = new(SymbolEqualityComparer.Default);

				bool includeReadOnly = methodSymbol.Name == "WriteWith";

				IEnumerable<IEnumerable<ISymbol>> GetRecursive(ITypeSymbol type, List<ISymbol> memberPath, Stack<ITypeSymbol> typeStack, BindingFlags bindingFlags)
				{
					memberPath ??= new();
					typeStack ??= new();

					if (!type.IsCM3D2Serializable() && !type.HasAttribute("DeepSerializable")
						&& type.Implements(includeReadOnly ? "IReadOnlyCollection" : "ICollection", 1, out var collectionInterface))
					{
						type.TryGetIndexer(out var indexer);
						var elementType = collectionInterface.TypeArguments[0];
						memberPath.Add(null);
						if (elementType.HasAttribute("DeepSerializable"))
						{
							foreach (var x in GetRecursive(elementType, memberPath, typeStack, bindingFlags & ~BindingFlags.NonPublic))
								yield return x;
						}
						else if (elementType.IsCM3D2Serializable())
						{
							yield return memberPath;
						}
						memberPath.RemoveLast();
						yield break;
					}

					if (typeStack.Contains(type)) yield break; // Can't require recursive type references to be serialized.
					typeStack.Push(type);
					foreach (var field in type.GetFields(bindingFlags, includeConst: false, includeReadOnly))
					{
						memberPath.Add(field);
						if (field.IsDeepSerialized())
						{
							foreach (var x in GetRecursive(field.Type, memberPath, typeStack, bindingFlags & ~BindingFlags.NonPublic)) 
								yield return x;
						}
						else if (field.Type.IsCM3D2Serializable())
						{
							yield return memberPath;
						}
						memberPath.RemoveLast();
					}
					typeStack.Pop();
				}

				var fieldPathList = new List<MemberPath>();
				foreach (var fieldStack in GetRecursive(enclosingType, null, null, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
				{
					fieldPathList.Add(new MemberPath(fieldStack));
				}
				return fieldPathList.AsReadOnly();
			}

			public void AnalyzeVariableDeclaratorSyntax(SyntaxNodeAnalysisContext context)
			{

				var declarator = context.Node as VariableDeclaratorSyntax;
				if (!declarator.TryGetDeclaredSymbol(context.SemanticModel, out ILocalSymbol localSymbol))
				{
					Console.WriteLine($"Unknown variable: {declarator.Identifier}");
					return;
				}
				if (declarator.Initializer != null)
				{
					var fieldPath = MemberPath.FromExpression(declarator.Initializer.Value, context.SemanticModel);
					if (fieldPath != MemberPath.Empty)
					{
						resolvedVariables[localSymbol] = fieldPath;
					}
				}
			}

			public void AnalyzeForEachStatementSyntax(SyntaxNodeAnalysisContext context)
			{
				var statement = context.Node as ForEachStatementSyntax;
				if (!statement.TryGetDeclaredSymbol(context.SemanticModel, out ILocalSymbol localSymbol))
				{
					Console.WriteLine($"Unknown variable: {statement.Identifier}");
					return;
				}
				var fieldPath = MemberPath.FromExpression(statement.Expression, context.SemanticModel);
				if (fieldPath != MemberPath.Empty)
				{
					resolvedVariables[localSymbol] = fieldPath;
				}
			}

			public void AnalyzeInvocationExpressionSyntax(SyntaxNodeAnalysisContext context)
			{
				var invocationExpression = context.Node as InvocationExpressionSyntax;

				if (!invocationExpression.TryGetSpeculativeSymbol(context.SemanticModel, out IMethodSymbol methodSymbol)) return;
				if (invocationExpression.Expression is not MemberAccessExpressionSyntax targetAccess) return;
				if (invocationExpression.ArgumentList.Arguments.Count < 1) return;

				if (!targetAccess.Expression.TryGetSpeculativeTypeSymbol(context.SemanticModel, out ITypeSymbol targetType)) return;
				if (!targetType.ImplementsAnyOf("ICM3D2Reader", "ICM3D2Writer")) return;

				if (!this.methodSymbol.Name.Contains(methodSymbol.Name)) return;

				var argumentExpression = invocationExpression.ArgumentList.Arguments[0].Expression;
				if (!argumentExpression.TryGetSpeculativeSymbol(context.SemanticModel, out IFieldSymbol argumentSymbol)) return;

				if (TryResolveMemberPath(argumentExpression, out var argumentPath, out var unresolvedSymbol, context.SemanticModel))
				{
					OnResolveArgumentPath(argumentPath, invocationExpression.GetLocation(), context.ReportDiagnostic);
				}
				else if (argumentPath.Count > 0 && unresolvedSymbol != null)
				{
					if (!unresolvedArgumentPaths.TryGetValue(unresolvedSymbol, out var list))
						unresolvedArgumentPaths[unresolvedSymbol] = list = new();

					list.Add((argumentPath, invocationExpression.GetLocation()));
				}

				Console.WriteLine($"{methodSymbol.Name}({argumentPath.ToString() ?? "null"})");
			}

			bool TryResolveMemberPath(ExpressionSyntax expression, out MemberPath memberPath, out ILocalSymbol unresolvedSymbol, SemanticModel semanticModel)
			{
				unresolvedSymbol = null;

				memberPath = MemberPath.FromExpression(expression, semanticModel, out expression);

				if (memberPath == MemberPath.Empty) return false;

				return TryResolveMemberPath(ref memberPath, out unresolvedSymbol, semanticModel);
			}

			/// <param name="unresolvedSymbol">The new unresolved symbol if the path was only partially resolved.</param>
			bool TryResolveMemberPath(ref MemberPath memberPath, out ILocalSymbol unresolvedSymbol, SemanticModel semanticModel)
			{
				bool partiallyResolved = false;
				int i = 0;
				while (memberPath[0] is ILocalSymbol localSymbol && i++ < resolvedVariables.Count)
				{
					if (resolvedVariables.TryGetValue(localSymbol, out var variablePath))
					{
						partiallyResolved = true;
						memberPath = memberPath.RemoveAt(0).InsertRange(0, variablePath);
					}
					else
					{
						break;
					}
				}
				unresolvedSymbol = (partiallyResolved && memberPath[0] is ILocalSymbol localSymbol2) ? localSymbol2 : null;
				return memberPath[0] is not ILocalSymbol;
			}

			void ResolveUnresolvedArgumentPaths(ILocalSymbol localSymbol, SemanticModel semanticModel, Action<Diagnostic> reportDiagnostic)
			{
				if (!unresolvedArgumentPaths.TryGetValue(localSymbol, out var list)) return;

				foreach (var tuple in list.ToArray())
				{
					int index = list.IndexOf(tuple);
					var memberPath = tuple.Item1;
					if (TryResolveMemberPath(ref memberPath, out var unresolvedSymbol, semanticModel))
					{
						OnResolveArgumentPath()
					}
					else if (unresolvedSymbol != null)
					{
						if (!unresolvedArgumentPaths.TryGetValue(unresolvedSymbol, out var newList))
							unresolvedArgumentPaths[unresolvedSymbol] = newList = new();

						newList.Add((memberPath, tuple.Item2));
					}
					list.RemoveAt(index);
				}
			}

			void OnResolveArgumentPath(MemberPath argumentPath, Location location, Action<Diagnostic> reportDiagnostic)
			{
				int fieldIndex = serializableFields.IndexOf(argumentPath, WildcardSymbolEqualityComparer.Default);
				if (fieldIndex == -1) return;

				bool isFirst = !serializedFields.TryPeek(out var previousArgument);
				serializedFields.Push(argumentPath);

				MemberPath? expectedPreviousArgument = fieldIndex == 0 ? null : serializableFields[fieldIndex - 1];

				if ((isFirst && expectedPreviousArgument == null)
					|| ((IStructuralEquatable)previousArgument).Equals(expectedPreviousArgument, WildcardSymbolEqualityComparer.Default))
					return; // All good

				var diagnostic = Diagnostic.Create(FieldReadWriteOutOfOrder, location,
					argumentPath, expectedPreviousArgument?.ToString() ?? "null");

				reportDiagnostic(diagnostic);
			}
		}
	}
}
