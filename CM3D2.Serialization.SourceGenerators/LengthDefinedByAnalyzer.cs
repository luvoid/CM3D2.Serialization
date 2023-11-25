using CM3D2.Serialization.SourceGenerators.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;

namespace CM3D2.Serialization.SourceGenerators
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class LengthDefinedByAnalyzer : DiagnosticAnalyzer
	{
		const string k_Category = "CM3D2.Serialization.LengthDefinedBy";

		private static readonly DiagnosticDescriptor MissingLengthDefinedByAttributeRule = new(
			id: "CM3D2Serialization050",
			title: "Missing LengthDefinedBy Attribute",
			messageFormat: "Add the 'LengthDefinedBy' attribute to the field",
			category: k_Category,
			defaultSeverity: DiagnosticSeverity.Info,
			isEnabledByDefault: true
		);

		private static readonly DiagnosticDescriptor InvalidLengthDefinedByRule = new(
			id: "CM3D2Serialization051",
			title: "Invalid LengthDefinedBy Attribute",
			messageFormat: "Invalid length definition ({0})",
			category: k_Category,
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true
		);

		private static readonly DiagnosticDescriptor WrongLengthSetRule = new(
			id: "CM3D2Serialization052",
			title: "Wrong Length Set",
			messageFormat: "The wrong field or propertry was used to set the length of '{0}' (expected '{1}', got '{2}')",
			category: k_Category,
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true
		);

		private static readonly DiagnosticDescriptor WrongLengthValidatedRule = new(
			id: "CM3D2Serialization053",
			title: "Wrong Length Validated",
			messageFormat: "The wrong field or propertry was used to validate the length of '{0}' (expected '{1}', got '{2}')",
			category: k_Category,
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true
		);

		private static readonly DiagnosticDescriptor LengthNotSetRule = new(
			id: "CM3D2Serialization054",
			title: "Length Not Set",
			messageFormat: "The length of '{0}' must be set (or validated) before reading into it",
			category: k_Category,
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true
		);

		private static readonly DiagnosticDescriptor LengthNotValidatedRule = new(
			id: "CM3D2Serialization055",
			title: "Length Not Validated",
			messageFormat: "The length of '{0}' must be validated (or set) before writing it",
			category: k_Category,
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true
		);

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
			MissingLengthDefinedByAttributeRule,
			InvalidLengthDefinedByRule,
			WrongLengthSetRule,
			WrongLengthValidatedRule,
			LengthNotSetRule,
			LengthNotValidatedRule
		);

		public override void Initialize(AnalysisContext context)
		{
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
			context.EnableConcurrentExecution();

			context.RegisterSyntaxNodeAction(AnalyzeFieldDeclarationSyntax, SyntaxKind.FieldDeclaration);

			context.RegisterCodeBlockStartAction<SyntaxKind>(StartAnalyzeCodeBlock);
		}

		private static void AnalyzeFieldDeclarationSyntax(SyntaxNodeAnalysisContext context)
		{
			var fieldDeclaration = context.Node as FieldDeclarationSyntax;
			var fieldSymbol = context.SemanticModel.LookupSymbols(
				fieldDeclaration.GetLocation().SourceSpan.Start,
				name: fieldDeclaration.Declaration.Variables[0].Identifier.ValueText
			)[0];

			//var fieldType = context.SemanticModel.GetTypeInfo(fieldDecleration.Declaration.Type).Type;
			var fieldType = context.SemanticModel.GetSpeculativeTypeInfo(
				fieldDeclaration.GetLocation().SourceSpan.Start,
				fieldDeclaration.Declaration.Type,
				SpeculativeBindingOption.BindAsTypeOrNamespace
			).Type;
			if (fieldType == null) return;
			if (fieldType.AllInterfaces.FirstOrDefault((i) => i.Name == "ILengthDefinedCollection") == null) return;

			GetLengthDefiningSymbol(fieldSymbol, context.SemanticModel, out var diagnostic, fieldDeclaration);
			if (diagnostic != null)
			{
				context.ReportDiagnostic(diagnostic);
			}
		}

		private static ISymbol GetLengthDefiningSymbol(ISymbol member, SemanticModel semanticModel, out Diagnostic diagnostic, MemberDeclarationSyntax memberDeclaration = null)
		{
			var attribute = member.GetAttributes().FirstOrDefault((a) => a.AttributeClass.Name == "LengthDefinedByAttribute");
			if (attribute == null)
			{
				diagnostic = Diagnostic.Create(MissingLengthDefinedByAttributeRule, memberDeclaration?.GetLocation());
				return null;
			}

			var attributeSyntax = attribute.ApplicationSyntaxReference.GetSyntax() as AttributeSyntax;
			var attributeArg = attribute.ConstructorArguments.FirstOrDefault();
			if (attributeArg.Kind == TypedConstantKind.Error)
			{
				diagnostic = Diagnostic.Create(InvalidLengthDefinedByRule, attributeSyntax.GetLocation(), "no field specified");
				return null;
			}

			var attributeArgSyntax = attributeSyntax.ArgumentList.Arguments[0];
			var namedSymbols = semanticModel.LookupSymbols(
				attributeArgSyntax.GetLocation().SourceSpan.Start,
				name: (string)attributeArg.Value
			);
			var foundSymbol = namedSymbols.FirstOrDefault((s) => (s is IFieldSymbol fs || s is IPropertySymbol));
			if (foundSymbol == null)
			{
				diagnostic = Diagnostic.Create(InvalidLengthDefinedByRule, attributeArgSyntax.GetLocation(),
					$"field or property '{attributeArg.Value}' does not exist in the current context");
				return null;
			}

			if ((  (foundSymbol is IFieldSymbol    fs) ? fs.Type.Name
				 : (foundSymbol is IPropertySymbol ps) ? ps.Type.Name
				 : null
				) != typeof(int).Name)
			{
				diagnostic = Diagnostic.Create(InvalidLengthDefinedByRule, attributeArgSyntax.GetLocation(),
					$"field or property '{attributeArg.Value}' must be of type 'int'");
				return foundSymbol;
			}

			diagnostic = null;
			return foundSymbol;
		}

		private static ISymbol GetLengthDefiningSymbol(ISymbol member, SemanticModel semanticModel)
		{
			return GetLengthDefiningSymbol(member, semanticModel, out _);
		}

		public void StartAnalyzeCodeBlock(CodeBlockStartAnalysisContext<SyntaxKind> context)
		{
			CodeBlockAnalyzer analyzer = new();
			context.RegisterSyntaxNodeAction(analyzer.AnalyzeInvocationExpressionSyntax, SyntaxKind.InvocationExpression);
		}

		private class CodeBlockAnalyzer
		{
			private readonly Dictionary<ISymbol, List<CachedInvocation>> cachedInvocations = new(SymbolEqualityComparer.Default);

			private readonly struct CachedInvocation
			{
				public readonly InvocationExpressionSyntax Syntax;
				public readonly ISymbol Target;
				public readonly IMethodSymbol Method;

				public CachedInvocation(InvocationExpressionSyntax syntax, ISymbol target, IMethodSymbol method)
				{
					Syntax = syntax;
					Target = target;
					Method = method;
				}
			}

			public void AnalyzeInvocationExpressionSyntax(SyntaxNodeAnalysisContext context)
			{
				var invocationExpression = context.Node as InvocationExpressionSyntax;
				if (invocationExpression.ArgumentList.Arguments.Count < 1) return;

				if (!invocationExpression.TryGetSpeculativeSymbol(context.SemanticModel, out IMethodSymbol methodSymbol)) return;

				AnalyzeILengthDefinedCollectionMethodInvocation(context, invocationExpression, methodSymbol);


				if (methodSymbol.Name != "Read" && methodSymbol.Name != "Write") return;
				if (invocationExpression.ArgumentList.Arguments.Count < 1) return;

				//var argumentSymbol = context.SemanticModel.GetSymbolInfo(invocationExpression.ArgumentList.Arguments[0]).Symbol;
				var argumentExpression = invocationExpression.ArgumentList.Arguments[0].Expression;
				var argumentSymbol = argumentExpression.GetSpeculativeSymbol(context.SemanticModel);
				var argumentType = argumentExpression.GetSpeculativeTypeSymbol(context.SemanticModel);
				if (!argumentType.Implements("ILengthDefinedCollection")) return;

				if (cachedInvocations.TryGetValue(argumentSymbol, out var previousInvocations))
				{
					foreach (var prevInvoke in previousInvocations)
					{
						if (prevInvoke.Method.Name == "SetLength" || prevInvoke.Method.Name == "ValidateLength")
							return; // All good
					}
				}

				// The length was not set or validated
				DiagnosticDescriptor descriptor = methodSymbol.Name switch
				{
					"Read" => LengthNotSetRule,
					"Write" => LengthNotValidatedRule,
				};
				var diagnostic = Diagnostic.Create(descriptor, invocationExpression.GetLocation(), argumentSymbol.Name);
				context.ReportDiagnostic(diagnostic);
			}

			private void AnalyzeILengthDefinedCollectionMethodInvocation(
				SyntaxNodeAnalysisContext context, InvocationExpressionSyntax invocationExpression, IMethodSymbol methodSymbol)
			{
				if (invocationExpression.Expression is not MemberAccessExpressionSyntax memberAccess) return;

				if (!memberAccess.Expression.TryGetSpeculativeTypeSymbol(context.SemanticModel, out ITypeSymbol targetType)) return;

				if (!targetType.Implements("ILengthDefinedCollection")) return;

				if (!memberAccess.Expression.TryGetSpeculativeSymbol(context.SemanticModel, out ISymbol targetSymbol)) return;


				if (!cachedInvocations.TryGetValue(targetSymbol, out var list))
					cachedInvocations[targetSymbol] = list = new();
				list.Add(new CachedInvocation(invocationExpression, targetSymbol, methodSymbol));


				if (invocationExpression.ArgumentList.Arguments.Count < 1) return;
				if (methodSymbol.Name != "SetLength" && methodSymbol.Name != "ValidateLength") return;

				var lengthDefiningSymbol = GetLengthDefiningSymbol(targetSymbol, context.SemanticModel);
				if (lengthDefiningSymbol == null) return;

				var argumentSymbol = invocationExpression.ArgumentList.Arguments[0].Expression.GetSpeculativeSymbol(context.SemanticModel);
				if (!SymbolEqualityComparer.Default.Equals(lengthDefiningSymbol, argumentSymbol))
				{
					DiagnosticDescriptor descriptor = methodSymbol.Name switch
					{
						"SetLength"      => WrongLengthSetRule,
						"ValidateLength" => WrongLengthValidatedRule,
					};

					var diagnostic = Diagnostic.Create(descriptor, invocationExpression.ArgumentList.Arguments[0].GetLocation(),
						targetSymbol.Name, lengthDefiningSymbol.Name, argumentSymbol.Name);
					context.ReportDiagnostic(diagnostic);
				}
			}
		}
	}
}
