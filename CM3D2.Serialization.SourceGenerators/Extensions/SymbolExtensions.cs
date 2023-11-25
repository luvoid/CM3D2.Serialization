using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Diagnostics.CodeAnalysis;
using System.Data;
using CM3D2.Serialization.Types;
using CM3D2.Serialization.SourceGenerators.Extensions.Old;

namespace CM3D2.Serialization.SourceGenerators.Extensions
{

#nullable enable
    internal static class SymbolExtensions
    {
		public static bool IsDeepSerialized(this IFieldSymbol fieldSymbol)
		{
			if (fieldSymbol.HasAttribute(typeof(DeepSerializedAttribute)))
				return true;

			if (fieldSymbol.Type.HasAttribute(typeof(DeepSerializableAttribute)))
				return true;

			return false;
		}


		public static bool TryGetTypeSymbol(this ISymbol symbol, [NotNullWhen(true)] out ITypeSymbol? typeSymbol)
		{
			if (symbol is IEventSymbol eventSymbol)
			{
				typeSymbol = eventSymbol.Type;
			}
			else if (symbol is IFieldSymbol fieldSymbol)
			{
				typeSymbol = fieldSymbol.Type;
			}
			else if (symbol is ILocalSymbol localSymbol)
			{
				typeSymbol = localSymbol.Type;
			}
			else if (symbol is IMethodSymbol methodSymbol)
			{
				typeSymbol = methodSymbol.ReturnType;
			}
			else if (symbol is IParameterSymbol parameterSymbol)
			{
				typeSymbol = parameterSymbol.Type;
			}
			else if (symbol is IPropertySymbol propertySymbol)
			{
				typeSymbol = propertySymbol.Type;
			}
			else
			{
				typeSymbol = null;
			}
			return typeSymbol is not null;
		}



		public static bool HasAttribute(this ISymbol symbol, Type typeReflections)
            => symbol.TryGetAttribute(typeReflections, out _);
        public static bool HasAttribute(this ISymbol symbol, string attributeName)
            => symbol.TryGetAttribute(attributeName, out _);

		public static bool HasAnyAttributeOf(this ISymbol symbol, params Type[] typeReflections)
		{
			foreach (AttributeData attr in symbol.GetAttributes())
			{
				var namedTypeSymbol = attr.AttributeClass;
				if (namedTypeSymbol == null) continue;
                if (typeReflections.Any(t => namedTypeSymbol.IsType(t)))
                    return true;
			}
			return false;
		}
		public static bool HasAnyAttributeOf(this ISymbol symbol, params string[] attributeNames)
        {
            string[] longAttributeNames = attributeNames.Select(s => $"{s}Attribute").ToArray();

			foreach (AttributeData attr in symbol.GetAttributes())
            {
                string? name = attr.AttributeClass?.Name;
                if (name == null) continue;
				if (attributeNames.Contains(name) || longAttributeNames.Contains(name))
                    return true;
            }
            return false;
        }

		public static bool TryGetAttribute(this ISymbol symbol, Type attributeReflection, [NotNullWhen(true)] out AttributeData? attribute)
		{
			foreach (AttributeData attr in symbol.GetAttributes())
			{
				var namedTypeSymbol = attr.AttributeClass;
                if (namedTypeSymbol == null) continue;
				if (namedTypeSymbol.IsType(attributeReflection))
				{
					attribute = attr;
					return true;
				}
			}
			attribute = null;
			return false;
		}
		public static bool TryGetAttribute(this ISymbol symbol, string attributeName, [NotNullWhen(true)] out AttributeData? attribute)
        {
            string longAttributeName = $"{attributeName}Attribute";
            foreach (AttributeData attr in symbol.GetAttributes())
            {
                var name = attr.AttributeClass?.Name;
                if (name == attributeName || name == longAttributeName)
                {
                    attribute = attr;
                    return true;
                }
            }
            attribute = null;
            return false;
        }




		public static bool IsFieldOrPropertySymbol(this ISymbol symbol, out FieldOrPropertySymbol? fieldOrPropertySymbol)
        {
			if (symbol is FieldOrPropertySymbol matchedSymbol)
            {
                fieldOrPropertySymbol = matchedSymbol;
                return true;
			}
            else if (symbol is IFieldSymbol fieldSymbol)
            {
                fieldOrPropertySymbol = new FieldOrPropertySymbol(fieldSymbol);
				return true;
			}
            else if (symbol is IPropertySymbol propertySymbol)
			{
				fieldOrPropertySymbol = new FieldOrPropertySymbol(propertySymbol);
				return true;
			}
            else
            {
                fieldOrPropertySymbol = default;
                return false;
			}
        }

	}
}
#nullable restore
