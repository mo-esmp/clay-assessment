namespace SmartLock.Domain;

[Serializable]
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }
}