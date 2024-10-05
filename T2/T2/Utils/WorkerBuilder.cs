using Task2.Implementation.Queue;

namespace Task2.Implementation.Utils;

/// <summary>
/// Utilities for building producers and consumers
/// </summary>
public static class WorkerBuilder
{
    public static Thread CreateProducer<T>(string name, IQueue<T> queue, Func<T> produceFunc, int debounceTime, CancellationToken token) {
        return new Thread(() =>
        {
            Console.WriteLine($"Producer {name} starts at thread {Environment.CurrentManagedThreadId}");
    
            while (!token.IsCancellationRequested)
            {
                T item = produceFunc.Invoke();
                queue.Enqueue(item);
                Console.WriteLine($"{name} produced item {item}");
                Thread.Sleep(debounceTime);
            }

            Console.WriteLine($"Producer {name} stopped");
        });
    }
    
    public static Thread CreateConsumer<T>(string name, IQueue<T> queue, Action<T> consumeFunc, int debounceTime, CancellationToken token) {
        return new Thread(() =>
        {
            Console.WriteLine($"Consumer {name} starts at thread {Environment.CurrentManagedThreadId}");
    
            while (!token.IsCancellationRequested)
            {
                if (queue.TryDequeue(out T item))
                {
                    consumeFunc(item);
                    Console.WriteLine($"{name} consumed item {item}");   
                }
                Thread.Sleep(debounceTime);
            }

            Console.WriteLine($"Consumer {name} stopped");
        });
    }
}