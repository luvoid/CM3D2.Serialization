using CM3D2.Serialization.SourceGenerators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CM3D2.Serialization.SourceGenerators
{
	[Generator(LanguageNames.CSharp)]
	public class FileVersionConstraintCommentGenerator : IIncrementalGenerator
	{
		class SyntaxReciever : ISyntaxReceiver
		{
			public FieldDeclarationSyntax FieldToAugment { get; private set; }

			public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
			{
				// Business logic to decide what we're interested in goes here
				if (ShouldTransformNode(syntaxNode))
				{
					FieldToAugment = syntaxNode as FieldDeclarationSyntax;
				}
			}
		}

		class TransformResult
		{
			public SyntaxTree OldSyntaxTree => OldField.SyntaxTree as CSharpSyntaxTree;
			public FieldDeclarationSyntax OldField;
			public FieldDeclarationSyntax NewField;
			public List<Diagnostic> Diagnostics;

			public static readonly TransformResult None = new TransformResult(null, null, null);

			public TransformResult(FieldDeclarationSyntax oldField, FieldDeclarationSyntax newField, List<Diagnostic> diagnostics = null)
			{
				OldField = oldField;
				NewField = newField;
				Diagnostics = diagnostics ?? new List<Diagnostic>();
			}
		}

		public void Initialize(IncrementalGeneratorInitializationContext context)
		{
			IncrementalValuesProvider<TransformResult> implementation = context.SyntaxProvider.CreateSyntaxProvider(ShouldTransformNode, Transform);

			IncrementalValueProvider<ImmutableArray<TransformResult>> collected = implementation.Collect();

			//context.RegisterSourceOutput(collected, OnRegisterSourceOutput);
		}

		static void OnRegisterSourceOutput(SourceProductionContext context, ImmutableArray<TransformResult> transformedFields)
		{
			foreach (var treeTransformedFields in transformedFields.GroupBy((tr) => tr.OldSyntaxTree))
			{
				var tree = treeTransformedFields.Key;
				tree = tree.WithRootAndOptions(
					root: tree.GetRoot().ReplaceNodes(
						from tf in treeTransformedFields select tf.OldField,
						(old, _) => (from tf in treeTransformedFields where tf.OldField == old select tf.NewField).First()
					),
					options: tree.Options
				);
				string filename = Path.GetFileName(tree.FilePath);
				context.AddSource(filename, SourceText.From(tree.ToString(), Encoding.UTF8));
			}
		}

		static bool ShouldTransformNode(SyntaxNode syntaxNode, CancellationToken token = default)
		{
			if (!(syntaxNode is FieldDeclarationSyntax fds)) return false;

			if (!(fds.AttributeLists.Where(
						(al) => al.Attributes.Where(
							(a) => a.Name.ToString() == "FileVersionConstraint" || a.Name.ToString() == "FileVersionConstraintAttribute"
						).Count() > 0
					).Count() > 0
				)) return false;

			return true;
		}

		static TransformResult Transform(GeneratorSyntaxContext context, CancellationToken cancellationToken = default)
		{
			if (!(context.Node is FieldDeclarationSyntax userField)) return TransformResult.None;



			SyntaxTriviaList leadingTrivia = userField.GetLeadingTrivia();
			SyntaxTrivia docTrivia = leadingTrivia.Where((t) => t.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia)).FirstOrDefault();
			var docNode = docTrivia.GetStructure() as DocumentationCommentTriviaSyntax;
			docNode ??= DocumentationCommentTrivia(SyntaxKind.SingleLineDocumentationCommentTrivia);
			var xmlElement = (from element in docNode.ChildNodes() where element.IsKind(SyntaxKind.XmlElement)
				              from endTag  in element.ChildNodes()  where endTag .IsKind(SyntaxKind.XmlElementEndTag)
				              from name    in endTag .ChildNodes()  where name   .IsKind(SyntaxKind.XmlName)
				              from token   in name   .ChildTokens() where token  .IsKind(SyntaxKind.IdentifierToken)
					                                                   && token.Text.ToLowerInvariant() == "summary"
				              select element as XmlElementSyntax
				             ).FirstOrDefault();

			var commentContentToAdd = new XmlNodeSyntax[] {
				XmlElement("b",
					List(new XmlNodeSyntax[] {
						XmlText("Minimum File Version")
					})
				),
				XmlText(": {0}")
			};

			SyntaxTrivia newTrivia;
			if (xmlElement != null)
			{
				newTrivia = Trivia(
					docNode.ReplaceNode(xmlElement,
						xmlElement.AddContent(
							XmlNewLine("\n"),
							XmlEmptyElement("br"),
							XmlEmptyElement("br")
						)
						.AddContent(commentContentToAdd)
					)
				);
			}
			else
			{
				newTrivia = Trivia(
					docNode.WithContent(
						docNode.Content.InsertRange(0, new XmlNodeSyntax[] {
							XmlNewLine("\n"),
							XmlElement("summary", List(commentContentToAdd)),
							XmlText("\n")
						})
					)
				);
			}

			SyntaxTriviaList newLeadingTrivia;
			if (docTrivia != default)
			{
				newLeadingTrivia = leadingTrivia.Replace(docTrivia, newTrivia);
			}
			else
			{
				newLeadingTrivia = leadingTrivia.Insert(Math.Min(1, leadingTrivia.Count), newTrivia);
			}

			FieldDeclarationSyntax newUserField = userField.WithLeadingTrivia(newLeadingTrivia);

			return new TransformResult(userField, newUserField);
		}
	}
}
