using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGA.SharedKernel.Application.Results;

namespace SGA.Desktop.Api.Controllers;

// Every endpoint in this API requires TransportAdminOnly except /api/auth/* — declared explicitly
// here (never "no [Authorize] at all") so a future endpoint can't accidentally ship unauthenticated.
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Policy = "TransportAdminOnly")]
public abstract class ApiControllerBase : ControllerBase
{
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
