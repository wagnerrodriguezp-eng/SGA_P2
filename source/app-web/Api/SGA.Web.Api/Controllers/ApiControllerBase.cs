using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using SGA.SharedKernel.Application.Results;

namespace SGA.Web.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    protected Guid CurrentUserId => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    protected IActionResult FromResult(OperationResult result) =>
        result.IsSuccess ? Ok(result) : StatusCode(MapStatus(result.Status), result);

    protected IActionResult FromResult<T>(OperationResult<T> result) =>
        result.IsSuccess ? Ok(result) : StatusCode(MapStatus(result.Status), result);

    private static int MapStatus(OperationResultStatus status) => status switch
    {
        OperationResultStatus.Success => StatusCodes.Status200OK,
        OperationResultStatus.ValidationError => StatusCodes.Status400BadRequest,
        OperationResultStatus.NotFound => StatusCodes.Status404NotFound,
        OperationResultStatus.Conflict => StatusCodes.Status409Conflict,
        OperationResultStatus.Unauthorized => StatusCodes.Status401Unauthorized,
        OperationResultStatus.Forbidden => StatusCodes.Status403Forbidden,
        _ => StatusCodes.Status500InternalServerError
    };
}
