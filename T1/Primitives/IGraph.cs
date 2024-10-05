namespace Task1.Implementation.Primitives;

public interface IGraph
{
    bool IsDirected { get; }
    int VerticesCount { get; }
    int EdgesCount { get; }
}