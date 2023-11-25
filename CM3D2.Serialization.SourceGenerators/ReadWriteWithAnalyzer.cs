using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;
using System.Reflection;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis;
using CM3D2.Serialization.SourceGenerators.Extensions;
using CM3D2.Serialization.Types;
using System.Linq;
using System.Linq.Expressions;
using System.IO;

namespace CM3D2.Serialization.SourceGenerators
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class ReadWriteWithAnalyzer : DiagnosticAnalyzer
	{
		const string k_Category = "CM3D2.Serialization.ReadWriteWith";

		private static readonly DiagnosticDescriptor MissingFieldReadWrite = new(
			id: "CM3D2Serialization030",
			title: "Missing Field Read / Write",
			messageFormat: "The field '{0}' has no read/write",
			category: k_Category,
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true
		);

		private static readonly DiagnosticDescriptor FieldReadWriteOutOfOrder = new(
			id: "CM3D2Serialization031",
			title: "Field Read / Write out of Order",
			messageFormat: "Expected read/write of field '{0}' to come after '{1}'",
			category: k_Category,
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true
		);

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
			MissingFieldReadWrite,
			FieldReadWriteOutOfOrder
		);

		public override void Initialize(AnalysisContext context)
		{
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
			context.EnableConcurrentExecution();

			context.RegisterCodeBlockStartAction<SyntaxKind>(StartAnalyzeCodeBlock);
		}


		public void StartAnalyzeCodeBlock(CodeBlockStartAnalysisContext<SyntaxKind> context)
		{
			if (context.OwningSymbol is not IMethodSymbol methodSymbol
				|| (!methodSymbol.Name.EndsWith(nameof(ICM3D2Serializable.ReadWith)) && !methodSymbol.Name.EndsWith(nameof(ICM3D2Serializable.WriteWith)))
				|| methodSymbol.ContainingType == null
				|| !methodSymbol.ContainingType.ImplementsAnyOf(typeof(ICM3D2Serializable), typeof(ICM3D2SerializableInstance))
			)
				return;

			MethodAnalyzer analyzer = new(context, methodSymbol);
		}



		private class MethodAnalyzer
		{
			private readonly IMethodSymbol methodSymbol;
			private readonly INamedTypeSymbol containingType;
			private readonly bool isReadMethod;
			private readonly bool isWriteMethod;
			private readonly ReadOnlyCollection<MemberPath> serializableFields;
			private readonly Stack<MemberPath> serializedFields;
			private readonly Dictionary<ILocalSymbol, MemberPath> resolvedVariables = new(SymbolEqualityComparer.Default);
			private readonly Dictionary<ILocalSymbol, List<(MemberPath, Location)>> unresolvedArgumentPaths = new(SymbolEqualityComparer.Default);
			private readonly SortedList<Location, MemberPath> resolvedArgumentPaths = new(LocationComparer.DepthFirst);


			public MethodAnalyzer(in CodeBlockStartAnalysisContext<SyntaxKind> context, IMethodSymbol methodSymbol)
			{
				this.methodSymbol = methodSymbol;
				this.containingType = methodSymbol.ContainingType;

				isReadMethod = methodSymbol.Name.Contains("Read");
				isWriteMethod = methodSymbol.Name.Contains("Write");

				serializableFields = GetSerializableFields(containingType);
				serializedFields = new();

				lock(typeof(Debug))
				{
					Debug.WriteLine("Serializable Fields:");
					Debug.Indent();
					foreach (MemberPath fieldPath in serializableFields)
					{
						Debug.WriteLine(fieldPath);
					}
					Debug.Unindent();
				}

				context.RegisterSyntaxNodeAction(AnalyzeVariableDeclaratorSyntax, SyntaxKind.VariableDeclarator);
				context.RegisterSyntaxNodeAction(AnalyzeForEachStatementSyntax, SyntaxKind.ForEachStatement);
				context.RegisterSyntaxNodeAction(AnalyzeAssignmentExpressionSyntax, SyntaxKind.SimpleAssignmentExpression);
				context.RegisterSyntaxNodeAction(AnalyzeInvocationExpressionSyntax, SyntaxKind.InvocationExpression);
				context.RegisterCodeBlockEndAction(AnalyzeCodeBlockEnd);
			}

			private ReadOnlyCollection<MemberPath> GetSerializableFields(INamedTypeSymbol enclosingType)
			{
				Dictionary<INamedTypeSymbol, ReadOnlyCollection<IFieldSymbol>> cachedSerializableFields = new(SymbolEqualityComparer.Default);

				bool includeReadOnly = isWriteMethod;

				IEnumerable<IEnumerable<ISymbol>> GetRecursive(ITypeSymbol type, List<ISymbol> memberPath, Stack<ITypeSymbol> typeStack, BindingFlags bindingFlags)
				{
					memberPath ??= new();
					typeStack ??= new();

					if (!type.IsCM3D2Serializable() && !type.HasAttribute(typeof(DeepSerializableAttribute))
						&& type.Implements(includeReadOnly ? "IReadOnlyCollection" : "ICollection", 1, out var collectionInterface))
					{
						type.TryGetIndexer(out var indexer);
						var elementType = collectionInterface.TypeArguments[0];
						memberPath.Add(null);
						if (elementType.HasAttribute(typeof(DeepSerializableAttribute)))
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
						if (!field.ContainingType.IsCM3D2Serializable()) continue;
						if (field.HasAttribute(typeof(NonSerializedAttribute))) continue;

						memberPath.Add(field.AssociatedSymbol ?? field);
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

			void AnalyzeVariableDeclaratorSyntax(SyntaxNodeAnalysisContext context)
			{

				var declarator = context.Node as VariableDeclaratorSyntax;
				if (!declarator.TryGetDeclaredSymbol(context.SemanticModel, out ILocalSymbol localSymbol))
				{
					Debug.WriteLine($"Unknown variable: {declarator.Identifier}");
				}
				else if (declarator.Initializer != null)
				{
					OnVariableAssigned(localSymbol, declarator.Initializer.Value, context.SemanticModel);
				}
			}

			void AnalyzeForEachStatementSyntax(SyntaxNodeAnalysisContext context)
			{
				var statement = context.Node as ForEachStatementSyntax;
				if (!statement.TryGetDeclaredSymbol(context.SemanticModel, out ILocalSymbol localSymbol))
				{
					Debug.WriteLine($"Unknown variable: {statement.Identifier}");
					return;
				}

				var memberPath = MemberPath.FromExpression(statement.Expression, context.SemanticModel);
				if (memberPath != MemberPath.Empty)
				{
					memberPath = memberPath.Add(SpecialSymbol.ForEach);
					resolvedVariables[localSymbol] = memberPath;
				}
			}

			void AnalyzeAssignmentExpressionSyntax(SyntaxNodeAnalysisContext context)
			{
				var assignmentExpression = context.Node as AssignmentExpressionSyntax;
				var leftSymbol = assignmentExpression.Left.GetSpeculativeSymbol(context.SemanticModel);
				var rightSymbol = assignmentExpression.Right.GetSpeculativeSymbol(context.SemanticModel);

				var leftPath = MemberPath.FromExpression(assignmentExpression.Left, context.SemanticModel);
				var rightPath = MemberPath.FromExpression(assignmentExpression.Right, context.SemanticModel);

				if (leftSymbol is ILocalSymbol leftLocalSymbol)
				{
					OnVariableAssigned(leftLocalSymbol, assignmentExpression.Right, context.SemanticModel);
				}
				else if (rightSymbol is ILocalSymbol rightLocalSymbol)
				{
					resolvedVariables[rightLocalSymbol] = leftPath;
					ResolveUnresolvedArgumentPaths(rightLocalSymbol, context.SemanticModel);
				}
			}

			void OnVariableAssigned(ILocalSymbol variableSymbol, ExpressionSyntax expression, SemanticModel semanticModel)
			{
				if (TryResolveMemberPath(expression, out var memberPath, out ILocalSymbol unresolvedSymbol, semanticModel))
				{
					resolvedVariables[variableSymbol] = memberPath;
				}
				else
				{
					SetAsUnresolved(variableSymbol);
					if (unresolvedSymbol != null 
						&& unresolvedArgumentPaths.TryGetValue(unresolvedSymbol, out var unresolvedPaths))
					{
						unresolvedArgumentPaths[variableSymbol].AddRange(
							from tuple in unresolvedPaths
							where tuple.Item1.StartsWith(memberPath, SymbolEqualityComparer.Default)
							select (
								tuple.Item1.Replace(unresolvedSymbol, variableSymbol, SymbolEqualityComparer.Default),
								tuple.Item2
							)
						);
					}
				}
			}

			void AnalyzeInvocationExpressionSyntax(SyntaxNodeAnalysisContext context)
			{
				var invocationExpression = context.Node as InvocationExpressionSyntax;

				Debug.WriteLine(invocationExpression);

				if (!invocationExpression.TryGetSpeculativeSymbol(context.SemanticModel, out IMethodSymbol methodSymbol)) return;
				if (invocationExpression.Expression is not MemberAccessExpressionSyntax targetAccess) return;
				if (invocationExpression.ArgumentList.Arguments.Count < 1) return;


				ITypeSymbol targetType;
				if (targetAccess.Expression.TryGetSpeculativeSymbol(context.SemanticModel, out ISymbol targetSymbol))
				{
					targetSymbol.TryGetTypeSymbol(out targetType);
					CheckAddInvocation(context, targetSymbol, methodSymbol);
				}
				else
				{
					targetAccess.Expression.TryGetSpeculativeTypeSymbol(context.SemanticModel, out targetType);
				}

				if (targetType is null) return;
				CheckReadWriteInvocation(context, targetType, methodSymbol);
			}

			private void CheckAddInvocation(SyntaxNodeAnalysisContext context, ISymbol targetSymbol, IMethodSymbol methodSymbol)
			{
				var invocationExpression = context.Node as InvocationExpressionSyntax;

				if (!targetSymbol.TryGetTypeSymbol(out ITypeSymbol targetType)) return;
				if (!targetType.ImplementsAnyOf(typeof(IList), typeof(ICollection<>))) return;

				var argumentExpression = invocationExpression.ArgumentList.Arguments[0].Expression;
				if (!argumentExpression.TryGetDeclaredSymbol(context.SemanticModel, out ILocalSymbol argumentSymbol)
					&& !argumentExpression.TryGetSpeculativeSymbol(context.SemanticModel, out argumentSymbol))
					return;

				var fieldPath = MemberPath.FromExpression(invocationExpression.Expression, context.SemanticModel);
				if (fieldPath != MemberPath.Empty)
				{
					resolvedVariables[argumentSymbol] = fieldPath;
					ResolveUnresolvedArgumentPaths(argumentSymbol, context.SemanticModel);
				}
			}

			private void CheckReadWriteInvocation(SyntaxNodeAnalysisContext context, ITypeSymbol targetType, IMethodSymbol methodSymbol)
			{
				var invocationExpression = context.Node as InvocationExpressionSyntax;

				if (!targetType.ImplementsAnyOf(typeof(ICM3D2Reader), typeof(ICM3D2Writer))) return;

				if (!this.methodSymbol.Name.Contains(methodSymbol.Name)) return;

				var argumentExpression = invocationExpression.ArgumentList.Arguments[0].Expression;
				if (!argumentExpression.TryGetSpeculativeSymbol(context.SemanticModel, out ISymbol argumentSymbol)) return;

				if (TryResolveMemberPath(argumentExpression, out var argumentPath, out var unresolvedSymbol, context.SemanticModel))
				{
					OnResolveArgumentPath(argumentPath, invocationExpression.GetLocation());
				}
				else if (argumentPath.Count > 0)
				{
					unresolvedArgumentPaths[unresolvedSymbol].Add((argumentPath, invocationExpression.GetLocation()));
				}

				Debug.WriteLine($"{methodSymbol.Name}({argumentPath.ToString() ?? "null"})");
			}


			private bool TryResolveMemberPath(ExpressionSyntax expression, out MemberPath memberPath, [NotNullWhen(false)] out ILocalSymbol unresolvedSymbol, SemanticModel semanticModel)
			{
				unresolvedSymbol = null;

				memberPath = MemberPath.FromExpression(expression, semanticModel, out expression);

				if (memberPath == MemberPath.Empty) return false;

				return TryResolveMemberPath(ref memberPath, out unresolvedSymbol, semanticModel);
			}

			/// <param name="unresolvedSymbol">The new unresolved symbol if the path was only partially resolved.</param>
			private bool TryResolveMemberPath(ref MemberPath memberPath, [NotNullWhen(false)] out ILocalSymbol unresolvedSymbol, SemanticModel semanticModel)
			{
				int i = 0;
				while (memberPath[0] is ILocalSymbol localSymbol && i++ < resolvedVariables.Count)
				{
					if (resolvedVariables.TryGetValue(localSymbol, out var variablePath))
					{
						memberPath = memberPath.RemoveAt(0).InsertRange(0, variablePath);
					}
					else
					{
						break;
					}
				}

				if (memberPath[0] is ILocalSymbol localSymbol2)
				{
					unresolvedSymbol = localSymbol2;
					SetAsUnresolved(unresolvedSymbol);
					return false;
				}
				else
				{
					unresolvedSymbol = null;
					return true;
				}
			}

			private void SetAsUnresolved(ILocalSymbol unresolvedSymbol)
			{
				resolvedVariables.Remove(unresolvedSymbol);

				if (!unresolvedArgumentPaths.ContainsKey(unresolvedSymbol))
					unresolvedArgumentPaths[unresolvedSymbol] = new();
			}

			private void ResolveUnresolvedArgumentPaths(ILocalSymbol localSymbol, SemanticModel semanticModel)
			{
				if (!unresolvedArgumentPaths.TryGetValue(localSymbol, out var list)) return;

				foreach (var tuple in list.ToArray())
				{
					int index = list.IndexOf(tuple);
					var memberPath = tuple.Item1;
					if (TryResolveMemberPath(ref memberPath, out var unresolvedSymbol, semanticModel))
					{
						OnResolveArgumentPath(memberPath, tuple.Item2);
						Debug.WriteLine($"-> {(methodSymbol.Name.Contains("Read") ? "Read" : "Write")}({memberPath})");
					}
					else if (!localSymbol.Equals(unresolvedSymbol, SymbolEqualityComparer.Default))
					{
						unresolvedArgumentPaths[unresolvedSymbol].Add((memberPath, tuple.Item2));
					}
					list.RemoveAt(index);
				}
			}

			private void OnResolveArgumentPath(MemberPath argumentPath, Location location)
			{
				resolvedArgumentPaths.Add(location, argumentPath);
			}

			void AnalyzeCodeBlockEnd(CodeBlockAnalysisContext context)
			{
				foreach (var list in unresolvedArgumentPaths.Values)
				{
					foreach (var tuple in list)
					{
						Debug.WriteLine($"{tuple.Item1} @ {tuple.Item2}");
					}
				}

				CheckSerializedFieldsOrder(context);
			}

			private void CheckSerializedFieldsOrder(CodeBlockAnalysisContext context)
			{
				var methodDecleration = context.CodeBlock as MethodDeclarationSyntax;

				foreach (var pair in resolvedArgumentPaths)
				{
					CheckFieldIsInOrder(pair.Value, pair.Key, context.ReportDiagnostic);
				}

				var location = methodDecleration?.Identifier.GetLocation() ?? context.CodeBlock.ChildNodes().First().GetLocation();
				foreach (var serializableField in serializableFields)
				{
					if (!serializedFields.Contains(serializableField, WildcardSymbolEqualityComparer.Default))
					{
						var diagnostic = Diagnostic.Create(MissingFieldReadWrite, location,
							serializableField);
						context.ReportDiagnostic(diagnostic);
					}
				}
			}

			private void CheckFieldIsInOrder(MemberPath argumentPath, Location location, Action<Diagnostic> reportDiagnostic)
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
				return;
			}
		}
	}
}
