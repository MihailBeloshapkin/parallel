using Task2.Implementation.Queue;
using Task2.Implementation.Utils;

namespace Task2.Example;

internal class Program
{
    private const int ProducersNumber = 4;
    private const int ConsumersNumber = 4;
    
    static void Main(string[] args)
    {
        TASConcurrentQueue<int> queue = new TASConcurrentQueue<int>();
        CancellationTokenSource cts = new CancellationTokenSource();
        Random random = new Random();
        
        List<Thread> producers = new List<Thread>(ProducersNumber);
        List<Thread> consumers = new List<Thread>(ConsumersNumber);

        for (int i = 0; i < ProducersNumber; i++)
        {
            producers.Add(WorkerBuilder.CreateProducer(
                $"p{i}", 
                queue, 
                () => random.Next(), 
                random.Next(300, 500), 
                cts.Token)
            );
        }
        
        for (int i = 0; i < ConsumersNumber; i++)
        {
            consumers.Add(WorkerBuilder.CreateConsumer(
                $"c{i}", 
                queue, 
                item => { Console.WriteLine($"   I'm consuming {item}..."); }, 
                random.Next(300, 500),
                cts.Token)
            );
        }

        Console.WriteLine($"Starting work with {ProducersNumber} producers and {ConsumersNumber} consumers. " + 
                          $"Press any key to stop.");
        
        producers.AsParallel().ForAll(worker => worker.Start());
        consumers.AsParallel().ForAll(worker => worker.Start());
        
        Console.ReadKey();
        
        cts.Cancel();
    }
}
