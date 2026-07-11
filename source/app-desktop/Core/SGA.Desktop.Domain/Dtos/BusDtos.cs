using System.ComponentModel.DataAnnotations;
using SGA.SharedKernel.Domain.Enums;

namespace SGA.Desktop.Domain.Dtos;

public class CreateBusDto
{
    [Required, MaxLength(15)]
    public string PlateNumber { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Model { get; set; }

    public int? Year { get; set; }

    [Required]
    public int Capacity { get; set; }
}

public class UpdateBusDto
{
    [MaxLength(100)]
    public string? Model { get; set; }

    public int? Year { get; set; }

    [Required]
    public int Capacity { get; set; }

    [Required]
    public BusStatus BusStatus { get; set; }
}
