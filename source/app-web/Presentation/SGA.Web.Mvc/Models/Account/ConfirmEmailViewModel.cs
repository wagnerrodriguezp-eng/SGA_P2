namespace SGA.Web.Mvc.Models.Account;

public class ConfirmEmailViewModel
{
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
}
