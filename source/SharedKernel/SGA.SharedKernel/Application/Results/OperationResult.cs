namespace SGA.SharedKernel.Application.Results;

public enum OperationResultStatus
{
    Success = 1,
    ValidationError = 2,
    NotFound = 3,
    Conflict = 4,
    Unauthorized = 5,
    Forbidden = 6,
    UnexpectedError = 7
}

public class OperationResult
{
    public bool IsSuccess => Status == OperationResultStatus.Success;
    public OperationResultStatus Status { get; init; }
    public string? Message { get; init; }
    public IReadOnlyCollection<string> Errors { get; init; } = Array.Empty<string>();

    public static OperationResult Success(string? message = null) =>
        new() { Status = OperationResultStatus.Success, Message = message };

    public static OperationResult Failure(OperationResultStatus status, params string[] errors) =>
        new() { Status = status, Errors = errors };
}

public class OperationResult<T> : OperationResult
{
    public T? Data { get; init; }

    public static OperationResult<T> Success(T data, string? message = null) =>
        new() { Status = OperationResultStatus.Success, Data = data, Message = message };

    public static new OperationResult<T> Failure(OperationResultStatus status, params string[] errors) =>
        new() { Status = status, Errors = errors };
}
