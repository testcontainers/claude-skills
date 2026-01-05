// Test to see what Wait methods are available in 4.10.0
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;

public class WaitTest {
    public void Test() {
        // This won't compile but will show us available methods
        var wait = Wait.ForUnixContainer();
    }
}
