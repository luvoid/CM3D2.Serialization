using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace CM3D2.Serialization.SourceGenerators.Extensions
{
	internal readonly struct VariableInfo
	{
		public readonly SyntaxToken Identifier;
		public readonly string Name;
		public readonly ITypeSymbol Type;
		public readonly SyntaxReference Reference;

		public VariableInfo(VariableDeclarationSyntax declaration, VariableDeclaratorSyntax declarator, SemanticModel semanticModel)
		{
			Identifier = declarator.Identifier;

			Name = Identifier.Text;

			if (declaration.Type.IsVar)
			{
				Type = declarator.Initializer.Value.GetSpeculativeTypeSymbol(semanticModel);
			}
			else
			{
				Type = declaration.Type.GetSpeculativeTypeSymbol(semanticModel);
			}

			Reference = declarator.GetReference();
		}
	}
}
