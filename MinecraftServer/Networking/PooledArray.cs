using System.Buffers;

namespace MinecraftServer.Networking;

public readonly struct PooledArray : IDisposable
{
    public byte this[int key]
    {
        get => Data[key];
        set => Data.Array![key] = value;
    }

    private PooledArray(Action func, ArraySegment<byte> data)
    {
        _returnDelegate = func;
        Data = data;
    }

    private readonly Action _returnDelegate;
    public static PooledArray Allocate(int length)
    {
        if (length < 10)
        {
            return new PooledArray(()=>{}, new byte[length]);
        }
        var data = new ArraySegment<byte>(ArrayPool<byte>.Shared.Rent(length), 0, length);
        return new PooledArray(() =>
        {
            ArrayPool<byte>.Shared.Return(data.Array);
        }, data);
    }
    public ArraySegment<byte> Data { get; init; }

    public void Dispose()
    {
        _returnDelegate();
    }

    public class SingleThreadAllocator
    {
        private byte[] _array;
        private bool hasReturned = true;
        public SingleThreadAllocator(int maxSize)
        {
            _array = new byte[maxSize];
        }

        public PooledArray Allocate(int length)
        {
            if (!hasReturned)
            {
                throw new InvalidOperationException("Cannot allocate an array that has already been lent out!");
            }
            var data = new ArraySegment<byte>(_array, 0, length);
            return new PooledArray(() =>
            {
                hasReturned = true;
            }, data);
        }
    }
}