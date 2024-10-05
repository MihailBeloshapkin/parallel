namespace Task2.Implementation.Queue;

public interface IQueue<T>
{
    public int Count { get; }
    public void Enqueue(T item);
    public T? Dequeue();
    public bool TryDequeue(out T item);
}