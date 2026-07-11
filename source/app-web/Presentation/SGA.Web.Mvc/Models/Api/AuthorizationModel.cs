namespace SGA.Web.Mvc.Models.Api;

public class AuthorizationModel
{
    public Guid Id { get; set; }
    public int AuthorizationType { get; set; }
    public int AuthorizationStatus { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public int? Balance { get; set; }
    public DateTime IssuedAtUtc { get; set; }
}
