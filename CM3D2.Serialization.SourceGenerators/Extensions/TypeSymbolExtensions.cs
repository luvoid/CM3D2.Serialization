using CM3D2.Serialization.Types;
using Microsoft.CodeAnalysis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;


#nullable enable
namespace CM3D2.Serialization.SourceGenerators.Extensions
{
	internal static class TypeSymbolExtensions
	{
		public static bool IsExplicitCM3D2Serializable(this ITypeSymbol typeSymbol)
		{
			return IsExplicitCM3D2Serializable(typeSymbol, out _, out _, out _, out _);
		}

		public static bool IsExplicitCM3D2Serializable(this ITypeSymbol typeSymbol, out bool isDeepSerializable)
		{
			return IsExplicitCM3D2Serializable(typeSymbol, out isDeepSerializable, out _, out _, out _);
		}

		public static bool IsExplicitCM3D2Serializable(this ITypeSymbol typeSymbol, 
			out bool isDeepSerializable, out bool isUnmanagedSerializable,
			out bool isInterfaceSerializable, out INamedTypeSymbol? implementedInterface)
		{
			isDeepSerializable = typeSymbol.HasAttribute(typeof(DeepSerializableAttribute));
			isUnmanagedSerializable = typeSymbol.HasAttribute(typeof(UnmanagedSerializableAttribute));
			isInterfaceSerializable = typeSymbol.ImplementsAnyOf(out implementedInterface, typeof(ICM3D2Serializable), typeof(ICM3D2SerializableInstance));
			return isDeepSerializable || isUnmanagedSerializable || isInterfaceSerializable;
		}

		public static bool IsCM3D2Serializable(this ITypeSymbol typeSymbol)
		{
			if (typeSymbol is INamedTypeSymbol namedTypeSymbol
				&& namedTypeSymbol.ConstructedFrom != null
				&& namedTypeSymbol.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T)
			{
				typeSymbol = namedTypeSymbol.TypeArguments[0];
			}

			if (typeSymbol.SpecialType == SpecialType.System_String)
				return true;

			if (typeSymbol.IsUnmanagedType)
				return true;

			if (IsExplicitCM3D2Serializable(typeSymbol))
				return true;

			return false;
		}




		/// <summary>
		/// Returns <see langword="true"/> if the <paramref name="typeSymbol"/> represents the same type as the <paramref name="typeReflection"/>.
		/// </summary>
		public static bool EqualsType(this ITypeSymbol typeSymbol, Type typeReflection)
		{
			if (typeSymbol.MetadataName != typeReflection.Name)
				return false;

			if (typeSymbol is INamedTypeSymbol namedTypeSymbol)
			{
				if (namedTypeSymbol.IsUnboundGenericType != typeReflection.IsGenericTypeDefinition)
					return false;

				for (int i = 0; i < namedTypeSymbol.Arity; i++)
				{
					if (!namedTypeSymbol.TypeArguments[i].IsType(typeReflection.GenericTypeArguments[i]))
						return false;
				}
			}
			else if (typeReflection.IsGenericTypeDefinition)
			{
				return false;
			}

			if (typeSymbol.ContainingType != null)
			{
				if (typeReflection.DeclaringType == null)
					return false;

				return typeSymbol.ContainingType.IsType(typeReflection.DeclaringType!);
			}
			else if (typeSymbol.ContainingNamespace != null)
			{
				string fullNamespace = typeSymbol.ContainingNamespace.ToDisplayString();
				return fullNamespace == typeReflection.Namespace;
			}
			else
			{
				return typeReflection.DeclaringType == null && typeReflection.Namespace == null;
			}
		}

		/// <summary>
		/// Returns <see langword="true"/> if the <paramref name="typeSymbol"/> represents the same type as the <paramref name="typeReflection"/>;
		/// or represents a generic type that is constructed from it.
		/// </summary>
		public static bool EqualsOrConstructedFromType(this ITypeSymbol typeSymbol, Type typeReflection)
		{
			if (typeSymbol.EqualsType(typeReflection))
				return true;

			if (typeSymbol is INamedTypeSymbol namedTypeSymbol
				&& namedTypeSymbol.ConstructedFrom != null
				&& !namedTypeSymbol.Equals(namedTypeSymbol.ConstructedFrom, SymbolEqualityComparer.Default)
				&& namedTypeSymbol.EqualsType(typeReflection))
				return true;

			return false;
		}

