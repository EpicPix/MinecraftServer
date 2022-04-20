using System.Buffers;

namespace MinecraftServer;

public readonly struct PooledArray : IDisposable
{
    public byte this[int key]
    {
        get => Data[key];
        set => Data.Array![key] = value;
    }
    public static PooledArray Allocate(int length)
    {
        return new PooledArray()
        {
            Data = new ArraySegment<byte>(ArrayPool<byte>.Shared.Rent(length), 0, length)
        };
    }
    public ArraySegment<byte> Data { get; init; }

    public void Dispose()
    {
        ArrayPool<byte>.Shared.Return(Data.Array);
    }
}