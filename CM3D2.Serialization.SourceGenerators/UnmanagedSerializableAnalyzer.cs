using CM3D2.Serialization.SourceGenerators.Extensions;
using CM3D2.Serialization.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace CM3D2.Serialization.SourceGenerators
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class UnmanagedSerializableAnalyzer : DiagnosticAnalyzer
	{
		const string k_Category = "CM3D2.Serialization.UnmanagedSerializable";

		private static readonly DiagnosticDescriptor InvalidInterfaceImplementation = new(
			id: "CM3D2Serialization020",
			title: "Invalid Interface on Unmanaged-Serializable Struct",
			messageFormat: "The struct '{0}' implements interface '{1}' which is not allowed on an explicitly unmanaged-serializable struct",
			category: k_Category,
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true
		);

		private static readonly DiagnosticDescriptor InvalidStructAttribute = new(
			id: "CM3D2Serialization021",
			title: "Invalid Attribute on Unmanaged-Serializable Struct",
			messageFormat: "The struct '{0}' is marked with [{1}] which is not allowed on an explicitly unmanaged-serializable struct",
			category: k_Category,
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true
		);

		private static readonly DiagnosticDescriptor MissingExplicitStructAttribute = new(
			id: "CM3D2Serialization022",
			title: "Invalid Attribute on Unmanaged-Serializable Struct",
			messageFormat: "The struct '{0}' is not explicitly marked with [{1}]. Serialization may not be as expected.",
			category: k_Category,
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true
		);

		private static readonly DiagnosticDescriptor NonUnmanagedSerializableField = new(
			id: "CM3D2Serialization023",
			title: "Non-Unmanaged-Serializable Field",
			messageFormat: "The field '{0}' is of type '{1}' which is not unmanaged-serializable, and cannot be in an explicitly unmanaged-serializable struct",
			category: k_Category,
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true
		);

		private static readonly DiagnosticDescriptor InaccessibleField = new(
			id: "CM3D2Serialization024",
			title: "Inaccessible Field in Unmanaged-Serializable Struct",
			messageFormat: "The field '{0}' of the unmanaged-serializable type '{1}' is not publicly accessible. Serialized value cannot be get or set.",
			category: k_Category,
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true
		);

		private static readonly DiagnosticDescriptor ImplicitUnmanagedSerializableField = new(
			id: "CM3D2Serialization025",
			title: "Implicit Unmanaged-Serializable Field",
			messageFormat: $"The field '{{0}}' is of type '{{1}}' which is not explicitly marked with [{nameof(UnmanagedSerializableAttribute)}]. Serialization may not be as expected.",
			category: k_Category,
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true
		);

		private static readonly DiagnosticDescriptor InvalidFieldAttribute = new(
			id: "CM3D2Serialization026",
			title: "Invalid Attribute on Unmanaged-Serializable Field",
			messageFormat: "The field '{0}' is marked with [{1}] which is not allowed in an explicitly unmanaged-serializable struct",
			category: k_Category,
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true
		);

		private static readonly DiagnosticDescriptor MissingExplicitFieldAttribute = new(
			id: "CM3D2Serialization027",
			title: "Missing Explicit Attribute on Unmanaged-Serializable Field",
			messageFormat: "The field '{0}' is not explicitly marked with [{1}]. Serialization may not be as expected.",
			category: k_Category,
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true
		);


		private static readonly DiagnosticDescriptor NonUnmanagedSerializableTypeArgument = new(
			id: "CM3D2Serialization028",
			title: "Non-Unmanaged-Serializable Type Argument",
			//messageFormat: "The type '{0}' cannot be used as type parameter '{1}' in the generic type or method '{2}'. The type argument used for '{1}' must be unmanaged-serializable.",
			messageFormat: "The type '{0}' must be an unmanaged-serializable value type in order to use it as parameter '{1}' in the generic type or method '{2}'",
			category: k_Category,
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true
		);

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
			InvalidInterfaceImplementation,
			InvalidStructAttribute,
			MissingExplicitStructAttribute,
			NonUnmanagedSerializableField,
			InaccessibleField,
			ImplicitUnmanagedSerializableField,
			InvalidFieldAttribute,
			MissingExplicitFieldAttribute,
			NonUnmanagedSerializableTypeArgument
		);

		private static readonly ImmutableArray<Type> invalidStructAttributes = ImmutableArray.Create(
			typeof(DeepSerializableAttribute),
			typeof(AutoCM3D2SerializableAttribute)
		);

		private static readonly ImmutableArray<Type> invalidFieldAttributes = ImmutableArray.Create(
			typeof(DeepSerializableAttribute),
			typeof(AutoCM3D2SerializableAttribute)
		);

		public override void Initialize(AnalysisContext context)
		{
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
			context.EnableConcurrentExecution();

			context.RegisterCompilationStartAction(OnCompilationStart);
			//var analyzer = new CompilationAnalyzer(context);
		}

		void OnCompilationStart(CompilationStartAnalysisContext context)
		{
			var analyzer = new CompilationAnalyzer(context);
		}

		private class CompilationAnalyzer
		{
			private readonly ConcurrentBag<(IFieldSymbol, DiagnosticDescriptor)> fieldDiagnostics = new();

			public CompilationAnalyzer(AnalysisContext context)
			{
				//context.RegisterSymbolAction(AnalyzeFieldSymbol, SymbolKind.Field);
				context.RegisterSemanticModelAction(AnalyzeSemanticModel);
			}

			public CompilationAnalyzer(CompilationStartAnalysisContext context)
			{
				context.RegisterSymbolAction(AnalyzeNamedTypeSymbol, SymbolKind.NamedType);
				context.RegisterSymbolAction(AnalyzeFieldSymbol, SymbolKind.Field);
				context.RegisterSemanticModelAction(AnalyzeSemanticModel);
			}

			void AnalyzeNamedTypeSymbol(SymbolAnalysisContext context)
			{
				if (context.Symbol is not INamedTypeSymbol typeSymbol)
					throw new ArgumentException($"Expected symbol of type {typeof(INamedTypeSymbol)}", nameof(context.Symbol));

				if (!typeSymbol.IsExplicitCM3D2Serializable(out bool isDeepSerializable, out bool isExplicitUnmanagedSerializable,
					out bool isInterfaceSerializable, out var implementedInterfaceSymbol))
					return;

				if (!isExplicitUnmanagedSerializable) return;

				if (isInterfaceSerializable)
				{
					context.ReportDiagnostic(
						Diagnostic.Create(InvalidInterfaceImplementation, typeSymbol.Locations.First(),
							typeSymbol.Name, implementedInterfaceSymbol.Name)
					);
				}

				CheckForInvalidAttributes(context.ReportDiagnostic, InvalidStructAttribute,
					typeSymbol, invalidStructAttributes);

				foreach (var fieldSymbol in typeSymbol.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
				{
					if (fieldSymbol.IsConst) continue;
					if (fieldSymbol.IsStatic) continue;

					CheckForInvalidAttributes(context.ReportDiagnostic, InvalidFieldAttribute,
							fieldSymbol, invalidFieldAttributes);

					if (fieldSymbol.DeclaredAccessibility != Accessibility.Public
						&& (fieldSymbol.AssociatedSymbol == null
							|| fieldSymbol.AssociatedSymbol.DeclaredAccessibility != Accessibility.Public))
					{
						var diagnostic = Diagnostic.Create(InaccessibleField, fieldSymbol.Locations.Single(),
							fieldSymbol.Name, typeSymbol.Name);
						context.ReportDiagnostic(diagnostic);
					}

					if (!fieldSymbol.Type.IsUnmanagedType)
					{
						fieldDiagnostics.Add((fieldSymbol, NonUnmanagedSerializableField));
					}
				}
			}

			void AnalyzeFieldSymbol(SymbolAnalysisContext context)
			{
				if (context.Symbol is not IFieldSymbol fieldSymbol)
					throw new ArgumentException($"Expected symbol of type {typeof(IFieldSymbol)}", nameof(context.Symbol));

				if (fieldSymbol.IsConst) return;
				if (fieldSymbol.IsStatic) return;

				CheckForInvalidTypeArguments(context, fieldSymbol.Type);
			}

			void AnalyzeSemanticModel(SemanticModelAnalysisContext context)
			{
				while (fieldDiagnostics.TryTake(out var tuple))
				{
					var (fieldSymbol, diagnosticDescriptor) = tuple;
					var location = fieldSymbol.Locations.Single();
					var position = location.SourceSpan.Start;
					var diagnostic = Diagnostic.Create(diagnosticDescriptor, location,
						fieldSymbol.Name,
						fieldSymbol.Type.ToMinimalDisplayString(context.SemanticModel, position, SymbolDisplayFormat.CSharpErrorMessageFormat)
					);
					context.ReportDiagnostic(diagnostic);
				}
			}

			private void CheckForInvalidAttributes(Action<Diagnostic> reportDiagnostics, DiagnosticDescriptor diagnosticDescriptor,
				ISymbol symbol, ImmutableArray<Type> invalidAttributes)
			{
				foreach (var attribute in symbol.GetAttributes())
				{
					var attributeTypeSymbol = attribute.AttributeClass;
					if (attributeTypeSymbol == null) continue;
					if (invalidAttributes.Any(t => attributeTypeSymbol.IsType(t)))
					{
						ReportInvalidAttribute(reportDiagnostics, diagnosticDescriptor, symbol, attribute);
					}
				}
			}

			private void ReportInvalidAttribute(Action<Diagnostic> reportDiagnostics, DiagnosticDescriptor diagnosticDescriptor,
				ISymbol symbol, AttributeData attribute)
			{
				var attributeSyntax = attribute.ApplicationSyntaxReference?.GetSyntax();
				reportDiagnostics(
					Diagnostic.Create(diagnosticDescriptor, attributeSyntax?.GetLocation() ?? symbol.Locations.First(),
						symbol.Name, attributeSyntax?.ToString() ?? attribute.AttributeClass.Name)
				);
			}

			private static void CheckForInvalidTypeArguments(SymbolAnalysisContext context, ISymbol genericTypeOrMethod)
			{
				ImmutableArray<ITypeParameterSymbol> typeParameters;
				ImmutableArray<ITypeSymbol> typeArguments;

				if (genericTypeOrMethod is INamedTypeSymbol genericType)
				{
					typeParameters = genericType.TypeParameters;
					typeArguments = genericType.TypeArguments;
				}
				else if (genericTypeOrMethod is IMethodSymbol genericMethod)
				{
					typeParameters = genericMethod.TypeParameters;
					typeArguments = genericMethod.TypeArguments;
				}
				else
				{
					throw new ArgumentException($"Expected symbol of type {typeof(INamedTypeSymbol)} or {typeof(IMethodSymbol)}", nameof(genericTypeOrMethod));
				}

				for (int i = 0; i < typeParameters.Length; i++)
				{
					var typeParameter = typeParameters[i];
					if (!typeParameter.HasAttribute(typeof(UnmanagedSerializableAttribute))) continue;

					var typeArgument = typeArguments[i];
					if (typeArgument.HasAttribute(typeof(UnmanagedSerializableAttribute))) continue;
					if (!typeArgument.IsUnmanagedType || typeArgument.IsExplicitCM3D2Serializable())
					{
						context.ReportDiagnostic(
							Diagnostic.Create(NonUnmanagedSerializableTypeArgument, context.Symbol.Locations.First(),
								typeArgument.Name, typeParameter.Name, genericTypeOrMethod.Name)
						);
					}
				}
			}
		}
	}
}
