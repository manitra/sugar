using System;
using System.Collections.Concurrent;

namespace Sugar.Parallel;

public class ConcurrentRunner : IDisposable
{
    private readonly SemaphoreSlim sem;
    private readonly ConcurrentQueue<Task> tasks;

    public ConcurrentRunner(int maxParallellism = 1)
    {
        sem = new SemaphoreSlim(maxParallellism);
        tasks = new ConcurrentQueue<Task>();
    }

    public async Task Run(Func<Task> action)
    {
        await sem.WaitAsync();
        tasks.Enqueue(CallAsync(action, sem));
    }

    public async Task WhenAll()
    {
        await Task.WhenAll(tasks);
        tasks.Clear();
    }

    public void Dispose()
    {
        sem.Dispose();
    }

    private async Task CallAsync(Func<Task> action, SemaphoreSlim sem)
    {
        try
        {
            await action();
        }
        finally
        {
            sem.Release();
        }
    }
}