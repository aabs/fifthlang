using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;

namespace compiler.Validation.GuardValidation.Infrastructure;

/// <summary>
/// Minimal pooled list implementation backed by ArrayPool&lt;T&gt; to reduce short-lived allocations in hot paths.
/// Use guarded by environment variable `FIFTH_GUARD_VALIDATION_POOL=1` to enable pooling.
/// </summary>
internal sealed class PooledList<T> : IEnumerable<T>, IDisposable
{
    private T[] _array;
    private int _count;
    private bool _disposed;

    public PooledList(int initialCapacity = 8)
    {
        _array = ArrayPool<T>.Shared.Rent(Math.Max(initialCapacity, 4));
        _count = 0;
    }

    public int Count => _count;

    public void Add(T item)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(PooledList<T>));
        if (_count >= _array.Length)
        {
            var newSize = Math.Max(_array.Length * 2, _array.Length + 4);
            var newArr = ArrayPool<T>.Shared.Rent(newSize);
            Array.Copy(_array, newArr, _count);
            ArrayPool<T>.Shared.Return(_array, clearArray: true);
            _array = newArr;
        }
        _array[_count++] = item;
    }

    public T this[int index]
    {
        get
        {
            if (index < 0 || index >= _count) throw new IndexOutOfRangeException();
            return _array[index];
        }
    }

    public List<T> ToList()
    {
        var result = new List<T>(_count);
        for (int i = 0; i < _count; i++) result.Add(_array[i]);
        return result;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            ArrayPool<T>.Shared.Return(_array, clearArray: true);
            _array = Array.Empty<T>();
            _count = 0;
            _disposed = true;
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        for (int i = 0; i < _count; i++) yield return _array[i];
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