		/// <summary>
		/// Returns <see langword="true"/> if the <paramref name="typeSymbol"/> represents the same type as the <paramref name="typeReflection"/>;
		/// or represents a type that derives from it, is constructed from it, or derives from a type that is constructed from it.
		/// </summary>
		/// <remarks>
		/// Generic parameters are not checked for compatibility; 
		/// e.g. <c><![CDATA[IEnumerable<object>]]></c> will not match with <c><![CDATA[IEnumerable<string>]]></c>.
		/// </remarks>
		public static bool IsType(this ITypeSymbol typeSymbol, Type typeReflection)
		{
			var baseTypeSymbol = typeSymbol;
			do
			{
				if (baseTypeSymbol.EqualsOrConstructedFromType(typeReflection))
					return true;
			}
			while ((baseTypeSymbol = baseTypeSymbol.BaseType) != null);

			return false;
		}






		public static bool Implements(this ITypeSymbol typeSymbol, string interfaceName, int arity = 0)
		{
			return typeSymbol.Implements(interfaceName, arity, out _);
		}
		public static bool Implements(this ITypeSymbol typeSymbol, string interfaceName, int arity, [NotNullWhen(true)] out INamedTypeSymbol? implementedSymbol)
		{
			if (typeSymbol is INamedTypeSymbol namedType
				&& namedType.TypeParameters.Length == arity
				&& namedType.Name == interfaceName)
			{
				implementedSymbol = namedType;
				return true;
			}
			foreach (var implemented in typeSymbol.AllInterfaces)
			{
				if (implemented.Name == interfaceName)
				{
					implementedSymbol = implemented;
					return true;
				}
			}
			implementedSymbol = null;
			return false;
		}
		public static bool Implements(this ITypeSymbol typeSymbol, INamedTypeSymbol interfaceSymbol)
		{
			var comparer = SymbolEqualityComparer.Default;
			if (comparer.Equals(typeSymbol, interfaceSymbol)) return true;
			foreach (var implemented in typeSymbol.AllInterfaces)
			{
				if (comparer.Equals(implemented, interfaceSymbol)) return true;
			}
			return false;
		}
		public static bool Implements(this ITypeSymbol typeSymbol, Type typeReflection)
		{
			return Implements(typeSymbol, typeReflection, out _);
		}
		public static bool Implements(this ITypeSymbol typeSymbol, Type typeReflection, [NotNullWhen(true)] out INamedTypeSymbol? implementedSymbol)
		{
			return ImplementsAnyOf(typeSymbol, out implementedSymbol, typeReflection);
		}


		public static bool ImplementsAnyOf(this ITypeSymbol typeSymbol, params string[] interfaceNames)
		{
			return ImplementsAnyOf(typeSymbol, out _, interfaceNames);
		}
		public static bool ImplementsAnyOf(this ITypeSymbol typeSymbol, [NotNullWhen(true)] out INamedTypeSymbol? implementedSymbol, params string[] interfaceNames)
		{
			if (interfaceNames.Contains(typeSymbol.Name))
			{
				implementedSymbol = (INamedTypeSymbol)typeSymbol;
				return true;
			}
			foreach (var implemented in typeSymbol.AllInterfaces)
			{
				if (interfaceNames.Contains(implemented.Name))
				{
					implementedSymbol = implemented;
					return true;
				}
			}
			implementedSymbol = null;
			return false;
		}
		public static bool ImplementsAnyOf(this ITypeSymbol typeSymbol, params Type[] typeReflections)
		{
			return ImplementsAnyOf(typeSymbol, out _, typeReflections);
		}
		public static bool ImplementsAnyOf(this ITypeSymbol typeSymbol, [NotNullWhen(true)] out INamedTypeSymbol? implementedSymbol, params Type[] typeReflections)
		{
			if (typeReflections.Any(t => typeSymbol.EqualsOrConstructedFromType(t)))
			{
				implementedSymbol = (INamedTypeSymbol)typeSymbol;
				return true;
			}
			foreach (var implemented in typeSymbol.AllInterfaces)
			{
				if (typeReflections.Any(t => implemented.EqualsOrConstructedFromType(t)))
				{
					implementedSymbol = implemented;
					return true;
				}
			}
			implementedSymbol = null;
			return false;
		}



		public static IEnumerable<IFieldSymbol> GetFields(this ITypeSymbol type, BindingFlags bindingFlags, bool includeConst = true, bool includeReadOnly = true)
		{
			foreach (ISymbol member in type.GetMembers())
			{
				if (member is not IFieldSymbol field) continue;

				if (field.IsStatic && !bindingFlags.HasFlag(BindingFlags.Static)) continue;
				if (!field.IsStatic && !bindingFlags.HasFlag(BindingFlags.Instance)) continue;
				if (!field.IsDefinition && bindingFlags.HasFlag(BindingFlags.DeclaredOnly)) continue;
				if (field.DeclaredAccessibility == Accessibility.Public && !bindingFlags.HasFlag(BindingFlags.Public)) continue;
				if (field.DeclaredAccessibility != Accessibility.Public && !bindingFlags.HasFlag(BindingFlags.NonPublic)) continue;
				if (field.IsConst && !includeConst) continue;
				if (field.IsReadOnly && !includeReadOnly) continue;

				yield return field;
			}
		}

