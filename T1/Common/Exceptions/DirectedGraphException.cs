namespace Task1.Implementation.Common.Exceptions;

public class DirectedGraphException : Exception
{
    public DirectedGraphException() : base() {}
    
    public DirectedGraphException(string message) : base(message) {}
}