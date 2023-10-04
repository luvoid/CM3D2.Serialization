
using CM3D2.Serialization.Files;
using CM3D2.Serialization.SourceGenerators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;


namespace CM3D2.Serialization.Tests
{
	// See "Solution B":
	// https://github.com/dotnet/roslyn/blob/main/docs/features/source-generators.cookbook.md#unit-testing-of-generators

	[TestClass]
	public class SourceGeneratorTests
	{
		[TestMethod]
		public void CM3D2SerializableAnalyzerTest()
		{
			var inputCompilation = CreateCompilationFromFileWithAnalyzers(
				"Resources/CM3D2SerializableAnalyzerTestSource.cs",
				out var analyzerExceptions,
				new CM3D2SerializableAnalyzer()
			);

			//var diagnostics = inputCompilation.GetAllDiagnosticsAsync().GetAwaiter().GetResult();
			var result = inputCompilation.GetAnalysisResultAsync(default).GetAwaiter().GetResult();

			foreach (var diagnostic in result.GetAllDiagnostics())
			{
				if (!diagnostic.Id.StartsWith("CM3D2")) continue;
				Console.WriteLine(diagnostic);
			}

			Assert.That.ThrewNoExceptions(analyzerExceptions);
		}

		[TestMethod]
		public void LengthDefinedByAnalyzerTest()
		{
			var inputCompilation = CreateCompilationFromFileWithAnalyzers(
				"Resources/LengthDefinedByAnalyzerTestSource.cs",
				out var analyzerExceptions,
				new LengthDefinedByAnalyzer()
			);

			//var diagnostics = inputCompilation.GetAllDiagnosticsAsync().GetAwaiter().GetResult();
			var result = inputCompilation.GetAnalysisResultAsync(default).GetAwaiter().GetResult();

			foreach (var diagnostic in result.GetAllDiagnostics())
			{
				if (!diagnostic.Id.StartsWith("CM3D2")) continue;
				Console.WriteLine(diagnostic);
			}

			Assert.That.ThrewNoExceptions(analyzerExceptions);
		}

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

		private static Compilation CreateCompilationFromFile(string filePath)
		{
			using var file = new FileStream(filePath, FileMode.Open, FileAccess.Read);
			return CreateCompilation(new StreamReader(file).ReadToEnd(), filePath);
		}

		private static CompilationWithAnalyzers CreateCompilationFromFileWithAnalyzers(
			string filePath, out ReadOnlyDictionary<DiagnosticAnalyzer, IList<Exception>> analyzerExceptions, params DiagnosticAnalyzer[] analyzers)
		{
			Dictionary<DiagnosticAnalyzer, IList<Exception>>  exceptions = new();
			analyzerExceptions = new(exceptions);

			void OnAnayzerException(Exception exception, DiagnosticAnalyzer analyzer, Diagnostic diagnostic)
			{
				if (!exceptions.TryGetValue(analyzer, out IList<Exception> list))
					exceptions[analyzer] = list = new List<Exception>();
				list.Add(exception);
			}

			return CreateCompilationFromFile(filePath).WithAnalyzers(
				ImmutableArray.Create(analyzers),
				new CompilationWithAnalyzersOptions(new AnalyzerOptions(default), OnAnayzerException, true, false)
			);
		}

		private static Compilation CreateCompilation(string source, string path = "")
			=> CSharpCompilation.Create("compilation",
				new[] { CSharpSyntaxTree.ParseText(source, path: path) },
				new[] { 
					MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location),
					MetadataReference.CreateFromFile(typeof(CM3D2Serializer).GetTypeInfo().Assembly.Location),
				},
				new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

	}
}

