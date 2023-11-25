using CM3D2.Serialization.SourceGenerators.Extensions;
using CM3D2.Serialization.Types;
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
using System.Diagnostics.CodeAnalysis;
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
		const string k_Category = "CM3D2.Serialization.CM3D2Serializable";

		private static readonly DiagnosticDescriptor NonSerializableField = new(
			id: "CM3D2Serialization010",
			title: "Non-Serializable Field",
			messageFormat: "The field '{0}' is of type '{1}' which cannot be serialized",
			category: k_Category,
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true
		);

		private static readonly DiagnosticDescriptor InaccessibleField = new (
			id: "CM3D2Serialization011",
			title: "Inaccessible Field in Deep-Serialized Type",
			messageFormat: "Instance field '{0}' of the deep-serializable type '{1}' is inaccessible, preventing deep-serialization",
			category: k_Category,
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true
		);

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
			NonSerializableField,
			InaccessibleField
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
			private readonly ConcurrentBag<IFieldSymbol> nonSerializableFields = new();

			public CompilationAnalyzer(AnalysisContext context)
			{
				//context.RegisterSymbolAction(AnalyzeFieldSymbol, SymbolKind.Field);
				context.RegisterSemanticModelAction(AnalyzeSemanticModel);
			}

			public CompilationAnalyzer(CompilationStartAnalysisContext context)
			{
				//context.RegisterSymbolAction(AnalyzeFieldSymbol, SymbolKind.Field);
				context.RegisterSymbolAction(AnalyzeNamedTypeSymbol, SymbolKind.NamedType);
				context.RegisterSemanticModelAction(AnalyzeSemanticModel);
			}

			void AnalyzeNamedTypeSymbol(SymbolAnalysisContext context)
			{
				if (context.Symbol is not INamedTypeSymbol typeSymbol)
					throw new ArgumentException($"Expected symbol of type {typeof(INamedTypeSymbol)}", nameof(context.Symbol));

				if (!typeSymbol.IsExplicitCM3D2Serializable(out bool isDeepSerializable))
					return;

				foreach (var fieldSymbol in typeSymbol.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
				{
					Debug.WriteLine($"{typeSymbol.Name} : {fieldSymbol.Name} (readonly: {fieldSymbol.IsReadOnly})");

					if (fieldSymbol.IsConst) continue;
					if (fieldSymbol.IsStatic) continue;

					if (fieldSymbol.HasAttribute(typeof(NonSerializedAttribute))) continue;

					if (isDeepSerializable)
					{
						if (fieldSymbol.DeclaredAccessibility != Accessibility.Public
							&& (fieldSymbol.AssociatedSymbol == null
								|| fieldSymbol.AssociatedSymbol.DeclaredAccessibility != Accessibility.Public))
						{
							var diagnostic = Diagnostic.Create(InaccessibleField, fieldSymbol.Locations.Single(),
								fieldSymbol.Name, typeSymbol.Name);
							context.ReportDiagnostic(diagnostic);
						}
					}

					if (fieldSymbol.IsDeepSerialized()) continue;
					if (fieldSymbol.Type.IsCM3D2Serializable()) continue;


					// else
					nonSerializableFields.Add(fieldSymbol);
				}
			}

			private void AnalyzeSemanticModel(SemanticModelAnalysisContext context)
			{
				while (nonSerializableFields.TryTake(out var fieldSymbol))
				{
					var location = fieldSymbol.Locations.Single();
					var position = location.SourceSpan.Start;
					var diagnostic = Diagnostic.Create(NonSerializableField, location,
						fieldSymbol.Name,
						fieldSymbol.Type.ToMinimalDisplayString(context.SemanticModel, position, SymbolDisplayFormat.CSharpErrorMessageFormat)
					);
					context.ReportDiagnostic(diagnostic);
				}
			}

		}
	}
}
