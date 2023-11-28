using CM3D2.Serialization.SourceGenerators.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CM3D2.Serialization.SourceGenerators
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CM3D2SerializableCodeFixProvider)), Shared]
	public class CM3D2SerializableCodeFixProvider : CodeFixProvider 
	{
		private const string title = "Implement interface using CM3D2 Serialization pattern";

		public sealed override ImmutableArray<string> FixableDiagnosticIds
		{
			get { return ImmutableArray.Create("CS0535"); }
		}

		public sealed override FixAllProvider GetFixAllProvider()
		{
			return WellKnownFixAllProviders.BatchFixer;
		}

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var root =
			  await context.Document.GetSyntaxRootAsync(context.CancellationToken)
			  .ConfigureAwait(false);

			var diagnostic = context.Diagnostics.First();
			var diagnosticSpan = diagnostic.Location.SourceSpan;

			var baseType = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<BaseTypeSyntax>().First();
			if (baseType.Type.ToString() != nameof(ICM3D2Serializable))
				return;

			var typeDecleration = baseType.AncestorsAndSelf().OfType<TypeDeclarationSyntax>().First();

			// Register a code action that will invoke the fix.
			context.RegisterCodeFix(
				CodeAction.Create(title, c =>
				ImplementMethodsAsync(context.Document, typeDecleration, c), equivalenceKey: title), diagnostic);
		}

		private async Task<Document> ImplementMethodsAsync(Document document,
		  TypeDeclarationSyntax typeDecleration,
		  CancellationToken cancellationToken)
		{
			AutoCM3D2SerializableGenerator.GenerateMethodSource(typeDecleration, out var readerCode, out var writerCode);

			var readMethod = SyntaxFactory.ParseMemberDeclaration(
				$"void ICM3D2Serializable.ReadWith(ICM3D2Reader reader) {{ {readerCode.ToString().Trim()} }}")
				.WithAdditionalAnnotations(Formatter.Annotation);

			var writeMethod = SyntaxFactory.ParseMemberDeclaration(
				$"void ICM3D2Serializable.WriteWith(ICM3D2Writer writer) {{ {writerCode.ToString().Trim()} }}")
				.WithAdditionalAnnotations(Formatter.Annotation);
			
			var root = await document.GetSyntaxRootAsync();
			var newRoot = root.ReplaceNode(typeDecleration, typeDecleration.AddMembers(readMethod, writeMethod));

			var newDocument = document.WithSyntaxRoot(newRoot);

			return newDocument;
		}
	}
}