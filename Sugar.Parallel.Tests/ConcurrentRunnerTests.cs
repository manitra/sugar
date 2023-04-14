namespace Sugar.Parallel.Tests;

public class ConcurrentRunnerTests
{
    [Fact]
    public async Task ShouldRunTasksConcurrently()
    {
        // Arrange
        var runner = new ConcurrentRunner(3);
        var task1 = Task.Delay(1000);
        var task2 = Task.Delay(1000);
        var task3 = Task.Delay(1000);

        // Act
        await runner.Run(() => task1);
        await runner.Run(() => task2);
        await runner.Run(() => task3);
        await runner.WhenAll();

        // Assert
        Assert.True(task1.IsCompletedSuccessfully);
        Assert.True(task2.IsCompletedSuccessfully);
        Assert.True(task3.IsCompletedSuccessfully);
    }

    [Fact]
    public async Task ShouldLimitParallelism()
    {
        // Arrange
        var runner = new ConcurrentRunner(2);
        var task1 = Task.Delay(500);
        var task2 = Task.Delay(1000);
        var task3 = Task.Delay(1000);

        // Act
        await runner.Run(() => task1);
        await runner.Run(() => task2);
        await runner.Run(() => task3);

        // Assert
        Assert.True(task1.IsCompletedSuccessfully, "1 should be completed");
        Assert.False(task2.IsCompletedSuccessfully, "2 should not be completed");
        Assert.False(task3.IsCompletedSuccessfully, "3 should not be completed");

        // Act
        await runner.WhenAll();

        // Assert
        Assert.True(task1.IsCompletedSuccessfully);
        Assert.True(task2.IsCompletedSuccessfully);
        Assert.True(task3.IsCompletedSuccessfully);
    }

    [Fact]
    public async Task ShouldDisposeOfSemaphore()
    {
        // Arrange
        var runner = new ConcurrentRunner(2);

        // Act
        await runner.DisposeAsync();

        // Assert
        await Assert.ThrowsAsync<ObjectDisposedException>(() => runner.Run(() => Task.CompletedTask));
    }

    [Fact]
    public async Task ShouldDisposeOfSemaphoreEvenIfTasksThrowExceptions()
    {
        // Arrange
        var runner = new ConcurrentRunner(2);
        var task1 = Task.FromException(new Exception());
        var task2 = Task.Delay(1000);

        // Act
        await runner.Run(() => task1);
        await runner.Run(() => task2);
        await Assert.ThrowsAsync<Exception>(() => runner.WhenAll());

        // Assert
        await Assert.ThrowsAsync<ObjectDisposedException>(() => runner.Run(() => Task.CompletedTask));
    }

    [Fact]
    public async Task ShouldClearTasksAfterWhenAll()
    {
        // Arrange
        var runner = new ConcurrentRunner(2);
        var task1 = Task.Delay(1000);
        var task2 = Task.Delay(1000);

        // Act
        await runner.Run(() => task1);
        await runner.Run(() => task2);
        await runner.WhenAll();

        // Assert
        Assert.Empty(runner.Tasks);
    }

    [Fact]
    public async Task ShouldRunActionInOrder()
    {
        // Arrange
        var runner = new ConcurrentRunner(1);
        var executedTasks = new List<int>();

        // Act
        await runner.Run(async () =>
        {
            await Task.Delay(100);
            executedTasks.Add(1);
        });
        await runner.Run(async () =>
        {
            await Task.Delay(50);
            executedTasks.Add(2);
        });
        await runner.Run(async () =>
        {
            await Task.Delay(10);
            executedTasks.Add(3);
        });
        await runner.WhenAll();

        // Assert
        Assert.Equal(new List<int> { 1, 2, 3 }, executedTasks);
    }
}