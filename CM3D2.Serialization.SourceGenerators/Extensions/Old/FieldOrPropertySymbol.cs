using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Threading;

#nullable enable
namespace CM3D2.Serialization.SourceGenerators.Extensions.Old
{
#pragma warning disable RS1009 // Only internal implementations of this interface are allowed
    internal class FieldOrPropertySymbol : ISymbol, IEquatableUsingComparer<ISymbol, SymbolEqualityComparer>
#pragma warning restore RS1009 // Only internal implementations of this interface are allowed
    {
        private ISymbol Symbol => (ISymbol?)fieldSymbol ?? propertySymbol!;
        private readonly IFieldSymbol? fieldSymbol;
        private readonly IPropertySymbol? propertySymbol;
        private readonly IMethodSymbol? methodSymbol;

        public FieldOrPropertySymbol(IFieldSymbol fieldSymbol)
        {
            this.fieldSymbol = fieldSymbol;
            propertySymbol = null;
        }
        public FieldOrPropertySymbol(IPropertySymbol propertySymbol)
        {
            fieldSymbol = null;
            this.propertySymbol = propertySymbol;
        }

        public SymbolKind Kind => Symbol.Kind;

        public string Language => Symbol.Language;

        public string Name => Symbol.Name;

        public string MetadataName => Symbol.MetadataName;

        public int MetadataToken => Symbol.MetadataToken;

        public ISymbol ContainingSymbol => Symbol.ContainingSymbol;

        public IAssemblySymbol ContainingAssembly => Symbol.ContainingAssembly;

        public IModuleSymbol ContainingModule => Symbol.ContainingModule;

        public INamedTypeSymbol ContainingType => Symbol.ContainingType;

        public INamespaceSymbol ContainingNamespace => Symbol.ContainingNamespace;

        public bool IsDefinition => Symbol.IsDefinition;

        public bool IsStatic => Symbol.IsStatic;

        public bool IsVirtual => Symbol.IsVirtual;

        public bool IsOverride => Symbol.IsOverride;

        public bool IsAbstract => Symbol.IsAbstract;

        public bool IsSealed => Symbol.IsSealed;

        public bool IsExtern => Symbol.IsExtern;

        public bool IsImplicitlyDeclared => Symbol.IsImplicitlyDeclared;

        public bool CanBeReferencedByName => Symbol.CanBeReferencedByName;

        public ImmutableArray<Location> Locations => Symbol.Locations;

        public ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => Symbol.DeclaringSyntaxReferences;

        public Accessibility DeclaredAccessibility => Symbol.DeclaredAccessibility;

        public ISymbol OriginalDefinition => Symbol.OriginalDefinition;

        public bool HasUnsupportedMetadata => Symbol.HasUnsupportedMetadata;

        public bool IsReadOnly => fieldSymbol?.IsReadOnly ?? propertySymbol?.IsReadOnly ?? methodSymbol!.IsReadOnly;

        //public bool IsRequired => fieldSymbol?.IsRequired ?? propertySymbol!.IsRequired;

        public RefKind RefKind => fieldSymbol?.RefKind ?? propertySymbol?.RefKind ?? methodSymbol!.RefKind;

        public ImmutableArray<CustomModifier> RefCustomModifiers => fieldSymbol?.RefCustomModifiers ?? propertySymbol?.RefCustomModifiers ?? methodSymbol!.RefCustomModifiers;

        public ITypeSymbol Type => fieldSymbol?.Type ?? propertySymbol!.Type;

        public NullableAnnotation NullableAnnotation => fieldSymbol?.NullableAnnotation ?? propertySymbol!.NullableAnnotation;

        public void Accept(SymbolVisitor visitor)
        {
            Symbol.Accept(visitor);
        }

        public TResult? Accept<TResult>(SymbolVisitor<TResult> visitor)
        {
            return Symbol.Accept(visitor);
        }

        public TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument argument)
        {
            return Symbol.Accept(visitor, argument);
        }

        public bool Equals(ISymbol? other, SymbolEqualityComparer equalityComparer)
        {
            return Symbol.Equals(other, equalityComparer);
        }

        public ImmutableArray<AttributeData> GetAttributes()
        {
            return Symbol.GetAttributes();
        }

        public string? GetDocumentationCommentId()
        {
            return Symbol.GetDocumentationCommentId();
        }

        public string? GetDocumentationCommentXml(CultureInfo? preferredCulture = null, bool expandIncludes = false, CancellationToken cancellationToken = default)
        {
            return Symbol.GetDocumentationCommentXml(preferredCulture, expandIncludes, cancellationToken);
        }

        public ImmutableArray<SymbolDisplayPart> ToDisplayParts(SymbolDisplayFormat? format = null)
        {
            return Symbol.ToDisplayParts(format);
        }

        public string ToDisplayString(SymbolDisplayFormat? format = null)
        {
            return Symbol.ToDisplayString(format);
        }

        public ImmutableArray<SymbolDisplayPart> ToMinimalDisplayParts(SemanticModel semanticModel, int position, SymbolDisplayFormat? format = null)
        {
            return Symbol.ToMinimalDisplayParts(semanticModel, position, format);
        }

        public string ToMinimalDisplayString(SemanticModel semanticModel, int position, SymbolDisplayFormat? format = null)
        {
            return Symbol.ToMinimalDisplayString(semanticModel, position, format);
        }

        public bool Equals(ISymbol? other)
        {
            if (other is FieldOrPropertySymbol otherFieldOrProperty)
            {
                return Symbol.Equals(otherFieldOrProperty.Symbol);

            }
            else
            {
                return Symbol.Equals(other);
            }
        }
    }
}
#nullable restore
