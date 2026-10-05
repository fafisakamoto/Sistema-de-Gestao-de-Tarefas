using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace TaskManager.Api.Extensions;

internal static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Task Manager API",
                Version = "v1",
                Description = "API RESTful para gestão de tarefas: cadastro, listagem com filtros, busca e paginação, edição e exclusão. " +
                              "Erros seguem o padrão ProblemDetails (RFC 9457). Toda resposta inclui o header X-Correlation-Id."
            });

            options.SupportNonNullableReferenceTypes();

            var dateSchema = new OpenApiSchema { Type = "string", Format = "date", Example = new OpenApiString("2026-12-31") };
            options.MapType<DateOnly>(() => dateSchema);
            options.MapType<DateOnly?>(() => new OpenApiSchema
            {
                Type = "string",
                Format = "date",
                Nullable = true,
                Example = new OpenApiString("2026-12-31")
            });

            foreach (var xmlFile in Directory.EnumerateFiles(AppContext.BaseDirectory, "TaskManager.*.xml"))
                options.IncludeXmlComments(xmlFile, includeControllerXmlComments: true);
        });

        return services;
    }
}
