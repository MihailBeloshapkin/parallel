using Task2.Implementation.Lock;
using Task2.Implementation.Utils;

namespace Task2.Implementation.Queue;

/// <summary>
/// Thread-safe queue synchronized using TASLock
/// </summary>
/// <typeparam name="T">Type of elements</typeparam>
public class TASConcurrentQueue<T> : IQueue<T>
{
    private ILock Lock { get; } = new TASLock();
    private List<T> Buffer { get; set; }

    public int Count => LockHelper.DoInLock(Lock, () => Buffer.Count);

    public int Capacity
    {
        get => LockHelper.DoInLock(Lock, () => Buffer.Capacity);
        set => LockHelper.DoInLock(Lock, () => Buffer.Capacity = value);
    }

    public TASConcurrentQueue()
    {
        Buffer = new List<T>();
    }
    
    public TASConcurrentQueue(int capacity)
    {
        Buffer = new List<T>(capacity);
    }

    /// <summary>
    /// Add item to the end of the queue
    /// </summary>
    public void Enqueue(T item)
    {
        LockHelper.DoInLock(Lock, () =>
        {
            Buffer.Add(item);
        });
    }

    /// <summary>
    /// Remove and return item at the beginning of the queue.
    /// If queue is empty, returns default value.
    /// </summary>
    public T? Dequeue()
    {
        return LockHelper.DoInLock(Lock, () =>
        {
            if (Buffer.Count == 0)
                return default;
            
            T item = Buffer[0];
            Buffer.RemoveAt(0);
            return item;
        });
    }

    /// <summary>
    /// Try to get and remove item at the beginning of the queue.
    /// </summary>
    /// <param name="item">If queue was not empty, contains the first element. Otherwise the default value.</param>
    /// <returns><b>true</b> if the element was successfully retrieved, <b>false</b> otherwise.</returns>
    public bool TryDequeue(out T item)
    {
        Lock.Lock();
        try
        {
            if (Buffer.Count == 0)
            {
                item = default;
                return false;   
            }

            item = Buffer[0];
            Buffer.RemoveAt(0);
            return true;
        }
        finally
        {
            Lock.Unlock();
        }
    }
}
