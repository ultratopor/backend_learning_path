using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using ZeroAllocation_Hash_Map;
using BenchmarkDotNet.Running;

namespace Zero_Allocation_Hash_Map
{
    [MemoryDiagnoser]
    public class DictionaryBenchmarks
    {
        // --- Данные для тестов ---

        [Params(1000, 100_000)]
        public int ElementCount { get; set; }

        private KeyValuePair<int, string>[] _dataToAdd;
        private int[] _keysToFind;

        // Экземпляры словарей для тестирования
        private Dictionary<int, string> _standardDict;
        private LightweightDictionary<int, string> _lightweightDict;

        // --- Настройка перед каждым запуском ---
        /*
        [GlobalSetup(Targets = new[] { nameof(Add_Standard), nameof(Add_Lightweight) })]
        public void SetupForAdd()
        {
            _dataToAdd = new KeyValuePair<int, string>[ElementCount];
            for (int i = 0; i < ElementCount; i++)
            {
                _dataToAdd[i] = new KeyValuePair<int, string>(i, $"Value_{i}");
            }
        }
        */
        [GlobalSetup(Targets = new[] { nameof(TryGetValue_Standard), nameof(TryGetValue_Lightweight) })]
        public void SetupForTryGetValue()
        {
            // 1. Готовим данные для поиска
            _keysToFind = new int[ElementCount];
            for (int i = 0; i < ElementCount; i++)
            {
                _keysToFind[i] = i;
            }

            // 2. Заполняем словари тестовыми данными
            _standardDict = new Dictionary<int, string>(ElementCount);
            _lightweightDict = new LightweightDictionary<int, string>(ElementCount);

            for (int i = 0; i < ElementCount; i++)
            {
                _standardDict.Add(i, $"Value_{i}");
                _lightweightDict.Add(i, $"Value_{i}");
            }
        }

        // --- Бенчмарки ---
        /*
        [Benchmark(Baseline = true)]
        public void Add_Standard()
        {
            var dict = new Dictionary<int, string>(ElementCount);
            foreach (var item in _dataToAdd)
            {
                dict.Add(item.Key, item.Value);
            }
        }

        [Benchmark]
        public void Add_Lightweight()
        {
            var dict = new LightweightDictionary<int, string>(ElementCount);
            foreach (var item in _dataToAdd)
            {
                dict.Add(item.Key, item.Value);
            }
        }
        */
        [Benchmark(Baseline = true)]
        public void TryGetValue_Standard()
        {
            foreach (var key in _keysToFind)
            {
                _standardDict.TryGetValue(key, out _);
            }
        }

        [Benchmark]
        public void TryGetValue_Lightweight()
        {
            foreach (var key in _keysToFind)
            {
                _lightweightDict.TryGetValue(key, out _);
            }
        }
    }
}
