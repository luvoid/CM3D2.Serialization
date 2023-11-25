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
	// See "Solution B":
	// https://github.com/dotnet/roslyn/blob/main/docs/features/source-generators.cookbook.md#unit-testing-of-generators

	[TestClass]
	public class SourceGeneratorTests : CompilationTestsBase
	{
		[TestMethod]
		public void FileVersionConstraintCommentGeneratorTest()
		{
			Compilation inputCompilation = CreateCompilationFromFile(
				"Resources/FileVersionConstraintCommentGeneratorTestSource.cs"
			);

			FileVersionConstraintCommentGenerator generator = new();
			GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

			// (Note: the generator driver itself is immutable, and all calls return an updated version of the driver that you should use for subsequent calls)
			driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);

			foreach (var syntaxTree in outputCompilation.SyntaxTrees)
			{
				string filename = Path.GetFileName(syntaxTree.FilePath);
				if (string.IsNullOrEmpty(filename))
					filename = "Unknown File";
				Console.WriteLine($"- - - - {filename} - - - - \n{syntaxTree}\n- - - - End of {filename} - - - -");
			}

			// The runResult contains the combined results of all generators passed to the driver
			GeneratorDriverRunResult runResult = driver.GetRunResult();
			Assert.That.ThrewNoExceptions(runResult);
		}
	}
}

