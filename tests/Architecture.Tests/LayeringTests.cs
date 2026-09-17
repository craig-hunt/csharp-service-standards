using System.Reflection;
using NetArchTest.Rules;
using ServiceStandards.Application.Abstractions;
using ServiceStandards.Domain.Tasks;
using ServiceStandards.Infrastructure.Persistence;
using Xunit;

namespace ServiceStandards.Architecture.Tests;

/// <summary>
/// The layering rules, asserted rather than described.
/// </summary>
/// <remarks>
/// A dependency arrow that turns around is easy to add and hard to notice in
/// review, and it stays harmless right up to the day someone needs to test the
/// domain without a database. These tests fail the build instead.
/// </remarks>
public sealed class LayeringTests
{
    private const string DomainNamespace = "ServiceStandards.Domain";
    private const string ApplicationNamespace = "ServiceStandards.Application";
    private const string WebNamespace = "ServiceStandards.Web";
    private const string StoreSuffix = "Store";
    private const string Separator = ", ";
    private const string NoFailures = "none";

    private static readonly Assembly DomainAssembly = typeof(TaskItem).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(ITaskStore).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(ServiceStandardsDbContext).Assembly;

    [Theory]
    [InlineData("ServiceStandards.Application")]
    [InlineData("ServiceStandards.Infrastructure")]
    [InlineData("ServiceStandards.Web")]
    [InlineData("Microsoft.EntityFrameworkCore")]
    [InlineData("Microsoft.AspNetCore")]
    [InlineData("Npgsql")]
    public void DomainDependsOnNothingOutsideItself(string forbidden)
    {
        var result = Types.InAssembly(DomainAssembly)
            .Should()
            .NotHaveDependencyOn(forbidden)
            .GetResult();

        Assert.True(result.IsSuccessful, Describe(result));
    }

    [Theory]
    [InlineData("ServiceStandards.Infrastructure")]
    [InlineData("ServiceStandards.Web")]
    [InlineData("Microsoft.EntityFrameworkCore")]
    [InlineData("Npgsql")]
    public void ApplicationReachesOnlyInward(string forbidden)
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .Should()
            .NotHaveDependencyOn(forbidden)
            .GetResult();

        Assert.True(result.IsSuccessful, Describe(result));
    }

    [Fact]
    public void InfrastructureNeverReachesTheWebLayer()
    {
        var result = Types.InAssembly(InfrastructureAssembly)
            .Should()
            .NotHaveDependencyOn(WebNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, Describe(result));
    }

    [Fact]
    public void DomainNamespacesStayInsideTheDomainAssembly()
    {
        var result = Types.InAssembly(DomainAssembly)
            .Should()
            .ResideInNamespaceStartingWith(DomainNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, Describe(result));
    }

    [Fact]
    public void EveryStorePortIsAnInterface()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .ResideInNamespaceStartingWith(ApplicationNamespace)
            .And()
            .HaveNameEndingWith(StoreSuffix)
            .Should()
            .BeInterfaces()
            .GetResult();

        Assert.True(result.IsSuccessful, Describe(result));
    }

    private static string Describe(NetArchTest.Rules.TestResult result) =>
        result.FailingTypeNames is null
            ? NoFailures
            : string.Join(Separator, result.FailingTypeNames);
}
