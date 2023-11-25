using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Text;
using System.Xml.Linq;

namespace CM3D2.Serialization.SourceGenerators.Extensions
{
	internal static class SyntaxNodeExtensions
	{
		public static ISymbol GetSpeculativeSymbol(this AttributeSyntax node, SemanticModel semanticModel)
			=> node.InternalGetSpeculativeSymbol(semanticModel, default);
		public static ISymbol GetSpeculativeSymbol(this ConstructorInitializerSyntax node, SemanticModel semanticModel)
			=> node.InternalGetSpeculativeSymbol(semanticModel, default);
		public static ISymbol GetSpeculativeSymbol(this PrimaryConstructorBaseTypeSyntax node, SemanticModel semanticModel)
			=> node.InternalGetSpeculativeSymbol(semanticModel, default);
		public static ISymbol GetSpeculativeSymbol(this CrefSyntax node, SemanticModel semanticModel, bool onlyTypeOrNamespace = false)
			=> node.InternalGetSpeculativeSymbol(semanticModel, onlyTypeOrNamespace);
		public static ISymbol GetSpeculativeSymbol(this ExpressionSyntax node, SemanticModel semanticModel, bool onlyTypeOrNamespace = false)
			=> node.InternalGetSpeculativeSymbol(semanticModel, onlyTypeOrNamespace);
		public static ISymbol GetSpeculativeSymbol(this VariableDeclaratorSyntax node, SemanticModel semanticModel)
			=> node.InternalGetSpeculativeSymbol(semanticModel, true);
		private static ISymbol InternalGetSpeculativeSymbol(this SyntaxNode node, SemanticModel semanticModel, bool onlyTypeOrNamespace)
		{
			node.InternalTryGetSpeculativeSymbol(semanticModel, out ISymbol symbol, onlyTypeOrNamespace);
			return symbol;
		}

		public static bool TryGetSpeculativeSymbol<T>(this AttributeSyntax node, SemanticModel semanticModel, out T symbol) where T : ISymbol
			=> node.InternalTryGetSpeculativeSymbol(semanticModel, out symbol, default);
		public static bool TryGetSpeculativeSymbol<T>(this ConstructorInitializerSyntax node, SemanticModel semanticModel, out T symbol) where T : ISymbol
			=> node.InternalTryGetSpeculativeSymbol(semanticModel, out symbol, default);
		public static bool TryGetSpeculativeSymbol<T>(this PrimaryConstructorBaseTypeSyntax node, SemanticModel semanticModel, out T symbol) where T : ISymbol
			=> node.InternalTryGetSpeculativeSymbol(semanticModel, out symbol, default);
		public static bool TryGetSpeculativeSymbol<T>(this CrefSyntax node, SemanticModel semanticModel, out T symbol, bool onlyTypeOrNamespace = false) where T : ISymbol
			=> node.InternalTryGetSpeculativeSymbol(semanticModel, out symbol, onlyTypeOrNamespace);
		public static bool TryGetSpeculativeSymbol<T>(this ExpressionSyntax node, SemanticModel semanticModel, out T symbol, bool onlyTypeOrNamespace = false) where T : ISymbol
			=> node.InternalTryGetSpeculativeSymbol(semanticModel, out symbol, onlyTypeOrNamespace);
		public static bool TryGetSpeculativeSymbol<T>(this VariableDeclaratorSyntax node, SemanticModel semanticModel, out T symbol) where T : ISymbol
			=> node.InternalTryGetSpeculativeSymbol(semanticModel, out symbol, true);
		private static bool InternalTryGetSpeculativeSymbol<T>(this SyntaxNode node, SemanticModel semanticModel, out T symbol, bool onlyTypeOrNamespace = false)
			where T : ISymbol
		{
			if (node == null) throw new ArgumentNullException(nameof(node));
			if (semanticModel == null) throw new ArgumentNullException(nameof(semanticModel));

			int position = node.GetLocation().SourceSpan.Start;
			SpeculativeBindingOption bindingOption = onlyTypeOrNamespace ? SpeculativeBindingOption.BindAsTypeOrNamespace : SpeculativeBindingOption.BindAsExpression;

			SymbolInfo symbolInfo;
			if      (node is AttributeSyntax                  attr) symbolInfo = semanticModel.GetSpeculativeSymbolInfo(position, attr);
			else if (node is ConstructorInitializerSyntax     ctor) symbolInfo = semanticModel.GetSpeculativeSymbolInfo(position, ctor);
			else if (node is PrimaryConstructorBaseTypeSyntax pcbt) symbolInfo = semanticModel.GetSpeculativeSymbolInfo(position, pcbt);
			else if (node is CrefSyntax                       cref) symbolInfo = semanticModel.GetSpeculativeSymbolInfo(position, cref, bindingOption);
			else if (node is ExpressionSyntax                 expr) symbolInfo = semanticModel.GetSpeculativeSymbolInfo(position, expr, bindingOption);
			else                                                    symbolInfo = semanticModel.GetSpeculativeSymbolInfo(position, node, bindingOption);

			if (symbolInfo.Symbol == null)
			{
				foreach (var symbolCandidate in symbolInfo.CandidateSymbols)
				{
					if (symbolCandidate is T tSymbol)
					{
						symbol = tSymbol;
						return true;
					}
				}
			}
			else
			{
				if (symbolInfo.Symbol is T tSymbol)
				{
					symbol = tSymbol;
					return true;
				}
			}

			symbol = default;
			return false;
		}




