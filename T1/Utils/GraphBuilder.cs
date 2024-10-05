using System.Globalization;
using Task1.Implementation.Primitives.AdjacencyListGraph;

namespace Task1.Implementation.Utils;

/// <summary>
/// Graph construction utilities.
/// </summary>
public static class GraphBuilder
{
    /// <summary>
    /// Read <b>AdjacencyListGraph</b> from stream. <br/>
    /// The stream must be given as text of the following form: <br/>
    /// - First line - the number of vertices in the graph; <br/>
    /// - Next lines - description of edges in the form of triplets <c>{first_vertex second_vertex edge_weight}</c>
    /// </summary>
    /// <param name="stream">Stream representing the graph</param>
    /// <typeparam name="V">Vertex type</typeparam>
    /// <typeparam name="E">Edge type</typeparam>
    /// <returns><b>AdjacencyListGraph</b></returns>
    public static AdjacencyListGraph<V, E> ReadFromStream<V, E>(Stream stream)
        where V : IParsable<V>, IEquatable<V>
        where E : IParsable<E>, IEquatable<E>, IComparable<E>
    {
        StreamReader reader = new StreamReader(stream);

        AdjacencyListGraph<V, E> graph = new AdjacencyListGraph<V, E>();

        string line = reader.ReadLine(); // number of vertices
        string[] tokens;
        
        while ((line = reader.ReadLine()) != null)
        {
            tokens = line.Split(" ");
            V v1 = V.Parse(tokens[0], CultureInfo.InvariantCulture);
            V v2 = V.Parse(tokens[1], CultureInfo.InvariantCulture);
            E e = E.Parse(tokens[2], CultureInfo.InvariantCulture);
            
            graph.AddEdge(v1, v2, e, true);
        }

        return graph;
    }

    /// <summary>
    /// Read <b>AdjacencyListGraph</b> from file. <br/>
    /// The file must contain text of the following form: <br/>
    /// - First line - the number of vertices in the graph; <br/>
    /// - Next lines - description of edges in the form of triplets <c>{first_vertex second_vertex edge_weight}</c>
    /// </summary>
    /// <param name="filepath">Path to the file</param>
    /// <typeparam name="V">Vertex type</typeparam>
    /// <typeparam name="E">Edge type</typeparam>
    /// <returns><b>AdjacencyListGraph</b></returns>
    public static AdjacencyListGraph<V, E> ReadFromFile<V, E>(String filepath)
        where V : IParsable<V>, IEquatable<V>
        where E : IParsable<E>, IEquatable<E>, IComparable<E>
    {
        return ReadFromStream<V, E>(new FileStream(filepath, FileMode.Open));
    }
    
    /// <summary>
    /// Read <b>AdjacencyListGraph</b> from string. <br/>
    /// The string must contain lines of the following form: <br/>
    /// - First line - the number of vertices in the graph; <br/>
    /// - Next lines - description of edges in the form of triplets <c>{first_vertex second_vertex edge_weight}</c>
    /// </summary>
    /// <param name="graphString">String representing the graph</param>
    /// <typeparam name="V">Vertex type</typeparam>
    /// <typeparam name="E">Edge type</typeparam>
    /// <returns><b>AdjacencyListGraph</b></returns>
    public static AdjacencyListGraph<V, E> ReadFromString<V, E>(String graphString)
        where V : IParsable<V>, IEquatable<V>
        where E : IParsable<E>, IEquatable<E>, IComparable<E>
    {
        return ReadFromStream<V, E>(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(graphString)));
    }

    /// <summary>
    /// Generate random graph with the specified number of vertices and average number of edges per vertex.
    /// </summary>
    /// <param name="numOfVertices">Number of vertices</param>
    /// <param name="fill">Proportion of edges per vertex relative to the maximum possible. Can be in [0, 1].</param>
    /// <param name="seed">Шnitializer for generator</param>
    /// <returns>New integer graph</returns>
    public static AdjacencyListGraph<int, int> GenerateRandomInt(uint numOfVertices, double fill, int? seed = null)
    {
        if (fill < 0 || fill > 1) throw new ArgumentException("f must be in [0, 1]");
        if (numOfVertices == 0) return new AdjacencyListGraph<int, int>();

        int edgesPerVetrex = (int)(fill * (numOfVertices - 1));
        
        Dictionary<int, Dictionary<int, int>> storage = new Dictionary<int, Dictionary<int, int>>((int)numOfVertices);
        for (int i = 0; i < numOfVertices; ++i)
            storage[i] = new Dictionary<int, int>();
        AdjacencyListGraph<int, int> graph = new AdjacencyListGraph<int, int>(storage);

        Random rnd = (seed != null) ? new Random(seed.Value) : new Random();

        for (int i = 0; i < numOfVertices; ++i)
        {
            for (int p = 0; p < edgesPerVetrex; ++p)
            {
                int j = (i + rnd.Next((int)numOfVertices - 2) + 1) % (int)numOfVertices;
                int val = rnd.Next(-10, 10);
                if (i < j)
                    graph.AddEdge(i, j, val, true);
                else
                    graph.AddEdge(j, i, val, true);
            }
        }

        return graph;
    }
}