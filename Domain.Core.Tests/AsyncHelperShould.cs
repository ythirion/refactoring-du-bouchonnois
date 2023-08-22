using FluentAssertions;

namespace Domain.Core.Tests;

public class AsyncHelperShould
{
    private static async Task<int> AsyncProcess() => await Task.FromResult(42);
    private static async Task AsyncProcessWithoutResult(Mutation m) => await Task.Run(() => m.Mutated = true);

    [Fact]
    public void run_a_task_synchronously()
    {
        AsyncHelper.RunSync(AsyncProcess)
            .Should()
            .Be(42);
    }

    private class Mutation
    {
        public bool Mutated { get; set; }
    }

    [Fact]
    public void run_a_task_without_result_synchronously()
    {
        var mutation = new Mutation();
        AsyncHelper.RunSync(() => AsyncProcessWithoutResult(mutation));
        mutation.Mutated.Should().BeTrue();
    }
}