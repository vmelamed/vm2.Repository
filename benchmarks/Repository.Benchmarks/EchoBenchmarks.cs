// SPDX-License-Identifier: MIT

namespace vm2.Repository.Benchmarks;

#if SHORT_RUN
[ShortRunJob]
#else
[SimpleJob(RuntimeMoniker.HostProcess)]
#endif
public class EchoBenchmarks
{
    private string _value = "payload";

    [Benchmark]
    public string Echo_Value() => RepositoryApi.Echo(_value, "fallback");

    [Benchmark]
    public string Echo_Fallback() => RepositoryApi.Echo(null, "fallback");
}
