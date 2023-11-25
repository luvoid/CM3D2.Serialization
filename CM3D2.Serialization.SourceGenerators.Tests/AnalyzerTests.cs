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
	public class AnalyzerTests : CompilationTestsBase
	{
		[TestMethod]
		public void TypeNameTest()
		{
			Console.WriteLine(typeof(List<int>).Name);
		}

		[TestMethod]
		public void CM3D2SerializableAnalyzerTest()
		{
			var diagnostics = TestAnalyzersWithFile(
				"Resources/CM3D2SerializableAnalyzerTestSource.cs",
				new CM3D2SerializableAnalyzer()
			);

			Console.Out.WriteAll(diagnostics);
		}

		[TestMethod]
		public void ReadWriteWithAnalyzerTest()
		{
			var diagnostics = TestAnalyzersWithFile(
				"Resources/CM3D2SerializableAnalyzerTestSource.cs",
				new ReadWriteWithAnalyzer()
			);

			Console.Out.WriteAll(diagnostics);
		}

		[TestMethod]
		public void ReadWriteWithAnalyzerResolutionTest()
		{
			var diagnostics = TestAnalyzersWithFile(
				"Resources/ReadWriteWithAnalyzerResolutionTestSource.cs",
				new ReadWriteWithAnalyzer()
			);

			Console.Out.WriteAll(diagnostics);
		}

		[TestMethod]
		public void LengthDefinedByAnalyzerTest()
		{
			var diagnostics = TestAnalyzersWithFile(
				"Resources/LengthDefinedByAnalyzerTestSource.cs",
				new LengthDefinedByAnalyzer()
			);
		}

		[TestMethod]
		public void UnmanagedSerializableAnalyzerTest()
		{
			var diagnostics = TestAnalyzersWithFile(
				"Resources/UnmanagedSerializableAnalyzerTestSource.cs",
				new UnmanagedSerializableAnalyzer()
			);

			Console.Out.WriteAll(diagnostics);
		}

		private static ImmutableArray<Diagnostic> TestAnalyzersWithFile(string filePath, params DiagnosticAnalyzer[] analyzers)
		{
			var inputCompilation = CreateCompilationFromFileWithAnalyzers(
				filePath,
				out var analyzerExceptions,
				analyzers
			);

			//var diagnosticsAsync = inputCompilation.GetAllDiagnosticsAsync().GetAwaiter().GetResult();
			var result = inputCompilation.GetAnalysisResultAsync(default).GetAwaiter().GetResult();

			List<Diagnostic> diagnostics = new();
			foreach (var analyzer in analyzers)
			{
				diagnostics.AddRange(result.GetAllDiagnostics(analyzer));
			}

			Assert.That.ThrewNoExceptions(analyzerExceptions);

			return diagnostics.ToImmutableArray();
		}

		private static CompilationWithAnalyzers CreateCompilationFromFileWithAnalyzers(
			string filePath, out ReadOnlyDictionary<DiagnosticAnalyzer, IList<Exception>> analyzerExceptions, params DiagnosticAnalyzer[] analyzers)
		{
			Dictionary<DiagnosticAnalyzer, IList<Exception>> exceptions = new();
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
	}
}

