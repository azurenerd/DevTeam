// Test file for verifying inline review comments
namespace AgentSquad.Tests;

public class InlineReviewTest
{
    public void Method1()
    {
        // This line should get an inline comment
        var x = 42;
        Console.WriteLine(x);
    }

    public void Method2()
    {
        // Another method to test multi-file comments
        var y = "hello";
        Console.WriteLine(y);
    }

    public void Method3()
    {
        // Third method
        var z = true;
        if (z) Console.WriteLine("done");
    }
}
