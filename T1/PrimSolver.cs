using MPI;
using Task1.Implementation.Common.Exceptions;
using Task1.Implementation.Primitives.AdjacencyListGraph;
using Task1.Implementation.Utils;

namespace Task1.Implementation;

/// <summary>
/// Сlass containing implementations of the Prim's algorithm.
/// </summary>
public static class PrimSolver
{
    /// <summary>
    /// Find Minimum Spanning Tree using the Prim's algorithm. Sequential implementation.
    /// </summary>
    /// <param name="graph">Adjacency List Graph</param>
    /// <param name="enableLogging">Whether to enable logging</param>
   /// <returns>Minimum Spanning Tree, represented by an Adjacency List Graph</returns>
    /// <exception cref="DirectedGraphException">Graph must be undirected</exception>
    /// <exception cref="DisconnectedGraphException">Graph must be connected</exception>
    public static AdjacencyListGraph<V, E> FindMst<V, E>(AdjacencyListGraph<V, E> graph, bool enableLogging = false)
        where V : IParsable<V>, IEquatable<V>
        where E : IParsable<E>, IEquatable<E>, IComparable<E>
    {
        if (graph.IsDirected)
            throw new DirectedGraphException("Graph must be undirected");

        AdjacencyListGraph<V, E> MST = new AdjacencyListGraph<V, E>();
        HashSet<V> fringeVertices = graph.GetVertices();

        V firstVertex = fringeVertices.GetEnumerator().Current;
        
        MST.AddVertex(firstVertex);
        fringeVertices.Remove(firstVertex);

        while (MST.VerticesCount != graph.VerticesCount)
        {
            if (enableLogging) 
                Console.WriteLine($"MST vertices: {MST.VerticesCount}; graph vertices: {graph.VerticesCount}");
            
            List<Edge<V, E>> cutEdges = graph.GetCutEdges(MST.GetVertices(), fringeVertices);

            if (cutEdges.Count == 0)
                throw new DisconnectedGraphException("Graph must be connected");

            Edge<V, E>? minEdge = null;
            foreach (var edge in cutEdges)
            {
                if (minEdge == null || edge.Weight.CompareTo(((Edge<V, E>)minEdge).Weight) < 0)
                    minEdge = edge;
            }
            
            MST.AddEdge((Edge<V, E>)minEdge);
            if (!fringeVertices.Remove(((Edge<V, E>)minEdge).FirstVertex)) 
                fringeVertices.Remove(((Edge<V, E>)minEdge).SecondVertex);
        }

        return MST;
    }

    /// <summary>
    /// Find Minimum Spanning Tree using the Prim's algorithm. Parallel implementation. <br/>
    /// Must be run in an <b>MPI</b> environment.
    /// </summary>
    /// <param name="graph">Adjacency List Graph</param>
    /// <param name="enableLogging">Whether to enable logging</param>
   /// <returns>Minimum Spanning Tree, represented by an Adjacency List Graph</returns>
    /// <exception cref="ArgumentNullException">Process with rank = 0 must pass valid graph</exception>
    /// <exception cref="DirectedGraphException">Graph must be undirected</exception>
    /// <exception cref="DisconnectedGraphException">Graph must be connected</exception>
    public static AdjacencyListGraph<V, E> FindMstParallel<V, E>(AdjacencyListGraph<V, E> graph, bool enableLogging = false)
        where V : IParsable<V>, IEquatable<V>
        where E : IParsable<E>, IEquatable<E>, IComparable<E>
    {
        Intracommunicator comm = Communicator.world;
        
        int rank = comm.Rank;
        int size = comm.Size;

        if (rank == 0 && graph == null)
            throw new ArgumentNullException(nameof(graph), "Process with rank = 0 must pass valid graph");

        AdjacencyListGraph<V, E> MST = null;
        HashSet<V> fringeVertices = null;
        bool iterCondition = false;

        if (rank == 0)
        {
            if (graph.IsDirected)
                throw new DirectedGraphException("Graph must be undirected");
            
            MST = new AdjacencyListGraph<V, E>();
            fringeVertices = graph.GetVertices();

            V firstVertex = fringeVertices.GetEnumerator().Current;
        
            MST.AddVertex(firstVertex);
            fringeVertices.Remove(firstVertex);

            iterCondition = MST.VerticesCount != graph.VerticesCount;
        }

        comm.Broadcast(ref iterCondition, 0);

        while (iterCondition)
        {
            List<Edge<V, E>>[] edgeDistribution = new List<Edge<V, E>>[size];
            Edge<V, E>? defaultMinEdge = null;

            if (rank == 0)
            {
                List<Edge<V, E>> cutEdges = graph.GetCutEdges(MST!.GetVertices(), fringeVertices!);

                if (cutEdges.Count == 0)
                    throw new DisconnectedGraphException("Graph must be connected");

                defaultMinEdge = cutEdges[0];
                var distr = MpiHelper.getDistribution(cutEdges.Count, size);
                
                for (int i = 0; i < size; ++i)
                {
                    edgeDistribution[i] = cutEdges.GetRange(distr.displs[i], distr.sizes[i]);
                }
            }

            comm.Broadcast(ref defaultMinEdge, 0);

            List<Edge<V, E>> procEdges = comm.Scatter(edgeDistribution, 0);

            Edge<V, E>? procMinEdge = null;
            
            foreach (var edge in procEdges)
            {
                if (procMinEdge == null || edge.Weight.CompareTo(((Edge<V, E>)procMinEdge).Weight) < 0)
                    procMinEdge = edge;
            }

            Edge<V, E>[] minEdges = comm.Gather(procMinEdge.GetValueOrDefault((Edge<V, E>)defaultMinEdge!), 0);

            if (rank == 0)
            {
                Edge<V, E>? minEdge = null;
                foreach (var edge in minEdges)
                {
                    if (minEdge == null || edge.Weight.CompareTo(((Edge<V, E>)minEdge).Weight) < 0)
                        minEdge = edge;
                }
                
                MST!.AddEdge((Edge<V, E>)minEdge!);
                if (!fringeVertices!.Remove(((Edge<V, E>)minEdge).FirstVertex)) 
                    fringeVertices.Remove(((Edge<V, E>)minEdge).SecondVertex);
                
                iterCondition = MST.VerticesCount != graph.VerticesCount;
                if (enableLogging) 
                    Console.WriteLine($"MST vertices: {MST.VerticesCount}; graph vertices: {graph.VerticesCount}");
            }

            comm.Broadcast(ref iterCondition, 0);
        }

        return rank == 0 ? MST : null;
    }

}
