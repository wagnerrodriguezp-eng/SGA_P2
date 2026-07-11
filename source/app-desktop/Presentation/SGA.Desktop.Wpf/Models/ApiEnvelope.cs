namespace SGA.Desktop.Wpf.Models;

// Mirrors SGA.SharedKernel's OperationResult/OperationResult<T> JSON contract. Not shared via a
// project reference — SGA.Desktop.Wpf talks to its own API purely over HTTP (true BFF isolation),
// same principle already applied to SGA.Web.Mvc.
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
