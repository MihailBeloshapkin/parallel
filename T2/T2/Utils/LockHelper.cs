using Task2.Implementation.Lock;

namespace Task2.Implementation.Utils;

/// <summary>
/// Utilities for working with Lock
/// </summary>
public static class LockHelper
{
    public static T DoInLock<T>(ILock locker, Func<T> lambda)
    {
        locker.Lock();
        try
        {
            return lambda.Invoke();
        }
        finally
        {
            locker.Unlock();
        }
    }
    
    public static void DoInLock(ILock locker, Action lambda)
    {
        locker.Lock();
        try
        {
            lambda.Invoke();
        }
        finally
        {
            locker.Unlock();
        }
    }
}