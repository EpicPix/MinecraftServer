using System.Runtime.CompilerServices;

namespace MinecraftServer.Data;

public class Chunk
{

    private readonly ChunkSection[] _sections;
    public int SectionsLength => _sections.Length;

    public readonly List<ushort> ChunkUpdates = new();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort GetChunkIndex(byte x, byte y, byte z)
    {
        return (ushort) ((x << 0) |
                         (y << 4) |
                         (z << 12));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (byte x, byte y, byte z) GetChunkIndex(ushort index)
    {
        return (
            (byte) (index & 0xf),
            (byte) ((index >> 4) & 0xff),
            (byte) ((index >> 12) & 0xf)
            );
    }

    public ChunkSection this[int sectionY] {
        get {
            if (sectionY < 0 || sectionY > _sections.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(sectionY), $"argument should be in range from 0 to {_sections.Length - 1}");
            }
            return _sections[sectionY];
        }
    }

    public BlockState this[(byte x, byte y, byte z) location] {
        get => this[location.y / 16][(location.x, (byte) (location.y % 16), location.z)];
        set {
            if (this[location].Id == value.Id)
            {
                return;
            }
            _sections[location.y / 16][(location.x, (byte) (location.y % 16), location.z)] = value;
            var loc = GetChunkIndex(location.x, (byte) (location.y % 16), location.z);
            if (!ChunkUpdates.Contains(loc))
            {
                ChunkUpdates.Add(loc);
            }
        }
    }

    public Chunk(int maxHeight)
    {
        if (maxHeight % 16 != 0)
        {
            throw new ArgumentException("maxHeight must be a multiple of 16");
        }
        _sections = new ChunkSection[maxHeight / 16];
        for (var i = 0; i < _sections.Length; i++)
        {
            _sections[i] = new ChunkSection();
        }
    }
    
}