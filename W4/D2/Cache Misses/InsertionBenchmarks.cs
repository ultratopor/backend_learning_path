using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Collections.Generic;

[MemoryDiagnoser]
public class InsertionBenchmarks
{
    // Проверяем разные размеры, чтобы найти порог
    [Params(100, 1_000, 10_000, 100_000)]
    public int Size { get; set; }

    private List<int> _list;
    private LinkedList<int> _linkedList;

    // Ссылка на средний узел для "идеального" O(1) теста
    private LinkedListNode<int> _middleNode;

    [GlobalSetup]
    public void Setup()
    {
        _list = new List<int>(Size);
        _linkedList = new LinkedList<int>();
        for (int i = 0; i < Size; i++)
        {
            _list.Add(i);
            _linkedList.AddLast(i);
        }

        // Находим средний узел заранее для теста идеальной вставки O(1)
        _middleNode = _linkedList.First;
        for (int i = 0; i < Size / 2; i++)
        {
            _middleNode = _middleNode.Next;
        }
    }

    [Benchmark(Baseline = true)]
    public void ListInsert_Middle()
    {
        _list.Insert(_list.Count / 2, -1);
    }

    [Benchmark]
    public void LinkedListInsert_IdealO1()
    {
        // Используем заранее найденный узел. Это чистая O(1) вставка.
        _linkedList.AddAfter(_middleNode, -1);
    }

    [Benchmark]
    public void LinkedListInsert_RealisticOn()
    {
        // Сначала находим узел (O(n)), потом вставляем (O(1)). Итого O(n).
        var nodeToFind = _linkedList.First;
        for (int i = 0; i < Size / 2; i++)
        {
            nodeToFind = nodeToFind.Next;
        }
        _linkedList.AddAfter(nodeToFind, -1);
    }
}
