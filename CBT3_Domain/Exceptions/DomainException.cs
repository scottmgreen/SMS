namespace CBT3_Domain.Exceptions;

public abstract class DomainException :Exception
{
    protected DomainException(string message):base(message)
    { }
}
