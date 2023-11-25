using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Text;
using System.Threading;

namespace CM3D2.Serialization.SourceGenerators.Extensions
{

#pragma warning disable RS1009 // Only internal implementations of this interface are allowed
	internal class SpecialSymbol : ISymbol
#pragma warning restore RS1009 // Only internal implementations of this interface are allowed
	{
		public static readonly SpecialSymbol ForEach = new() { name = "<ForEach>" };

		private string name;

		private SpecialSymbol() { }

		public SymbolKind Kind => SymbolKind.Property;

		public string Language => "CSharp";

		public string Name => name;

		public string MetadataName => name;

		public int MetadataToken => base.GetHashCode();

		public ISymbol ContainingSymbol => null;

		public IAssemblySymbol ContainingAssembly => null;

		public IModuleSymbol ContainingModule => null;

		public INamedTypeSymbol ContainingType => null;

		public INamespaceSymbol ContainingNamespace => null;

		public bool IsDefinition => false;

		public bool IsStatic => false;

		public bool IsVirtual => false;

		public bool IsOverride => false;

		public bool IsAbstract => false;

		public bool IsSealed => false;

		public bool IsExtern => false;

		public bool IsImplicitlyDeclared => false;

		public bool CanBeReferencedByName => false;

		public ImmutableArray<Location> Locations => default;

		public ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => default;

		public Accessibility DeclaredAccessibility => default;

		public ISymbol OriginalDefinition => default;

		public bool HasUnsupportedMetadata => false;

#nullable enable
		public void Accept(SymbolVisitor visitor)
		{
			throw new NotImplementedException();
		}

		public TResult? Accept<TResult>(SymbolVisitor<TResult> visitor)
		{
			return default;
		}

		public TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument argument)
		{
			return default;
		}

		public bool Equals(ISymbol? other, SymbolEqualityComparer equalityComparer)
		{
			return ReferenceEquals(this, other) || equalityComparer.Equals(this, other);
		}

		public bool Equals(ISymbol? other)
		{
			return ReferenceEquals(this, other);
		}

		public ImmutableArray<AttributeData> GetAttributes()
		{
			return default;
		}

		public string? GetDocumentationCommentId()
		{
			return default;
		}

		public string? GetDocumentationCommentXml(CultureInfo? preferredCulture = null, bool expandIncludes = false, CancellationToken cancellationToken = default)
		{
			return default;
		}

		public ImmutableArray<SymbolDisplayPart> ToDisplayParts(SymbolDisplayFormat? format = null)
		{
			return default;
		}

		public string ToDisplayString(SymbolDisplayFormat? format = null)
		{
			return Name;
		}

		public ImmutableArray<SymbolDisplayPart> ToMinimalDisplayParts(SemanticModel semanticModel, int position, SymbolDisplayFormat? format = null)
		{
			return default;
		}

		public string ToMinimalDisplayString(SemanticModel semanticModel, int position, SymbolDisplayFormat? format = null)
		{
			return Name;
		}
	}
#nullable restore
}
