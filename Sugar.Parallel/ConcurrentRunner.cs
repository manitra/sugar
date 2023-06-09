﻿using System;
using System.Collections;
using System.Collections.Concurrent;

namespace Sugar.Parallel;

public class ConcurrentRunner : IAsyncDisposable
{
    private readonly SemaphoreSlim sem;
    private readonly ConcurrentQueue<Task> tasks;

    public IReadOnlyCollection<Task> Tasks => tasks;

    public ConcurrentRunner(int maxParallellism = 1)
    {
        sem = new SemaphoreSlim(maxParallellism);
        tasks = new ConcurrentQueue<Task>();
    }

    public async Task Run(Func<Task> asyncAction)
    {
        await sem.WaitAsync();
        tasks.Enqueue(CallAsync(asyncAction, sem));
    }

    public async Task WhenAll()
    {
        await Task.WhenAll(tasks);
        tasks.Clear();
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

    public async ValueTask DisposeAsync()
    {
        await WhenAll();
        sem.Dispose();
    }
}