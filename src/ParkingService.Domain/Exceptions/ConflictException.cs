namespace ParkingService.Domain.Exceptions;

public class ConflictException : DomainException
{
    public ConflictException(string message) : base(message) { }

    public ConflictException(string entityName, string key, object value)
        : base($"{entityName} with {key} '{value}' already exists.") { }
}
