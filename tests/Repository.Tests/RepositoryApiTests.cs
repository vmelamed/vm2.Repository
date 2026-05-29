// SPDX-License-Identifier: MIT

namespace vm2.Repository.Tests;

public class RepositoryApiTests(ITestOutputHelper outputHelper) : TestBase(outputHelper)
{
    [Fact]
    public void Echo_returns_value_when_present()
    {
        var result = RepositoryApi.Echo("hi", "fallback");
        result.Should().Be("hi");
    }

    [Fact]
    public void Echo_returns_fallback_when_null()
    {
        var result = RepositoryApi.Echo(null, "fallback");
        result.Should().Be("fallback");
    }
}
