using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Diagnostics.CodeAnalysis;
using System.Data;

namespace CM3D2.Serialization.SourceGenerators.Extensions
{

#nullable enable
	internal static class SymbolExtensions
    {
        public static bool IsCM3D2Serializable(this ITypeSymbol typeSymbol)
        {
            if (typeSymbol.ImplementsAnyOf("ICM3D2Serializable", "ICM3D2SerializableInstance"))
                return true;

            if (typeSymbol.IsUnmanagedType)
                return true;

            if (typeSymbol.Name.ToLower() == "string" && typeSymbol.ContainingNamespace.Name == "System")
                return true;

            return false;
        }

        public static bool IsDeepSerialized(this IFieldSymbol fieldSymbol)
        {
            if (fieldSymbol.HasAttribute("DeepSerialized"))
                return true;

            if (fieldSymbol.Type.HasAttribute("DeepSerializable"))
                return true;

            return false;
        }


        public static bool HasAttribute(this ISymbol symbol, string attributeName)
            => symbol.TryGetAttribute(attributeName, out _);

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

        public static bool Implements(this ITypeSymbol type, string interfaceName, int arity = 0)
        {
            return type.Implements(interfaceName, arity, out _);
        }

        public static bool Implements(this ITypeSymbol type, string interfaceName, int arity, [MaybeNull, NotNullWhen(true)] out INamedTypeSymbol interfaceSymbol)
        {
            if (type is INamedTypeSymbol namedType
                && namedType.TypeParameters.Length == arity
                && namedType.Name == interfaceName)
            {
                interfaceSymbol = namedType;
                return true;
            }
            foreach (var implemented in type.AllInterfaces)
            {
                if (implemented.Name == interfaceName)
                {
                    interfaceSymbol = implemented;
                    return true;
                }
            }
            interfaceSymbol = null;
            return false;
        }

        public static bool ImplementsAnyOf(this ITypeSymbol type, params string[] interfaceNames)
        {
            if (interfaceNames.Contains(type.Name)) return true;
            foreach (var implemented in type.AllInterfaces)
            {
                if (interfaceNames.Contains(implemented.Name)) return true;
            }
            return false;
        }

        public static bool Implements(this ITypeSymbol type, INamedTypeSymbol interfaceSymbol)
        {
            var comparer = SymbolEqualityComparer.Default;
            if (comparer.Equals(type, interfaceSymbol)) return true;
            foreach (var implemented in type.AllInterfaces)
            {
                if (comparer.Equals(implemented, interfaceSymbol)) return true;
            }
            return false;
        }

        public static IEnumerable<IFieldSymbol> GetFields(this ITypeSymbol type, BindingFlags bindingFlags, bool includeConst = true, bool includeReadOnly = true)
        {
            if (type.BaseType != null)
            {
                foreach (IFieldSymbol inheritedField in type.BaseType.GetFields(bindingFlags, includeConst, includeReadOnly))
                {
                    yield return inheritedField;
				}
            }

            foreach (ISymbol member in type.GetMembers())
            {
                if (member is not IFieldSymbol field) continue;

                if ( field.IsStatic     && !bindingFlags.HasFlag(BindingFlags.Static      )) continue;
                if (!field.IsStatic     && !bindingFlags.HasFlag(BindingFlags.Instance    )) continue;
                if (!field.IsDefinition &&  bindingFlags.HasFlag(BindingFlags.DeclaredOnly)) continue;
                if (field.DeclaredAccessibility == Accessibility.Public && !bindingFlags.HasFlag(BindingFlags.Public   )) continue;
                if (field.DeclaredAccessibility != Accessibility.Public && !bindingFlags.HasFlag(BindingFlags.NonPublic)) continue;
                if (field.IsConst    && !includeConst   ) continue;
                if (field.IsReadOnly && !includeReadOnly) continue;

                yield return field;
            }
        }

        public static bool TryGetMember<TSymbol>(this ITypeSymbol type, string memberName, [MaybeNull, NotNullWhen(true)] out TSymbol memberSymbol)
            where TSymbol : ISymbol
        {
			foreach (var member in type.GetDeepMembers(memberName))
			{
				if (member is TSymbol tMember)
                {
                    memberSymbol = tMember;
                    return true;
                }
			}
            memberSymbol = default;
			return false;
		}

        public static IPropertySymbol GetIndexer(this ITypeSymbol type, params string?[] parameterTypeNames)
        {
			if (!TryGetIndexer(type, out var indexerSymbol, parameterTypeNames))
            {
                string args;
                if (parameterTypeNames.Length == 0)
                {
                    args = "...";
                }
                else
                {
                    for (int i = 0; i < parameterTypeNames.Length; i++)
                    {
                        if (parameterTypeNames[i] == null)
                        {
                            parameterTypeNames[i] = "*";
						}
                    }
					args = string.Join(", ", parameterTypeNames);
				}
                throw new MissingMemberException(type.Name, $"[{args}]");
            }
            else
			{
				return indexerSymbol;
			}
		}

		public static bool TryGetIndexer(this ITypeSymbol type, [NotNullWhen(true)] out IPropertySymbol? indexerSymbol, params string?[] parameterTypeNames)
		{
			foreach (ISymbol member in type.GetDeepMembers())
			{
				if (member is IPropertySymbol property && property.IsIndexer)
				{
					if (parameterTypeNames.Length > 0)
					{
						if (parameterTypeNames.Length != property.Parameters.Length) continue;

						for (int i = 0; i < parameterTypeNames.Length; i++)
						{
							var paramTypeName = parameterTypeNames[i];
							if (paramTypeName == null) continue;
							if (property.Parameters[i].Type.Name != paramTypeName)
								goto continue_member_loop;
						}
					}
					// If all checks above passed
					indexerSymbol = property;
					return true;
				}
			continue_member_loop:
				continue;
			}
			// If not matching member was found
			indexerSymbol = default;
			return false;
		}


        public static IEnumerable<ISymbol> GetDeepMembers(this ITypeSymbol type, string? name = null)
        {
            Queue<ITypeSymbol> typeQueue = new();
            ITypeSymbol? baseType = type;
            while (baseType != null)
            {
                typeQueue.Enqueue(baseType);
                baseType = baseType.BaseType;
			}
            ISet<ITypeSymbol> checkedTypes = new HashSet<ITypeSymbol>(typeQueue, SymbolEqualityComparer.Default);

            while (typeQueue.Count > 0)
            {
                var subtype = typeQueue.Dequeue();
                var members = name == null ? subtype.GetMembers() : subtype.GetMembers(name);
                foreach (ISymbol member in members)
                {
                    yield return member;
                }

				if (subtype.TypeKind == TypeKind.Interface || subtype.TypeKind == TypeKind.TypeParameter)
				{
					foreach (INamedTypeSymbol @interface in subtype.Interfaces)
					{
						if (checkedTypes.Contains(@interface)) continue;
						checkedTypes.Add(@interface);
                        typeQueue.Enqueue(@interface);
                    }
				}
			}
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
