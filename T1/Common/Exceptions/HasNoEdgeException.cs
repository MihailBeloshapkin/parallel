namespace Task1.Implementation.Common.Exceptions;

public class HasNoEdgeException : Exception
{
    public HasNoEdgeException() : base() {}
    
    public HasNoEdgeException(string message) : base(message) {}
}