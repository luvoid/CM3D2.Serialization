using CM3D2.Serialization.SourceGenerators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;


namespace CM3D2.Serialization.SourceGenerators.Tests
{
	public class CompilationTestsBase
	{
		protected static Compilation CreateCompilationFromFile(string filePath)
		{
			using var file = new FileStream(filePath, FileMode.Open, FileAccess.Read);
			return CreateCompilation(new StreamReader(file).ReadToEnd(), filePath);
		}

		protected static Compilation CreateCompilation(string source, string path = "")
			=> CSharpCompilation.Create("compilation",
				new[] { CSharpSyntaxTree.ParseText(source, path: path) },
				new[] {
					MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location),
					MetadataReference.CreateFromFile(typeof(CM3D2Serializer).GetTypeInfo().Assembly.Location),
				},
				new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

	}
}

