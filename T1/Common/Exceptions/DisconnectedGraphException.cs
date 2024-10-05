namespace Task1.Implementation.Common.Exceptions;

public class DisconnectedGraphException : Exception
{
    public DisconnectedGraphException() : base() {}
    
    public DisconnectedGraphException(string message) : base(message) {}
}