using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;

namespace CM3D2.Serialization.SourceGenerators
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class FileVersionAnalyzer : DiagnosticAnalyzer
	{
		private static readonly DiagnosticDescriptor FileVersionConstraintRule = new DiagnosticDescriptor(
			id: "CM3D2Serialization060",
			title: "File Version Constraint",
			messageFormat: "Minumum File Version = {0}",
			category: "CM3D2.Serialization.FileVersion",
			defaultSeverity: DiagnosticSeverity.Info, 
			isEnabledByDefault: true
		);

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(FileVersionConstraintRule); } }

		public override void Initialize(AnalysisContext context)
		{
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
			context.EnableConcurrentExecution();

			context.RegisterSyntaxNodeAction(AnalyzeIdentifierNameSyntax, SyntaxKind.IdentifierName);
		}

		private static void AnalyzeIdentifierNameSyntax(SyntaxNodeAnalysisContext context)
		{
			var identifierName = context.Node as IdentifierNameSyntax;
			var symbol = context.SemanticModel.GetSymbolInfo(identifierName).Symbol;

			// Find just those named type symbols with names containing lowercase letters.
			if (symbol is IFieldSymbol)
			{
				var attributes = symbol.GetAttributes();
				foreach (var attribute in attributes)
				{
					if (attribute.AttributeClass.Name == "FileVersionConstraintAttribute")
					{
						var attributeArgs = attribute.ConstructorArguments;
						var diagnostic = Diagnostic.Create(FileVersionConstraintRule, identifierName.GetLocation(), attributeArgs[0].Value);
						context.ReportDiagnostic(diagnostic);
					}
				}
			}
		}
	}
}
