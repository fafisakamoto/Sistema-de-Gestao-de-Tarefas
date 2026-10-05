using System.Text.Json.Serialization;
using TaskManager.Api.ErrorHandling;
using TaskManager.Api.Extensions;
using TaskManager.Api.Middleware;
using TaskManager.Application;
using TaskManager.Infrastructure;
using TaskManager.Infrastructure.Persistence;
using TaskManager.Infrastructure.Persistence.Seed;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers(options => options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true)
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false)));

builder.Services
    .AddApplication()
    .AddInfrastructure();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddSwaggerDocumentation();
builder.Services.AddHealthChecks().AddDbContextCheck<AppDbContext>("database");

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.DocumentTitle = "Task Manager API";
    options.DisplayRequestDuration();
});

app.MapControllers();
app.MapHealthChecks("/health");
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

if (app.Environment.IsDevelopment())
    await app.Services.SeedDevelopmentDataAsync();

await app.RunAsync();

public partial class Program { }
