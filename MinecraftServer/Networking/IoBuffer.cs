using System.Buffers;

namespace MinecraftServer.Networking;

public readonly struct IoBuffer : IDisposable
{
    public static readonly IAllocator Default = new PooledArrayAllocator();
    public byte this[int key]
    {
        get => Data[key];
        set => Data.Array![key] = value;
    }

    private IoBuffer(IAllocator by, ArraySegment<byte> data)
    {
        Data = data;
        _allocatedBy = by;
    }

    private readonly IAllocator _allocatedBy;
    public ArraySegment<byte> Data { get; init; }

    public void Dispose()
    {
        _allocatedBy.Return(this);
    }

    public class SingleThreadAllocator : IAllocator
    {
        private byte[] _array;
        private bool hasReturned = true;
        public SingleThreadAllocator(int maxSize)
        {
            _array = new byte[maxSize];
        }

        public IoBuffer Allocate(int length)
        {
            if (!hasReturned)
            {
                throw new InvalidOperationException("Cannot allocate an array that has already been lent out!");
            }
            var data = new ArraySegment<byte>(_array, 0, length);
            return new IoBuffer(this, data);
        }
        public void Return(IoBuffer buffer)
        {
            hasReturned = true;
        }
    }

    public class PooledArrayAllocator : IAllocator
    {
        public IoBuffer Allocate(int length)
        {
            var data = new ArraySegment<byte>(ArrayPool<byte>.Shared.Rent(length), 0, length);
            return new IoBuffer(this, data);
        }

        public void Return(IoBuffer buffer)
        {
            ArrayPool<byte>.Shared.Return(buffer.Data.Array!);
        }
    }
    
    public interface IAllocator
    {
        public IoBuffer Allocate(int length);
        public void Return(IoBuffer buffer);
    }

    public static IoBuffer Allocate(int i)
    {
        return Default.Allocate(i);
    }
}