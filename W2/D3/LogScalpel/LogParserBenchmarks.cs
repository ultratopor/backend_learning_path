using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using LogScalpel;
using System;
using System.Linq;

[MemoryDiagnoser]
public class LogParserBenchmarks
{
    private const string LogLine = "[INFO] 2025-12-02 User 'Admin' logged in from IP 192.168.1.1";

    private readonly LegacyLogParser _legacy = new();
    private readonly SpanLogParser _span = new();

    [Benchmark(Baseline = true)]
    public LogEntry Parse_Legacy()
    {
        return _legacy.Parse(LogLine);
    }

    [Benchmark]
    public LogEntryV2 Parse_Span()
    {
        _span.Parse(LogLine, out var result);
        return result;
    }
}
