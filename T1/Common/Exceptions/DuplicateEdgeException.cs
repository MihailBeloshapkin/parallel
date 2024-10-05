namespace Task1.Implementation.Common.Exceptions;

public class DuplicateEdgeException : Exception
{
    public DuplicateEdgeException() : base() {}
    
    public DuplicateEdgeException(string message) : base(message) {}
}