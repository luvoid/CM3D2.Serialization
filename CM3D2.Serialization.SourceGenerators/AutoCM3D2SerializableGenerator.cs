using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Dynamic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace CM3D2.Serialization.SourceGenerators
{
	[Generator(LanguageNames.CSharp)]
	public class AutoCM3D2SerializableGenerator : IIncrementalGenerator
	{
		public void Initialize(GeneratorInitializationContext context)
		{
			// Register a factory that can create our custom syntax receiver
			context.RegisterForSyntaxNotifications(() => new AutoCM3D2SerializableSyntaxReciever());
		}

		public void Initialize(IncrementalGeneratorInitializationContext context)
		{
			IncrementalValuesProvider<AutoCM3D2SerializeTransformResult> implementation = context.SyntaxProvider.CreateSyntaxProvider(ShouldTransformNode, Transform);

			IncrementalValueProvider<ImmutableArray<AutoCM3D2SerializeTransformResult>> collected = implementation.Collect();

			context.RegisterSourceOutput(collected, OnRegisterSourceOutput);
		}

		public static void OnRegisterSourceOutput(SourceProductionContext context, ImmutableArray<AutoCM3D2SerializeTransformResult> sources)
		{
			foreach (var source in sources)
			{
				foreach (var diagnostic in source.Diagnostics)
				{
					context.ReportDiagnostic(diagnostic);
				}
				context.AddSource(source.Filename, source.SourceText);
			}
		}

		public static bool ShouldTransformNode(SyntaxNode syntaxNode, System.Threading.CancellationToken token = default)
		{
			if (!(syntaxNode is TypeDeclarationSyntax tds)) return false;

			if (!(	   tds.Keyword.ValueText == "class" 
					|| tds.Keyword.ValueText == "struct")
				) return false;

			if (!(	tds.AttributeLists.Where(
						(al) => al.Attributes.Where(
							(a) => a.Name.ToString() == "AutoCM3D2Serializable"
						).Count() > 0
					).Count() > 0
				)) return false;

			return true;
		}

		public void Execute(GeneratorExecutionContext context)
		{
			// the generator infrastructure will create a receiver and populate it
			// we can retrieve the populated instance via the context
			AutoCM3D2SerializableSyntaxReciever syntaxReceiver = context.SyntaxReceiver as AutoCM3D2SerializableSyntaxReciever;

			TypeDeclarationSyntax userType = syntaxReceiver.TypeToAugment;
			if (userType is null) return;

			string source = GetImplementationSource(userType, context.ReportDiagnostic);

			// Add the source code to the compilation
			context.AddSource($"{userType.Identifier}.g.cs", source);
		}
	
		public static AutoCM3D2SerializeTransformResult Transform(GeneratorSyntaxContext context, System.Threading.CancellationToken cancellationToken = default)
		{
			if (!(context.Node is TypeDeclarationSyntax userType)) return AutoCM3D2SerializeTransformResult.None;

			List<Diagnostic> diagnostics = new List<Diagnostic>();
			string source = GetImplementationSource(userType, diagnostics.Add);

			return new AutoCM3D2SerializeTransformResult(
				$"{userType.Identifier}.g.cs",
				SourceText.From(source, Encoding.UTF8),
				diagnostics
			);
		}

		static string GetImplementationSource(TypeDeclarationSyntax userType, Action<Diagnostic> reportDiagnostic)
		{
			// Check if the userType is marked as partial
			if (!userType.Modifiers.Any(SyntaxKind.PartialKeyword))
			{
				reportDiagnostic(TypeDeclarationNotAutoSerializableDiagnostic.Create(
					userType.Identifier,
					$"because it is not marked as partial",
					Location.Create(userType.SyntaxTree,
						new TextSpan(userType.Modifiers.First().SpanStart, userType.Identifier.Span.End - userType.Modifiers.First().SpanStart)
					)
				));
			}

			// Collect all the fields
			var fields = from member in userType.Members 
			             where member.IsKind(SyntaxKind.FieldDeclaration)
			             select member as FieldDeclarationSyntax;

			StringBuilder readerCode = new StringBuilder();
			StringBuilder writerCode = new StringBuilder();

			foreach (var field in fields)
			{
				// Don't serialize static variables
				if (field.Modifiers.Any(SyntaxKind.StaticKeyword)) continue;

				foreach (var variable in field.Declaration.Variables)
				{
					if (field.Modifiers.Any(SyntaxKind.ReadOnlyKeyword))
					{
						reportDiagnostic(FieldNotAutoSerializableDiagnostic.Create(
							variable.Identifier,
							"because it is readonly",
							field.GetLocation()
						));
					}

					readerCode.AppendLine($"\t\treader.Read(out {variable.Identifier});");
					writerCode.AppendLine($"\t\twriter.Write({variable.Identifier});");
				}
			}

			// add the generated implementation to the compilation
			string source =
$@"{userType.Modifiers} {userType.Keyword} {userType.Identifier} {userType.BaseList}
{{
	void ICM3D2Serializable.ReadWith(ICM3D2Reader reader)
	{{
		{readerCode.ToString().Trim()}
	}}

	void ICM3D2Serializable.WriteWith(ICM3D2Writer writer)
	{{
		{writerCode.ToString().Trim()}
	}}
}}";

			CSharpSyntaxNode currentNode = userType;
			while (currentNode.Parent is TypeDeclarationSyntax typeParent)
			{
				if (!typeParent.Modifiers.Any(SyntaxKind.PartialKeyword))
				{
					reportDiagnostic(TypeDeclarationNotAutoSerializableDiagnostic.Create(
						userType.Identifier,
						$"because the containing type '{typeParent.Identifier}' is not marked as partial",
						Location.Create(userType.SyntaxTree, 
							new TextSpan(userType.Modifiers.First().SpanStart, userType.Identifier.Span.End - userType.Modifiers.First().SpanStart)
						)
					));
				}

				source =
$@"{typeParent.Modifiers} {typeParent.Keyword} {typeParent.Identifier} {typeParent.BaseList}
{{
	{source.Replace("\n", "\n\t")}
}}";
				currentNode = typeParent;
			}
			while (currentNode.Parent is NamespaceDeclarationSyntax namespaceParent)
			{
				source =
$@"{namespaceParent.NamespaceKeyword} {namespaceParent.Name}
{{
	{source.Replace("\n", "\n\t")}
}}";
				currentNode = namespaceParent;
			}

			source = 
$@"// <auto-generated/>
using CM3D2.Serialization;

{source}
";
			return source;
		}	
	}

	public class AutoCM3D2SerializableSyntaxReciever : ISyntaxReceiver
	{
		public TypeDeclarationSyntax TypeToAugment { get; private set; }

		public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
		{
			// Business logic to decide what we're interested in goes here
			if (AutoCM3D2SerializableGenerator.ShouldTransformNode(syntaxNode))
			{
				TypeToAugment = syntaxNode as TypeDeclarationSyntax;
			}
		}
	}

	public static class TypeDeclarationNotAutoSerializableDiagnostic
	{
		private static DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
			id: "CM3D2Serialization040",
			title: "Unserializable Type Declaration",
			messageFormat: "Cannot auto serialize type declaration '{0}'{1}",
			category: "CM3D2.Serialization.AutoCM3D2Serializable",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true
		);

		public static Diagnostic Create(object type, string reason, Location location = null)
		{
			if (string.IsNullOrWhiteSpace(reason))
			{
				reason = "";
			}
			else
			{
				reason = " " + reason;
			}
			return Diagnostic.Create(descriptor, location, type, reason);
		}
	}

	public static class FieldNotAutoSerializableDiagnostic
	{
		private static DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
			id: "CM3D2Serialization041",
			title: "Unserializable Field",
			messageFormat: "Cannot auto serialize field '{0}'{1}",
			category: "CM3D2.Serialization.AutoCM3D2Serializable",
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true
		);

		public static Diagnostic Create(object field, string reason, Location location = null)
		{
			if (string.IsNullOrWhiteSpace(reason))
			{
				reason = "";
			}
			else
			{
				reason = " " + reason;
			}
			return Diagnostic.Create(descriptor, location, field, reason);
		}
	}

	public class AutoCM3D2SerializeTransformResult
	{
		public string Filename;
		public SourceText SourceText;
		public List<Diagnostic> Diagnostics;

		public static readonly AutoCM3D2SerializeTransformResult None = new AutoCM3D2SerializeTransformResult(null, null, null);

		public AutoCM3D2SerializeTransformResult(string filename, SourceText sourceText, List<Diagnostic> diagnostics)
		{
			this.Filename = filename;
			this.SourceText = sourceText;
			this.Diagnostics = diagnostics;
		}
	}
}
