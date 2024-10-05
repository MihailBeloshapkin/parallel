namespace Task2.Implementation.Lock;

/// <summary>
/// TASLock class from Common/LocksContinued. Lock using atomic operation.
/// </summary>
public class TASLock : ILock
{
    private volatile int state = 0;

    public void Lock()
    {
        while (Interlocked.CompareExchange(ref state, 1, 0) == 1) { }
    }

    public void Unlock()
    {
        state = 0;
    }
}