using System.ComponentModel.DataAnnotations;

namespace SGA.Web.Mvc.Models;

public enum IncidentTypeOption
{
    Delay = 1,
    MechanicalIssue = 2,
    TrafficIncident = 3,
    PassengerIssue = 4,
    Other = 5
}

public class ReportIncidentViewModel
{
    [Required]
    public Guid TripId { get; set; }

    [Required]
    [Display(Name = "Incident type")]
    public IncidentTypeOption IncidentType { get; set; }

    [Required, MaxLength(500)]
    public string Description { get; set; } = string.Empty;
}
