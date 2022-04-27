using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace MinecraftServer.Data;

public class ChunkSection
{

    private ushort[] _blocks = new ushort[16 * 16 * 16];

    public BlockState this[(byte x, byte y, byte z) sectionLocation] {
        get => BlockState.States[GetBlockId(sectionLocation.x, sectionLocation.y, sectionLocation.z)];
        set {
            if (sectionLocation.x is < 0 or > 15)
            {
                throw new ArgumentOutOfRangeException(nameof(sectionLocation), "x should be in range from 0 to 15");
            }
            if (sectionLocation.y is < 0 or > 15)
            {
                throw new ArgumentOutOfRangeException(nameof(sectionLocation), "y should be in range from 0 to 15");
            }
            if (sectionLocation.z is < 0 or > 15)
            {
                throw new ArgumentOutOfRangeException(nameof(sectionLocation), "z should be in range from 0 to 15");
            }
            var index = GetChunkSectionIndex(sectionLocation.x, sectionLocation.y, sectionLocation.z);
            if (_blocks[index] == value.Id) return;
            _blocks[index] = value.Id;
            _generatedPalette = false;

        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort GetChunkSectionIndex(byte x, byte y, byte z)
    {
        return (ushort) ((x << 0) |
                         (y << 4) |
                         (z << 8));
    }

    public ushort GetBlockId(byte x, byte y, byte z)
    {
        if (x is < 0 or > 15)
        {
            throw new ArgumentOutOfRangeException(nameof(x), "x should be in range from 0 to 15");
        }
        if (y is < 0 or > 15)
        {
            throw new ArgumentOutOfRangeException(nameof(y), "y should be in range from 0 to 15");
        }
        if (z is < 0 or > 15)
        {
            throw new ArgumentOutOfRangeException(nameof(z), "z should be in range from 0 to 15");
        }
        return _blocks[GetChunkSectionIndex(x, y, z)];
    }

    private bool _generatedPalette;
    private ushort[] _palette = null!;
    private ImmutableDictionary<ushort, byte> _paletteMapping = null!;

    public ushort[] GetPalette()
    {
        if (_generatedPalette)
        {
            return _palette;
        }

        var palette = new List<ushort>();
        var paletteMapping = new Dictionary<ushort, byte>();

        foreach (var block in _blocks)
        {
            if (!palette.Contains(block))
            {
                paletteMapping[block] = (byte) palette.Count;
                palette.Add(block);
            }
        }

        if (palette.Count > 255)
        {
            Console.WriteLine("There are more than 255 unique states in this chunk section");
        }

        if (palette.Count == 1 && palette[0] == 0)
        {
            palette.Clear();
        }
        
        _palette = palette.ToArray();
        _paletteMapping = paletteMapping.ToImmutableDictionary();
        _generatedPalette = true;
        return _palette;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte GetBlockIdToPalette(ushort blockId)
    {
        if (!_generatedPalette)
        {
            GetPalette();
        }
        return _paletteMapping[blockId];
    }

}