		public static IEnumerable<IFieldSymbol> GetDeepFields(this ITypeSymbol type, BindingFlags bindingFlags, bool includeConst = true, bool includeReadOnly = true)
		{
			do
			{
				foreach (IFieldSymbol inheritedField in type!.GetDeepFields(bindingFlags, includeConst, includeReadOnly))
				{
					yield return inheritedField;
				}
			}
			while ((type = type.BaseType!) != null);
		}




		public static TSymbol GetMember<TSymbol>(this ITypeSymbol type, string memberName)
			where TSymbol : ISymbol
		{
			if (TryGetMember<TSymbol>(type, memberName, out var memberSymbol))
			{
				return memberSymbol;
			}
			else
			{
				throw new MissingMemberException(type.Name, memberName);
			}
		}
		public static TSymbol GetMember<TSymbol>(this ITypeSymbol type, MemberInfo memberReflection)
			where TSymbol : ISymbol
		{
			if (TryGetMember<TSymbol>(type, memberReflection, out var memberSymbol))
			{
				return memberSymbol;
			}
			else
			{
				throw new MissingMemberException(type.Name, memberReflection.Name);
			}
		}

		public static bool TryGetMember<TSymbol>(this ITypeSymbol type, string memberName, [MaybeNullWhen(false)] out TSymbol memberSymbol)
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
		public static bool TryGetMember<TSymbol>(this ITypeSymbol type, MemberInfo memberReflection, [MaybeNullWhen(false)] out TSymbol memberSymbol)
			where TSymbol : ISymbol
		{
			string memberReflectionName = memberReflection.Name;
			int tickIndex = memberReflectionName.IndexOf('`');
			if (tickIndex != -1)
			{
				memberReflectionName = memberReflectionName.Substring(0, tickIndex);
			}

			foreach (var member in type.GetDeepMembers(memberReflectionName))
			{
				if (!member.ContainingType.EqualsOrConstructedFromType(memberReflection.DeclaringType))
					continue;

				if (memberReflection is MethodBase methodReflection)
				{
					if (member is not IMethodSymbol methodSymbol)
						continue;

					if (methodReflection.GetGenericArguments().Length != methodSymbol.TypeArguments.Length)
						continue;

					var parametersReflection = methodReflection.GetParameters();
					if (parametersReflection.Length != methodSymbol.Parameters.Length)
						continue;

					for (int i = 0; i < parametersReflection.Length; i++)
					{
						if (parametersReflection[i].ParameterType.IsGenericParameter)
							continue;
						if (!methodSymbol.Parameters[i].Type.EqualsOrConstructedFromType(parametersReflection[i].ParameterType))
							goto continue_members_loop;
					}
				}

				if (member is TSymbol tMember)
				{
					memberSymbol = tMember;
					return true;
				}
				
			continue_members_loop:
				continue;
			}
			memberSymbol = default;
			return false;
		}

		public static IMethodSymbol GetMethod(this ITypeSymbol type, string methodName)
		{
			if (TryGetMethod(type, methodName, out var memberSymbol))
			{
				return memberSymbol;
			}
			else
			{
				throw new MissingMethodException(type.Name, methodName);
			}
		}
		public static IMethodSymbol GetMethod(this ITypeSymbol type, MethodBase methodReflection)
		{
			if (TryGetMethod(type, methodReflection, out var methodSymbol))
			{
				return methodSymbol;
			}
			else
			{
				throw new MissingMethodException(type.Name, methodReflection.Name);
			}
		}
		public static IMethodSymbol GetMethod(this ITypeSymbol type, Delegate method) => GetMethod(type, method.Method);

		public static bool TryGetMethod(this ITypeSymbol type, string methodName, [NotNullWhen(true)] out IMethodSymbol? methodSymbol)
		{
			return TryGetMember<IMethodSymbol>(type, methodName, out methodSymbol);
		}
		public static bool TryGetMethod(this ITypeSymbol type, MethodBase methodReflection, [NotNullWhen(true)] out IMethodSymbol? methodSymbol)
		{
			return TryGetMember<IMethodSymbol>(type, methodReflection, out methodSymbol);
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
	}
}
#nullable restore
