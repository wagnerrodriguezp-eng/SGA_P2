namespace SGA.SharedKernel.Application.Persistence;

// Thrown by IGenericRepository.SaveChangesAsync implementations when a row's concurrency token
// (e.g. Trip.RowVersion) no longer matches — translates the persistence-specific concurrency
// exception into an EF-agnostic type the Application layer is allowed to catch.
public class ConcurrencyConflictException : Exception
{
    public ConcurrencyConflictException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
