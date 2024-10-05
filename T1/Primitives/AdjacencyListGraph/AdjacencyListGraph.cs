using System.Text;
using Task1.Implementation.Common.Exceptions;

namespace Task1.Implementation.Primitives.AdjacencyListGraph;

/// <summary>
/// Adjacency List Graph. Represents simple graph. Can be directed or undirected.
/// Edges are stored in a Dictionary for performance reasons.
/// </summary>
/// <typeparam name="V">Vertex type</typeparam>
/// <typeparam name="E">Edge type</typeparam>
public class AdjacencyListGraph<V, E> : IGraph
    where V : IParsable<V>, IEquatable<V>
    where E : IParsable<E>, IEquatable<E>, IComparable<E>
{
    public bool IsDirected { get; }
    public int VerticesCount { get => Storage.Count; }
    public int EdgesCount { get; private set; } = 0;
    
    public Dictionary<V, Dictionary<V, E>> Storage { get; }

    public AdjacencyListGraph(bool isDirected = false)
    {
        IsDirected = isDirected;
        Storage = new Dictionary<V, Dictionary<V, E>>();
    }

    public AdjacencyListGraph(Dictionary<V, Dictionary<V, E>> storage, bool isDirected = false)
    {
        IsDirected = isDirected;
        Storage = storage;
    }

    /// <summary>
    /// Determine whether a given vertex exists in the graph. <br/>
    /// Performance: O(1) 
    /// </summary>
    public bool HasVertex(V vertex)
    {
        return Storage.ContainsKey(vertex);
    }

    /// <summary>
    /// Determine whether a given edge exists in the graph. <br/>
    /// Performance: O(1)
    /// </summary>
    /// <param name="v1">First vertex of edge</param>
    /// <param name="v2">Second vertex of edge</param>
    public bool HasEdge(V v1, V v2)
    {
        return IsDirected 
            ? Storage.ContainsKey(v1) && Storage[v1].ContainsKey(v2)
            : (Storage.ContainsKey(v1) && Storage[v1].ContainsKey(v2)) ||
              (Storage.ContainsKey(v2) && Storage[v2].ContainsKey(v1));
    }

    /// <summary>
    /// Retrieve edge's weight by its vertices. <br/>
    /// Performance: O(1)
    /// </summary>
    /// <param name="v1">First vertex of edge</param>
    /// <param name="v2">Second vertex of edge</param>
    /// <exception cref="HasNoEdgeException"></exception>
    public Edge<V, E> GetEdge(V v1, V v2)
    {
        Dictionary<V, E> edges;
        E edge;
        
        if (Storage.TryGetValue(v1, out edges) && edges.TryGetValue(v2, out edge))
            return new Edge<V, E>(v1, v2, edge, IsDirected);

        if (IsDirected) throw new HasNoEdgeException();

        if (Storage.TryGetValue(v2, out edges) && edges.TryGetValue(v1, out edge))
            return new Edge<V, E>(v2, v1, edge, IsDirected);

        throw new HasNoEdgeException();
    }

    /// <summary>
    /// Add a new vertex to the graph. <br/>
    /// Performance: ~O(1)
    /// </summary>
    public void AddVertex(V vertex)
    {
        Storage.Add(vertex, new Dictionary<V, E>());
    }

    /// <summary>
    /// Add a new edge to the graph. <br/>
    /// Performance: ~O(1)
    /// </summary>
    /// <param name="v1">First vertex of edge</param>
    /// <param name="v2">Second vertex of edge</param>
    /// <param name="e">Weight of edge</param>
    /// <param name="replace">Whether to replace the edge if it already exists. Default is <b>false</b>.</param>
    /// <exception cref="DuplicateEdgeException"></exception>
    public void AddEdge(V v1, V v2, E e, bool replace = false)
    {
        if (!Storage.ContainsKey(v1)) AddVertex(v1);
        if (!Storage.ContainsKey(v2)) AddVertex(v2);
        
        Dictionary<V, E> edges;

        if (Storage.TryGetValue(v1, out edges) && edges.TryGetValue(v2, out _))
            if (replace)
            {
                edges[v2] = e;
                return;
            }
            else throw new DuplicateEdgeException();

        if (IsDirected)
        {
            Storage[v1].Add(v2, e);
            return;
        }

        if (Storage.TryGetValue(v2, out edges) && edges.TryGetValue(v1, out _))
            if (replace)
            {
                edges[v1] = e;
                return;
            }
            else throw new DuplicateEdgeException();
            
        Storage[v1].Add(v2, e);
        EdgesCount++;
    }
    
    /// <summary>
    /// Add a new edge to the graph. If the graph is undirected, direction of the edge is ignored. <br/>
    /// Performance: ~O(1)
    /// </summary>
    /// <param name="edge">Edge to add</param>
    /// <param name="replace">Whether to replace the edge if it already exists. Default is <b>false</b>.</param>
    public void AddEdge(Edge<V, E> edge, bool replace = false)
    {
        AddEdge(edge.FirstVertex, edge.SecondVertex, edge.Weight);
    }

    /// <summary>
    /// Remove specified edge from the graph. <br/>
    /// Performance: O(1)
    /// </summary>
    /// <param name="v1">First vertex of edge</param>
    /// <param name="v2">Second vertex of edge</param>
    /// <returns><b>True</b> if the edge was successfully removed. <b>False</b> otherwise.</returns>
    public bool RemoveEdge(V v1, V v2)
    {
        Dictionary<V, E> edges;

        bool HandleRemove(bool removed)
        {
            if (removed) EdgesCount--;
            return removed;
        }

        if (Storage.TryGetValue(v1, out edges) && edges.TryGetValue(v2, out _))
            return HandleRemove(edges.Remove(v2));

        if (IsDirected) return false;

        if (Storage.TryGetValue(v2, out edges) && edges.TryGetValue(v1, out _))
            return HandleRemove(edges.Remove(v1));

        return false;
    }

    /// <summary>
    /// Remove specified vertex from the graph. <br/>
    /// Performance: O(V)
    /// </summary>
    /// <returns><b>True</b> if the vertex was successfully removed. <b>False</b> otherwise.</returns>
    public bool RemoveVertex(V vertex)
    {
        if (!HasVertex(vertex)) return false;

        foreach (var v in Storage)
        {
            RemoveEdge(v.Key, vertex);
        }

        return Storage.Remove(vertex);
    }

    /// <summary>
    /// Get graph vertices
    /// </summary>
    public HashSet<V> GetVertices()
    {
        return Storage.Keys.ToHashSet();
    }

    /// <summary>
    /// Get graph edges
    /// </summary>
    /// <returns></returns>
    public List<Edge<V, E>> GetEdges()
    {
        return Storage.SelectMany(
            verticesDict => verticesDict.Value,
            (verticesDict, edgesDict) => 
                new Edge<V, E>(verticesDict.Key, edgesDict.Key, edgesDict.Value, IsDirected)
            ).ToList();
    }

    /// <summary>
    /// Get incident edges for specified vertex and subset of vertices. <br/>
    /// Performance: O(V), where V - number of vertices in subset.
    /// </summary>
    /// <param name="vertex">Vertex for which incident edges are sought</param>
    /// <param name="vertexSubset">A subset of vertices among which the search is performed.
    /// If <b>null</b> (default), searched among all graph vertices. </param>
    /// <returns>List of incident edges</returns>
    public List<Edge<V, E>> GetIncidentEdges(V vertex, ICollection<V>? vertexSubset = null)
    {
        ICollection<V> subset = vertexSubset ?? Storage.Keys;
        List<Edge<V, E>> incidentEdges = new List<Edge<V, E>>(subset.Count);
        
        foreach (V v in subset)
        {
            Dictionary<V, E> edges;
            E edge;
        
            if (Storage.TryGetValue(vertex, out edges) && edges.TryGetValue(v, out edge))
                incidentEdges.Add(new Edge<V, E>(vertex, v, edge, IsDirected));

            if (Storage.TryGetValue(v, out edges) && edges.TryGetValue(vertex, out edge))
                incidentEdges.Add(new Edge<V, E>(v, vertex, edge, IsDirected));
        }
        
        incidentEdges.TrimExcess();

        return incidentEdges;
    }

    /// <summary>
    /// Get edges in a given graph cut. Vertex subset must be disjoint. <br/>
    /// Performance: O(V1 * V2), where V1 and V2 - number of vertices in subsets.
    /// </summary>
    /// <param name="vertexSubset1">First subset of vertices</param>
    /// <param name="vertexSubset2">Second subset of vertices</param>
    /// <returns>List of edges in cut</returns>
    public List<Edge<V, E>> GetCutEdges(ICollection<V> vertexSubset1, ICollection<V> vertexSubset2)
    {
        List<Edge<V, E>> cutEdges = new List<Edge<V, E>>(EdgesCount);

        foreach (var vertex in vertexSubset1)
        {
            cutEdges.AddRange(GetIncidentEdges(vertex, vertexSubset2));
        }
        
        cutEdges.TrimExcess();

        return cutEdges;
    }

    /// <summary>
    /// Get deep copy of graph.
    /// </summary>
    public AdjacencyListGraph<V, E> Copy()
    {
        Dictionary<V, Dictionary<V, E>> newStorage = new Dictionary<V, Dictionary<V, E>>(VerticesCount);
        AdjacencyListGraph<V, E> newGraph = new AdjacencyListGraph<V, E>(newStorage);
        newGraph.EdgesCount = EdgesCount;
        
        foreach (var edges in Storage)
        {
            newStorage.Add(edges.Key, new Dictionary<V, E>(edges.Value));
        }

        return newGraph;
    }

    /// <summary>
    /// Get a string representing the graph's storage in readable form.
    /// </summary>
    public string StorageToString()
    {
        StringBuilder storageString = new StringBuilder();
        foreach (var v in Storage)
        {
            storageString.Append($"{v.Key}: ");
            foreach (var edge in v.Value)
            {
                storageString.Append($"({edge.Key},{edge.Value}) ");
            }
            storageString.AppendLine();
        }

        return storageString.ToString();
    }
    
    public override string ToString()
    {
        return $"AdjacencyListGraph (Vertices: {VerticesCount}, Edges: {EdgesCount})\nStorage:\n" + StorageToString();
    }

    /// <summary>
    /// Write graph to stream in the following form: <br/>
    /// - First line - the number of vertices in the graph; <br/>
    /// - Next lines - description of edges in the form of triplets <c>{first_vertex second_vertex edge_weight}</c>
    /// </summary>
    /// <param name="stream">Input stream</param>
    public void WriteToStream(Stream stream)
    {
        StreamWriter writer = new StreamWriter(stream);
        
        writer.WriteLine(VerticesCount);
        foreach (var edgeDict in Storage)
        {
            if (edgeDict.Value.Count == 0) continue;
            foreach (var edge in edgeDict.Value)
            {
                writer.WriteLine($"{edgeDict.Key} {edge.Key} {edge.Value}");
            }
        }
        writer.Flush();
        writer.Close();
    }

    /// <summary>
    /// Write graph to file in the following form: <br/>
    /// - First line - the number of vertices in the graph; <br/>
    /// - Next lines - description of edges in the form of triplets <c>{first_vertex second_vertex edge_weight}</c>
    /// </summary>
    /// <param name="filepath">Path to file</param>
    public void WriteToFile(String filepath)
    {
        WriteToStream(new FileStream(filepath, FileMode.OpenOrCreate));
    }
}
