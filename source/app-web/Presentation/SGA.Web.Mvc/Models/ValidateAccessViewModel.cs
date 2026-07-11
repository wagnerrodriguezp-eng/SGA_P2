using System.ComponentModel.DataAnnotations;

namespace SGA.Web.Mvc.Models;

public class ValidateAccessViewModel
{
    [Required, MaxLength(20)]
    [Display(Name = "Ticket / card code")]
    public string AuthorizationIdentifier { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Trip")]
    public Guid TripId { get; set; }
}
