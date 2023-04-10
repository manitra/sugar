using Sugar.Parallel;

await using var runner = new ConcurrentRunner(4);

for (int i = 0; i < 100; i++)
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