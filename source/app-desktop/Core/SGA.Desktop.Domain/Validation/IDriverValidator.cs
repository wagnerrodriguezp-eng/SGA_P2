using SGA.SharedKernel.Domain.Entities;
using SGA.SharedKernel.Domain.Validation;
using SGA.Desktop.Domain.Dtos;

namespace SGA.Desktop.Domain.Validation;

public interface IDriverValidator : IValidator<DriverProfile, CreateDriverDto, UpdateDriverDto>
{
}
