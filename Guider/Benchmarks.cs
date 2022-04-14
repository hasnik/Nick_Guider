//See https://aka.ms/new-console-template for more information

using BenchmarkDotNet.Attributes;
using Guider;

[MemoryDiagnoser(displayGenColumns: false)]
public class Benchmarks
{
    private readonly Guid TestIdAsGuid = Guid.Parse("2e38d121-e607-4f41-81d7-baf92b847e93");
    private readonly string TestIdAsString = "IdE4LgfmQU_B17r5K4R_kw";

    [Benchmark]
    public string ToStringFromGuid_Base()
    {
        return GuiderBase.ToStringFromGuid(TestIdAsGuid);
    }

    [Benchmark]
    public string ToStringFromGuid_Nick()
    {
        return GuiderNick.ToStringFromGuid(TestIdAsGuid);
    }

    [Benchmark]
    public string ToStringFromGuid_Mine1()
    {
        return GuiderMine_1.ToStringFromGuid(TestIdAsGuid);
    }

    [Benchmark]
    public string ToStringFromGuid_Mine2()
    {
        return GuiderMine_2.ToStringFromGuid(TestIdAsGuid);
    }

    [Benchmark]
    public string ToStringFromGuid_Mine3()
    {
        return GuiderMine_3.ToStringFromGuid(TestIdAsGuid);
    }

    [Benchmark]
    public string ToStringFromGuid_Mine4()
    {
        return GuiderMine_4.ToStringFromGuid(TestIdAsGuid);
    }

    [Benchmark]
    public string ToStringFromGuid_Nick4()
    {
        return GuiderMine_4.ToStringFromGuid2(TestIdAsGuid);
    }

    [Benchmark]
    public Guid ToGuidFromString_Base()
    {
        return GuiderBase.ToGuidFromString(TestIdAsString);
    }

    [Benchmark]
    public Guid ToGuidFromString_Nick()
    {
        return GuiderNick.ToGuidFromString(TestIdAsString);
    }

    [Benchmark]
    public Guid ToGuidFromString_Mine1()
    {
        return GuiderMine_1.ToGuidFromString(TestIdAsString);
    }

    [Benchmark]
    public Guid ToGuidFromString_Mine2()
    {
        return GuiderMine_2.ToGuidFromString(TestIdAsString);
    }

    [Benchmark]
    public Guid ToGuidFromString_Mine3()
    {
        return GuiderMine_3.ToGuidFromString(TestIdAsString);
    }

    [Benchmark]
    public Guid ToGuidFromString_Mine4()
    {
        return GuiderMine_4.ToGuidFromString(TestIdAsString);
    }
}