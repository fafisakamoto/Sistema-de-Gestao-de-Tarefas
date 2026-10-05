using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.Common.Pagination;
using TaskManager.Application.Tasks;
using TaskManager.Application.Tasks.Create;
using TaskManager.Application.Tasks.Delete;
using TaskManager.Application.Tasks.GetById;
using TaskManager.Application.Tasks.List;
using TaskManager.Application.Tasks.Update;

namespace TaskManager.Api.Controllers;

/// <summary>
/// Gerenciamento de tarefas.
/// </summary>
[Route("api/tasks")]
public sealed class TasksController : ApiControllerBase
{
    /// <summary>
    /// Lista tarefas com filtros, busca, ordenação e paginação.
    /// </summary>
    /// <remarks>
    /// Todos os parâmetros são opcionais e combináveis. Exemplos:
    ///
    ///     GET /api/tasks?status=Pending
    ///     GET /api/tasks?dueDateFrom=2026-10-01&amp;dueDateTo=2026-10-31
    ///     GET /api/tasks?search=relatório&amp;sortBy=CreatedAt&amp;sortDirection=Desc&amp;page=2&amp;pageSize=10
    ///
    /// Para filtrar por uma data exata, informe o mesmo valor em `dueDateFrom` e `dueDateTo`.
    /// </remarks>
    /// <response code="200">Página de tarefas (pode ser vazia).</response>
    /// <response code="400">Parâmetros inválidos.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<TaskResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest, "application/problem+json")]
    public async Task<ActionResult<PagedResult<TaskResponse>>> List(
        [FromQuery] ListTasksQuery query,
        [FromServices] ListTasksHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(query, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : ProblemFrom(result.Error);
    }

    /// <summary>
    /// Obtém uma tarefa pelo código.
    /// </summary>
    /// <param name="id">Código único da tarefa.</param>
    /// <response code="200">Tarefa encontrada.</response>
    /// <response code="404">Tarefa não encontrada.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<ActionResult<TaskResponse>> GetById(
        Guid id,
        [FromServices] GetTaskByIdHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : ProblemFrom(result.Error);
    }

    /// <summary>
    /// Cria uma tarefa.
    /// </summary>
    /// <remarks>
    /// Exemplo:
    ///
    ///     POST /api/tasks
    ///     {
    ///         "title": "Preparar apresentação da sprint",
    ///         "description": "Consolidar métricas e principais entregas do time",
    ///         "dueDate": "2026-12-31",
    ///         "status": "Pending"
    ///     }
    ///
    /// O código único gerado é retornado no campo `id` e no header `Location`.
    /// </remarks>
    /// <response code="201">Tarefa criada.</response>
    /// <response code="400">Dados inválidos.</response>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest, "application/problem+json")]
    public async Task<ActionResult<TaskResponse>> Create(
        [FromBody] CreateTaskRequest request,
        [FromServices] CreateTaskHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(request, cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value)
            : ProblemFrom(result.Error);
    }

    /// <summary>
    /// Atualiza uma tarefa (título, descrição, vencimento e status).
    /// </summary>
    /// <param name="id">Código único da tarefa.</param>
    /// <param name="request">Novos dados. Semântica de PUT: todos os campos são substituídos.</param>
    /// <response code="200">Tarefa atualizada.</response>
    /// <response code="400">Dados inválidos.</response>
    /// <response code="404">Tarefa não encontrada.</response>
    [HttpPut("{id:guid}")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<ActionResult<TaskResponse>> Update(
        Guid id,
        [FromBody] UpdateTaskRequest request,
        [FromServices] UpdateTaskHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : ProblemFrom(result.Error);
    }

    /// <summary>
    /// Exclui uma tarefa.
    /// </summary>
    /// <param name="id">Código único da tarefa.</param>
    /// <response code="204">Tarefa excluída.</response>
    /// <response code="404">Tarefa não encontrada.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<ActionResult> Delete(
        Guid id,
        [FromServices] DeleteTaskHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(id, cancellationToken);
        return result.IsSuccess ? NoContent() : ProblemFrom(result.Error);
    }
}
