namespace SGA.SharedKernel.Domain.Enums;

public enum AccessResult
{
    Granted = 1,
    DeniedExpired = 2,
    DeniedNoBalance = 3,
    DeniedNoCapacity = 4,
    DeniedUnauthorized = 5
}