		public static IRangeVariableSymbol GetDeclaredSymbol(this QueryContinuationSyntax                 node, SemanticModel semanticModel) =>	InternalGetDeclaredSymbol<IRangeVariableSymbol>(node, semanticModel);
		public static IRangeVariableSymbol GetDeclaredSymbol(this JoinIntoClauseSyntax                    node, SemanticModel semanticModel) =>	InternalGetDeclaredSymbol<IRangeVariableSymbol>(node, semanticModel);
		public static IRangeVariableSymbol GetDeclaredSymbol(this QueryClauseSyntax                       node, SemanticModel semanticModel) =>	InternalGetDeclaredSymbol<IRangeVariableSymbol>(node, semanticModel);
		public static ILocalSymbol         GetDeclaredSymbol(this CatchDeclarationSyntax                  node, SemanticModel semanticModel) =>	InternalGetDeclaredSymbol<ILocalSymbol        >(node, semanticModel);
		public static ILocalSymbol         GetDeclaredSymbol(this ForEachStatementSyntax                  node, SemanticModel semanticModel) =>	InternalGetDeclaredSymbol<ILocalSymbol        >(node, semanticModel);
		public static ITypeParameterSymbol GetDeclaredSymbol(this TypeParameterSyntax                     node, SemanticModel semanticModel) =>	InternalGetDeclaredSymbol<ITypeParameterSymbol>(node, semanticModel);
		public static IParameterSymbol     GetDeclaredSymbol(this ParameterSyntax                         node, SemanticModel semanticModel) =>	InternalGetDeclaredSymbol<IParameterSymbol    >(node, semanticModel);
		public static IAliasSymbol         GetDeclaredSymbol(this ExternAliasDirectiveSyntax              node, SemanticModel semanticModel) =>	InternalGetDeclaredSymbol<IAliasSymbol        >(node, semanticModel);
		public static IAliasSymbol         GetDeclaredSymbol(this UsingDirectiveSyntax                    node, SemanticModel semanticModel) =>	InternalGetDeclaredSymbol<IAliasSymbol        >(node, semanticModel);
		public static ILabelSymbol         GetDeclaredSymbol(this SwitchLabelSyntax                       node, SemanticModel semanticModel) =>	InternalGetDeclaredSymbol<ILabelSymbol        >(node, semanticModel);
		public static ILabelSymbol         GetDeclaredSymbol(this LabeledStatementSyntax                  node, SemanticModel semanticModel) =>	InternalGetDeclaredSymbol<ILabelSymbol        >(node, semanticModel);
		public static ISymbol              GetDeclaredSymbol(this TupleElementSyntax                      node, SemanticModel semanticModel) =>	InternalGetDeclaredSymbol<ISymbol             >(node, semanticModel);
		public static ISymbol              GetDeclaredSymbol(this VariableDeclaratorSyntax                node, SemanticModel semanticModel) =>	InternalGetDeclaredSymbol<ISymbol             >(node, semanticModel);
		public static ISymbol              GetDeclaredSymbol(this SingleVariableDesignationSyntax         node, SemanticModel semanticModel) =>	InternalGetDeclaredSymbol<ISymbol             >(node, semanticModel);
		public static IMethodSymbol        GetDeclaredSymbol(this AccessorDeclarationSyntax               node, SemanticModel semanticModel) =>	InternalGetDeclaredSymbol<IMethodSymbol       >(node, semanticModel);
		public static ISymbol              GetDeclaredSymbol(this ArgumentSyntax                          node, SemanticModel semanticModel) =>	InternalGetDeclaredSymbol<ISymbol             >(node, semanticModel);
		public static INamedTypeSymbol     GetDeclaredSymbol(this TupleExpressionSyntax                   node, SemanticModel semanticModel) =>	InternalGetDeclaredSymbol<INamedTypeSymbol    >(node, semanticModel);
		public static INamedTypeSymbol     GetDeclaredSymbol(this AnonymousObjectCreationExpressionSyntax node, SemanticModel semanticModel) =>	InternalGetDeclaredSymbol<INamedTypeSymbol    >(node, semanticModel);
		public static IPropertySymbol      GetDeclaredSymbol(this AnonymousObjectMemberDeclaratorSyntax   node, SemanticModel semanticModel) =>	InternalGetDeclaredSymbol<IPropertySymbol     >(node, semanticModel);
		public static IEventSymbol         GetDeclaredSymbol(this EventDeclarationSyntax                  node, SemanticModel semanticModel) =>	InternalGetDeclaredSymbol<IEventSymbol        >(node, semanticModel);
		public static IPropertySymbol      GetDeclaredSymbol(this IndexerDeclarationSyntax                node, SemanticModel semanticModel) =>	InternalGetDeclaredSymbol<IPropertySymbol     >(node, semanticModel);
		public static IPropertySymbol      GetDeclaredSymbol(this PropertyDeclarationSyntax               node, SemanticModel semanticModel) =>	InternalGetDeclaredSymbol<IPropertySymbol     >(node, semanticModel);
		public static ISymbol              GetDeclaredSymbol(this BasePropertyDeclarationSyntax           node, SemanticModel semanticModel) =>	InternalGetDeclaredSymbol<ISymbol             >(node, semanticModel);
		public static IMethodSymbol        GetDeclaredSymbol(this BaseMethodDeclarationSyntax             node, SemanticModel semanticModel) =>	InternalGetDeclaredSymbol<IMethodSymbol       >(node, semanticModel);
		public static IFieldSymbol         GetDeclaredSymbol(this EnumMemberDeclarationSyntax             node, SemanticModel semanticModel) =>	InternalGetDeclaredSymbol<IFieldSymbol        >(node, semanticModel);
		public static INamedTypeSymbol     GetDeclaredSymbol(this DelegateDeclarationSyntax               node, SemanticModel semanticModel) =>	InternalGetDeclaredSymbol<INamedTypeSymbol    >(node, semanticModel);
		public static INamedTypeSymbol     GetDeclaredSymbol(this BaseTypeDeclarationSyntax               node, SemanticModel semanticModel) =>	InternalGetDeclaredSymbol<INamedTypeSymbol    >(node, semanticModel);
		public static INamespaceSymbol     GetDeclaredSymbol(this FileScopedNamespaceDeclarationSyntax    node, SemanticModel semanticModel) =>	InternalGetDeclaredSymbol<INamespaceSymbol    >(node, semanticModel);
		public static INamespaceSymbol     GetDeclaredSymbol(this NamespaceDeclarationSyntax              node, SemanticModel semanticModel) =>	InternalGetDeclaredSymbol<INamespaceSymbol    >(node, semanticModel);
		public static IMethodSymbol        GetDeclaredSymbol(this CompilationUnitSyntax                   node, SemanticModel semanticModel) => InternalGetDeclaredSymbol<IMethodSymbol       >(node, semanticModel);
		public static ISymbol              GetDeclaredSymbol(this MemberDeclarationSyntax                 node, SemanticModel semanticModel) => InternalGetDeclaredSymbol<ISymbol             >(node, semanticModel);
		private static T InternalGetDeclaredSymbol<T>(SyntaxNode node, SemanticModel semanticModel) where T : ISymbol
		{
			TryGetDeclaredSymbol(node, semanticModel, out T symbol);
			return symbol;
		}

