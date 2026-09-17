using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ServiceStandards.Analyzers;

/// <summary>
/// Reports every string or numeric literal that carries meaning beyond its own
/// expression and does not live in a named constant.
/// </summary>
/// <remarks>
/// go-standards enforces this rule by walking the syntax tree in a test, which
/// reaches every literal rather than the few call sites a lint rule matches.
/// C# offers a stronger place to stand: an analyzer runs during compilation, so
/// with warnings as errors a stray literal fails the build rather than a test
/// run. The permitted set mirrors the Go original: a literal may appear where it
/// is itself the named value.
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class LiteralAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "SSD001";

    private const string Category = "Naming";
    private const string Title = "Literal lives outside a named constant";
    private const string MessageFormat = "Move the literal {0} into a named constant";
    private const string Description =
        "Every literal carrying meaning beyond its immediate expression belongs in a named constant: " +
        "configuration keys, routes, SQL, log messages, error codes, user-facing copy, and test data.";

    private const int EmptyLength = 0;
    private const int TrivialZero = 0;
    private const int TrivialOne = 1;

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(
            Analyze,
            SyntaxKind.StringLiteralExpression,
            SyntaxKind.Utf8StringLiteralExpression,
            SyntaxKind.NumericLiteralExpression,
            SyntaxKind.InterpolatedStringExpression);
    }

    private static void Analyze(SyntaxNodeAnalysisContext context)
    {
        var node = context.Node;
        if (IsTrivial(node) || IsPermitted(node))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, node.GetLocation(), node.ToString()));
    }

    /// <summary>
    /// The empty string, zero, and one carry no meaning worth naming, matching
    /// the Go conventions test.
    /// </summary>
    private static bool IsTrivial(SyntaxNode node)
    {
        if (node is not LiteralExpressionSyntax literal)
        {
            return false;
        }

        return literal.Token.Value switch
        {
            string text => text.Length == EmptyLength,
            int number => number is TrivialZero or TrivialOne,
            _ => false,
        };
    }

    /// <summary>
    /// A literal may appear where it is itself the named value: const and enum
    /// declarations, attribute arguments such as xUnit's InlineData, switch
    /// labels over constants, nameof, and default parameter values.
    /// </summary>
    private static bool IsPermitted(SyntaxNode node)
    {
        foreach (var ancestor in node.Ancestors())
        {
            switch (ancestor)
            {
                case AttributeSyntax:
                case EnumMemberDeclarationSyntax:
                case ParameterSyntax:
                case ConstantPatternSyntax:
                case CaseSwitchLabelSyntax:
                    return true;

                case FieldDeclarationSyntax field when IsConstant(field.Modifiers):
                    return true;

                case LocalDeclarationStatementSyntax local when local.IsConst:
                    return true;

                case MethodDeclarationSyntax:
                case PropertyDeclarationSyntax:
                    return false;
            }
        }

        return false;
    }

    private static bool IsConstant(SyntaxTokenList modifiers) =>
        modifiers.Any(modifier => modifier.IsKind(SyntaxKind.ConstKeyword));
}
