using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using ZeroAllocation_Hash_Map;

namespace Zero_Allocation_Hash_Map
{
    [MemoryDiagnoser]
    public class DictionaryDegradationBenchmarks
    {
        [Params(1_000, 10_000)]
        public int ElementCount { get; set; }

        private BadHashKey[] _keysToAdd;
        private Dictionary<BadHashKey, string> _standardDict;
        private LightweightDictionary<BadHashKey, string> _lightweightDict;

        [GlobalSetup]
        public void Setup()
        {
            // Создаем ключи с одинаковым хэш-кодом
            _keysToAdd = new BadHashKey[ElementCount];
            for (int i = 0; i < ElementCount; i++)
            {
                _keysToAdd[i] = new BadHashKey { Id = i };
            }
        }

        [Benchmark(Baseline = true)]
        public void Add_Standard_BadHash()
        {
            var dict = new Dictionary<BadHashKey, string>(ElementCount);
            foreach (var key in _keysToAdd)
            {
                dict.Add(key, $"Value_{key.Id}");
            }
        }

        [Benchmark]
        public void Add_Lightweight_BadHash()
        {
            var dict = new LightweightDictionary<BadHashKey, string>(ElementCount);
            foreach (var key in _keysToAdd)
            {
                dict.Add(key, $"Value_{key.Id}");
            }
        }
    }

    // Ключ с ужасным хэш-кодом для стресс-теста
    public class BadHashKey
    {
        public int Id { get; set; }

        public override int GetHashCode()
        {
            // ВСЕ ключи возвращают один и тот же хэш!
            return 42;
        }
    }
}