		/// <summary>
		/// Prefer using <paramref name="node"/>.GetDeclaredSymbol(<see cref="SemanticModel"/>) when possible.
		/// </summary>
		public static bool TryGetDeclaredSymbol<T>(this SyntaxNode node, SemanticModel semanticModel, out T symbol) where T : ISymbol
		{
			if (node == null) throw new ArgumentNullException(nameof(node));
			if (semanticModel == null) throw new ArgumentNullException(nameof(semanticModel));

			ISymbol declaredSymbol;
			if (node is QueryContinuationSyntax qcon) declaredSymbol = semanticModel.GetDeclaredSymbol(qcon); // IRangeVariableSymbol?
			else if (node is JoinIntoClauseSyntax jnin) declaredSymbol = semanticModel.GetDeclaredSymbol(jnin); // IRangeVariableSymbol?
			else if (node is QueryClauseSyntax qury) declaredSymbol = semanticModel.GetDeclaredSymbol(qury); // IRangeVariableSymbol?
			else if (node is CatchDeclarationSyntax ctch) declaredSymbol = semanticModel.GetDeclaredSymbol(ctch); // ILocalSymbol?
			else if (node is ForEachStatementSyntax fore) declaredSymbol = semanticModel.GetDeclaredSymbol(fore); // ILocalSymbol?
			else if (node is TypeParameterSyntax tprm) declaredSymbol = semanticModel.GetDeclaredSymbol(tprm); // ITypeParameterSymbol?
			else if (node is ParameterSyntax prm) declaredSymbol = semanticModel.GetDeclaredSymbol(prm); // IParameterSymbol?
			else if (node is ExternAliasDirectiveSyntax exal) declaredSymbol = semanticModel.GetDeclaredSymbol(exal); // IAliasSymbol?
			else if (node is UsingDirectiveSyntax usgd) declaredSymbol = semanticModel.GetDeclaredSymbol(usgd); // IAliasSymbol?
			else if (node is SwitchLabelSyntax slbl) declaredSymbol = semanticModel.GetDeclaredSymbol(slbl); // ILabelSymbol?
			else if (node is LabeledStatementSyntax lbl) declaredSymbol = semanticModel.GetDeclaredSymbol(lbl); // ILabelSymbol?
			else if (node is TupleElementSyntax tple) declaredSymbol = semanticModel.GetDeclaredSymbol(tple); // ISymbol?
			else if (node is VariableDeclaratorSyntax dtor) declaredSymbol = semanticModel.GetDeclaredSymbol(dtor); // ISymbol?
			else if (node is SingleVariableDesignationSyntax svar) declaredSymbol = semanticModel.GetDeclaredSymbol(svar); // ISymbol?
			else if (node is AccessorDeclarationSyntax accs) declaredSymbol = semanticModel.GetDeclaredSymbol(accs); // IMethodSymbol?
			else if (node is ArgumentSyntax arg) declaredSymbol = semanticModel.GetDeclaredSymbol(arg); // ISymbol?
			else if (node is TupleExpressionSyntax tpl) declaredSymbol = semanticModel.GetDeclaredSymbol(tpl); // INamedTypeSymbol?
			else if (node is AnonymousObjectCreationExpressionSyntax aoce) declaredSymbol = semanticModel.GetDeclaredSymbol(aoce); // INamedTypeSymbol?
			else if (node is AnonymousObjectMemberDeclaratorSyntax aomd) declaredSymbol = semanticModel.GetDeclaredSymbol(aomd); // IPropertySymbol?
			else if (node is EventDeclarationSyntax evnt) declaredSymbol = semanticModel.GetDeclaredSymbol(evnt); // IEventSymbol?
			else if (node is IndexerDeclarationSyntax idxr) declaredSymbol = semanticModel.GetDeclaredSymbol(idxr); // IPropertySymbol?
			else if (node is PropertyDeclarationSyntax prop) declaredSymbol = semanticModel.GetDeclaredSymbol(prop); // IPropertySymbol?
			else if (node is BasePropertyDeclarationSyntax bprp) declaredSymbol = semanticModel.GetDeclaredSymbol(bprp); // ISymbol?
			else if (node is BaseMethodDeclarationSyntax meth) declaredSymbol = semanticModel.GetDeclaredSymbol(meth); // IMethodSymbol?
			else if (node is EnumMemberDeclarationSyntax emem) declaredSymbol = semanticModel.GetDeclaredSymbol(emem); // IFieldSymbol?
			else if (node is DelegateDeclarationSyntax delg) declaredSymbol = semanticModel.GetDeclaredSymbol(delg); // INamedTypeSymbol?
			else if (node is BaseTypeDeclarationSyntax type) declaredSymbol = semanticModel.GetDeclaredSymbol(type); // INamedTypeSymbol?
			else if (node is FileScopedNamespaceDeclarationSyntax fsns) declaredSymbol = semanticModel.GetDeclaredSymbol(fsns); // INamespaceSymbol?
			else if (node is NamespaceDeclarationSyntax ns) declaredSymbol = semanticModel.GetDeclaredSymbol(ns); // INamespaceSymbol?
			else if (node is CompilationUnitSyntax comp) declaredSymbol = semanticModel.GetDeclaredSymbol(comp); // IMethodSymbol?
			else if (node is MemberDeclarationSyntax mmbr) declaredSymbol = semanticModel.GetDeclaredSymbol(mmbr); // ISymbol?
			else declaredSymbol = semanticModel.GetDeclaredSymbol(node);

			if (declaredSymbol is T and not null)
			{
				symbol = (T)declaredSymbol;
				return true;
			}
			else
			{
				symbol = default;
				return false;
			}
		}




