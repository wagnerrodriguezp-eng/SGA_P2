namespace SGA.Web.Mvc.Models.Api;

// Mirrors the shape of SGA.SharedKernel's OperationResult/OperationResult<T> JSON contract.
// Deliberately not shared via a project reference — SGA.Web.Mvc talks to its own API purely over
// HTTP, decoupled from the server-side Application/SharedKernel assemblies (true BFF isolation).
public class ApiEnvelope
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class ApiEnvelope<T> : ApiEnvelope
{
    public T? Data { get; set; }
}
