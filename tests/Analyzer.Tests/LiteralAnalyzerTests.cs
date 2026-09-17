using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using ServiceStandards.Analyzers;
using Xunit;

namespace ServiceStandards.Analyzer.Tests;

/// <summary>
/// What the literal rule reports, and what it leaves alone.
/// </summary>
/// <remarks>
/// The analyzer runs against compiled snippets through Roslyn rather than a
/// testing harness, which keeps a second test framework out of the repo and
/// shows plainly what the compiler hands the rule.
///
/// This project references the analyzer as a library rather than as an
/// analyzer, so the rule does not run over its own test data.
/// </remarks>
public sealed class LiteralAnalyzerTests
{
    private const string CompilationName = "Snippet";
    private const string TrustedAssemblies = "TRUSTED_PLATFORM_ASSEMBLIES";

    private const string LiteralInAMethod = """
        internal static class Sample
        {
            public static string Describe() => "a meaningful value";
        }
        """;

    private const string NumberInAMethod = """
        internal static class Sample
        {
            public static int Limit() => 200;
        }
        """;

    private const string InterpolatedInAMethod = """
        internal static class Sample
        {
            public static string Greet(string name) => $"Hello {name}";
        }
        """;

    private const string TrivialValues = """
        internal static class Sample
        {
            public static string Blank() => "";
            public static int Zero() => 0;
            public static int One() => 1;
        }
        """;

    private const string NamedConstants = """
        internal static class Sample
        {
            public const string Key = "configuration-key";
            private const int Limit = 200;
            public static int Reveal() => Limit;
        }
        """;

    private const string AttributeArgument = """
        using System;

        internal sealed class SampleAttribute : Attribute
        {
            public SampleAttribute(string name) => Name = name;

            public string Name { get; }
        }

        [Sample("configuration")]
        internal static class Decorated
        {
        }
        """;

    private const string EnumMembers = """
        internal enum Level
        {
            Low = 5,
            High = 200,
        }
        """;

    private const string DefaultParameterValue = """
        internal static class Sample
        {
            public static int Grow(int amount = 25) => amount;
        }
        """;

    private const string SwitchCaseLabel = """
        internal static class Sample
        {
            public static int Rank(string value)
            {
                switch (value)
                {
                    case "alpha": return 1;
                    default: return 0;
                }
            }
        }
        """;

    private const string LocalConstant = """
        internal static class Sample
        {
            public static int Limit()
            {
                const int Ceiling = 200;
                return Ceiling;
            }
        }
        """;

    [Fact]
    public async Task ReportsAStringLiteralInAMethod()
    {
        var reported = await AnalyzeAsync(LiteralInAMethod);

        var single = Assert.Single(reported);
        Assert.Equal(LiteralAnalyzer.DiagnosticId, single.Id);
    }

    [Fact]
    public async Task ReportsANumberInAMethod()
    {
        var reported = await AnalyzeAsync(NumberInAMethod);

        Assert.Single(reported);
    }

    [Fact]
    public async Task ReportsAnInterpolatedStringInAMethod()
    {
        var reported = await AnalyzeAsync(InterpolatedInAMethod);

        Assert.Single(reported);
    }

    [Fact]
    public async Task LeavesTrivialValuesAlone()
    {
        var reported = await AnalyzeAsync(TrivialValues);

        Assert.Empty(reported);
    }

    [Fact]
    public async Task LeavesNamedConstantsAlone()
    {
        var reported = await AnalyzeAsync(NamedConstants);

        Assert.Empty(reported);
    }

    [Fact]
    public async Task LeavesALocalConstantAlone()
    {
        var reported = await AnalyzeAsync(LocalConstant);

        Assert.Empty(reported);
    }

    [Fact]
    public async Task LeavesAnAttributeArgumentAlone()
    {
        var reported = await AnalyzeAsync(AttributeArgument);

        Assert.Empty(reported);
    }

    [Fact]
    public async Task LeavesEnumMembersAlone()
    {
        var reported = await AnalyzeAsync(EnumMembers);

        Assert.Empty(reported);
    }

    [Fact]
    public async Task LeavesADefaultParameterValueAlone()
    {
        var reported = await AnalyzeAsync(DefaultParameterValue);

        Assert.Empty(reported);
    }

    [Fact]
    public async Task LeavesASwitchCaseLabelAlone()
    {
        var reported = await AnalyzeAsync(SwitchCaseLabel);

        Assert.Empty(reported);
    }

    private static async Task<ImmutableArray<Diagnostic>> AnalyzeAsync(string source)
    {
        var trusted = (string?)AppContext.GetData(TrustedAssemblies) ?? string.Empty;
        var references = trusted
            .Split(Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path));

        var compilation = CSharpCompilation.Create(
            CompilationName,
            [CSharpSyntaxTree.ParseText(source)],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(new LiteralAnalyzer());
        return await compilation
            .WithAnalyzers(analyzers)
            .GetAnalyzerDiagnosticsAsync(TestContext.Current.CancellationToken)
            .ConfigureAwait(false);
    }
}
