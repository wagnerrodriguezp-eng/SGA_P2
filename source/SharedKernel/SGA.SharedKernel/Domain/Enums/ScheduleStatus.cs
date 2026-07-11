namespace SGA.SharedKernel.Domain.Enums;

// Kept as its own named enum (rather than reusing RecordStatus directly) so a future status
// (e.g. TemporarilySuspended) can be added to schedules without affecting generic soft-delete semantics.
public enum ScheduleStatus
{
    Active = 1,
    Inactive = 2
}
