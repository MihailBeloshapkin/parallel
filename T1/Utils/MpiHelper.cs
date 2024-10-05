using MPI;

namespace Task1.Implementation.Utils;

/// <summary>
/// Utilities for working with MPI
/// </summary>
public static class MpiHelper
{
    /// <summary>
    /// Get a uniform distribution of the data collection for a given number of processes.
    /// </summary>
    /// <param name="numOfElements">Number of elements in collection</param>
    /// <param name="numOfProcesses">Number of processes</param>
    /// <returns><b>(sizes, displs)</b> - two arrays of size <b>numOfProcesses</b> containing the number
    /// of elements in a portion of data and their location relative to the general collection per each process.
    /// </returns>
    public static (int[] sizes, int[] displs) getDistribution(int numOfElements, int numOfProcesses)
    {
        int[] sizes = new int[numOfProcesses];
        int[] displs = new int[numOfProcesses];

        int quot = numOfElements / numOfProcesses;
        int rem = numOfElements % numOfProcesses;

        for (int i = 0; i < numOfProcesses; ++i)
        {
            sizes[i] = quot;
            if (i < rem) ++sizes[i];

            if (i == 0)
                displs[i] = 0;
            else
                displs[i] = displs[i - 1] + sizes[i - 1];
        }

        return (sizes, displs);
    }
    
    /// <summary>
    /// Execute passed function in the MPI environment.
    /// </summary>
    /// <param name="args">MPI.Environment args</param>
    /// <param name="lambda">Function</param>
    /// <typeparam name="T">Type of function result</typeparam>
    public static T RunWithMpi<T>(string[] args, Func<T> lambda)
    {
        using (new MPI.Environment(ref args))
        {
            return lambda.Invoke();
        }
    }
    
    /// <summary>
    /// Execute passed function in the MPI environment.
    /// </summary>
    /// <param name="args">MPI.Environment args</param>
    /// <param name="lambda">Action</param>
    public static void RunWithMpi(string[] args, Action lambda)
    {
        using (new MPI.Environment(ref args))
        {
            lambda();
        }
    }

    public static void Print(string str, int rank = 0)
    {
        if (MPI.Communicator.world.Rank == rank)
            Console.Write(str);
    }
    
    public static void PrintLine(string str = null, int rank = 0)
    {
        if (MPI.Communicator.world.Rank == rank)
            Console.WriteLine(str);
    }
    
    public static void PrintLine(object? value, int rank = 0)
    {
        if (MPI.Communicator.world.Rank == rank)
            Console.WriteLine(value);
    }

    /// <summary>
    /// Get current world communicator.
    /// </summary>
    public static Intracommunicator GetWorldCommunicator()
    {
        return MPI.Communicator.world;
    }
}