		public static ITypeSymbol GetSpeculativeTypeSymbol(this ExpressionSyntax expression, SemanticModel semanticModel, bool onlyTypeOrNamespace = false)
		{
			expression.InternalTryGetSpeculativeTypeSymbol(semanticModel, onlyTypeOrNamespace, out ITypeSymbol typeSymbol);
			return typeSymbol;
		}

		public static bool TryGetSpeculativeTypeSymbol<T>(this ExpressionSyntax expression, SemanticModel semanticModel, out T typeSymbol, bool onlyTypeOrNamespace = false)
			where T : ITypeSymbol
		{
			return expression.InternalTryGetSpeculativeTypeSymbol(semanticModel, onlyTypeOrNamespace, out typeSymbol);
		}

		private static bool InternalTryGetSpeculativeTypeSymbol<T>(this SyntaxNode node, SemanticModel semanticModel, bool onlyTypeOrNamespace, out T typeSymbol)
			where T : ITypeSymbol
		{
			int position = node.GetLocation().SourceSpan.Start;
			SpeculativeBindingOption bindingOption = onlyTypeOrNamespace ? SpeculativeBindingOption.BindAsTypeOrNamespace : SpeculativeBindingOption.BindAsExpression;

			TypeInfo typeInfo;
			if (node is ExpressionSyntax expr) typeInfo = semanticModel.GetSpeculativeTypeInfo(position, expr, bindingOption);
			else typeInfo = semanticModel.GetSpeculativeTypeInfo(position, node, bindingOption);

			if (typeInfo.Type is T tTypeSymbol)
			{
				typeSymbol = tTypeSymbol;
				return true;
			}

			typeSymbol = default;
			return false;
		}
	}
}
