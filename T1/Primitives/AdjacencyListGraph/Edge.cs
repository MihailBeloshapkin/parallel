using System.Runtime.Serialization;

namespace Task1.Implementation.Primitives.AdjacencyListGraph;

/// <summary>
/// Represents immutable edge.
/// </summary>
/// <typeparam name="V">Vertex type</typeparam>
/// <typeparam name="E">Edge type</typeparam>
[Serializable]
public struct Edge<V, E> : ISerializable
    where V : IParsable<V>, IEquatable<V>
    where E : IParsable<E>, IEquatable<E>, IComparable<E>
{
    public V FirstVertex { get; }
    public V SecondVertex { get; }
    public E Weight { get; }
    public bool IsDirected { get; }

    public Edge(V v1, V v2, E e, bool isDirected = false)
    {
        FirstVertex = v1;
        SecondVertex = v2;
        Weight = e;
        IsDirected = isDirected;
    }
    
    public Edge((V v1, V v2, E e) edge)
    {
        FirstVertex = edge.v1;
        SecondVertex = edge.v2;
        Weight = edge.e;
        IsDirected = false;
    }
    
    /// <summary>
    /// Deserealization constructor
    /// </summary>
    public Edge(SerializationInfo info, StreamingContext ctxt)
    {
        FirstVertex = (V)info.GetValue("FirstVertex", typeof(V));
        SecondVertex = (V)info.GetValue("SecondVertex", typeof(V));
        Weight = (E)info.GetValue("Weight", typeof(E));
        IsDirected = (bool)info.GetValue("IsDirected", typeof(bool));
    }
    
    public static implicit operator Edge<V, E>((V v1, V v2, E e) edge)
    {
        return new Edge<V, E>(edge);
    }

    public bool HasVertex(V v)
    {
        return FirstVertex.Equals(v) || SecondVertex.Equals(v);
    }
    
    public static bool operator == (Edge<V, E> a, Edge<V, E> b)
    {
        if (a.IsDirected != b.IsDirected) 
            return false;

        bool directed = a.IsDirected;

        return directed
            ? a.FirstVertex.Equals(b.FirstVertex) && a.SecondVertex.Equals(b.SecondVertex) && a.Weight.Equals(b.Weight)
            : a.Weight.Equals(b.Weight) && (
                (a.FirstVertex.Equals(b.FirstVertex) && a.SecondVertex.Equals(b.SecondVertex)) ||
                (a.FirstVertex.Equals(b.SecondVertex) && a.SecondVertex.Equals(b.FirstVertex)));
    }
    
    public static bool operator != (Edge<V, E> a, Edge<V, E> b)
    {
        return !(a == b);
    }
    
    public static bool operator == (Edge<V, E> a, (V v1, V v2, E e) b)
    {
        return a.IsDirected
            ? a.FirstVertex.Equals(b.v1) && a.SecondVertex.Equals(b.v2) && a.Weight.Equals(b.e)
            : a.Weight.Equals(b.e) && (
                (a.FirstVertex.Equals(b.v1) && a.SecondVertex.Equals(b.v2)) ||
                (a.FirstVertex.Equals(b.v2) && a.SecondVertex.Equals(b.v1)));
    }
    
    public static bool operator != (Edge<V, E> a, (V v1, V v2, E e) b)
    {
        return !(a == b);
    }

    public static bool operator ==((V v1, V v2, E e) a, Edge<V, E> b)
    {
        return b == a;
    }
    
    public static bool operator !=((V v1, V v2, E e) a, Edge<V, E> b)
    {
        return !(a == b);
    }
    
    public override bool Equals(Object obj)
    {
        if (!(obj is Edge<V, E>))
            return false;

        Edge<V, E> edge = (Edge<V, E>) obj;

        return this == edge;
    }

    public override int GetHashCode()
    {
        return FirstVertex.GetHashCode() + SecondVertex.GetHashCode() + Weight.GetHashCode() + IsDirected.GetHashCode();
    }

    public override string ToString()
    {
        return $"({FirstVertex}, {SecondVertex}, {Weight})";
    }

    /// <summary>
    /// Serialization function
    /// </summary>
    public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
    {
        info.AddValue("FirstVertex", FirstVertex);
        info.AddValue("SecondVertex", SecondVertex);
        info.AddValue("Weight", Weight);
        info.AddValue("IsDirected", IsDirected);
    }
}