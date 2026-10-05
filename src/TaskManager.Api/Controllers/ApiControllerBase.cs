using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using TaskManager.Application.Common.Results;

namespace TaskManager.Api.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    private const string ErrorCodeKey = "errorCode";

    protected ActionResult ProblemFrom(Error error)
    {
        if (error.Type == ErrorType.Validation)
        {
            var modelState = new ModelStateDictionary();
            foreach (var (field, messages) in error.ValidationErrors)
            {
                foreach (var message in messages)
                    modelState.AddModelError(field, message);
            }

            return WithErrorCode(ValidationProblem(title: error.Message, modelStateDictionary: modelState), error);
        }

        var (statusCode, title) = error.Type switch
        {
            ErrorType.NotFound => (StatusCodes.Status404NotFound, "Recurso não encontrado."),
            _ => (StatusCodes.Status400BadRequest, "Requisição inválida.")
        };

        return WithErrorCode(Problem(statusCode: statusCode, title: title, detail: error.Message), error);
    }

    private static ActionResult WithErrorCode(ActionResult result, Error error)
    {
        if (result is ObjectResult { Value: ProblemDetails details })
            details.Extensions[ErrorCodeKey] = error.Code;

        return result;
    }
}
