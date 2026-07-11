namespace SGA.Web.Mvc.Models.Api;

public class TokenResponseModel
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
}
