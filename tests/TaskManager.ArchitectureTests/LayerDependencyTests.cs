using System.Reflection;
using NetArchTest.Rules;
using TaskManager.Application.Tasks;
using TaskManager.Domain.Tasks;
using TaskManager.Infrastructure.Persistence;

namespace TaskManager.ArchitectureTests;

public sealed class LayerDependencyTests
{
    private const string ApplicationNamespace = "TaskManager.Application";
    private const string InfrastructureNamespace = "TaskManager.Infrastructure";
    private const string ApiNamespace = "TaskManager.Api";

    private static readonly Assembly Domain = typeof(TaskItem).Assembly;
    private static readonly Assembly Application = typeof(ITaskRepository).Assembly;
    private static readonly Assembly Infrastructure = typeof(AppDbContext).Assembly;
    private static readonly Assembly Api = typeof(Program).Assembly;

    private static void AssertSuccessful(TestResult result) =>
        Assert.True(result.IsSuccessful,
            "Tipos violando a regra: " + string.Join(", ", result.FailingTypeNames ?? Enumerable.Empty<string>()));

    [Fact]
    public void Domain_DependsOnNothingElse() =>
        AssertSuccessful(Types.InAssembly(Domain)
            .ShouldNot()
            .HaveDependencyOnAny(ApplicationNamespace, InfrastructureNamespace, ApiNamespace,
                "Microsoft.EntityFrameworkCore", "Microsoft.AspNetCore", "FluentValidation")
            .GetResult());

    [Fact]
    public void Application_DoesNotDependOnInfrastructureOrHttp() =>
        AssertSuccessful(Types.InAssembly(Application)
            .ShouldNot()
            .HaveDependencyOnAny(InfrastructureNamespace, ApiNamespace,
                "Microsoft.EntityFrameworkCore", "Microsoft.AspNetCore")
            .GetResult());

    [Fact]
    public void Infrastructure_DoesNotDependOnApi() =>
        AssertSuccessful(Types.InAssembly(Infrastructure)
            .ShouldNot()
            .HaveDependencyOn(ApiNamespace)
            .GetResult());

    [Fact]
    public void Controllers_DoNotAccessInfrastructure() =>
        AssertSuccessful(Types.InAssembly(Api)
            .That().ResideInNamespace("TaskManager.Api.Controllers")
            .ShouldNot()
            .HaveDependencyOn(InfrastructureNamespace)
            .GetResult());

    [Fact]
    public void Handlers_AreSealed() =>
        AssertSuccessful(Types.InAssembly(Application)
            .That().HaveNameEndingWith("Handler")
            .Should().BeSealed()
            .GetResult());

    [Fact]
    public void Handlers_DoNotDependOnOtherHandlers()
    {
        var handlers = Types.InAssembly(Application)
            .That().HaveNameEndingWith("Handler")
            .GetTypes()
            .ToList();

        Assert.NotEmpty(handlers);

        foreach (var handler in handlers)
        {
            var otherHandlers = handlers
                .Where(other => other != handler)
                .Select(other => other.FullName!)
                .ToArray();

            AssertSuccessful(Types.InAssembly(Application)
                .That().HaveName(handler.Name)
                .ShouldNot().HaveDependencyOnAny(otherHandlers)
                .GetResult());
        }
    }

    [Fact]
    public void Repositories_AreNotPublic() =>
        AssertSuccessful(Types.InAssembly(Infrastructure)
            .That().ImplementInterface(typeof(ITaskRepository))
            .Should().NotBePublic()
            .GetResult());

    [Fact]
    public void ConcreteClasses_AreSealedByDefault() =>
        AssertSuccessful(Types.InAssemblies(new[] { Domain, Application, Infrastructure })
            .That().ResideInNamespace("TaskManager")
            .And().AreClasses()
            .And().AreNotAbstract()
            .And().DoNotHaveNameMatching("^<")
            .And().DoNotHaveName("Result")
            .Should().BeSealed()
            .GetResult());
}
