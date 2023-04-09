using Sugar.Parallel;

using var runner = new ConcurrentRunner(20);

for (int i = 0; i < 1000; i++)
{
    Console.WriteLine($"Main {i}");
    var local = i;
    await runner.Run(async () =>
    {
        Console.WriteLine($"  Sub {local}");
        await Task.Delay(1000);
        Console.WriteLine($"  Sub {local} done");
    });
}

await runner.WhenAll();

Console.WriteLine("bye");