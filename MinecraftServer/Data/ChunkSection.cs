namespace MinecraftServer.Data;

public class ChunkSection
{

    private ushort[] Blocks = new ushort[16 * 16 * 16];
    
    public BlockState this[(int x, int y, int z) sectionLocation] {
        get {
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
            return BlockState.States[
                Blocks[
                    (sectionLocation.x << 0) |
                    (sectionLocation.y << 4) |
                    (sectionLocation.z << 8)
                ]
            ];
        }
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
            var index = (sectionLocation.x << 0) |
                        (sectionLocation.y << 4) |
                        (sectionLocation.z << 8);
            if (Blocks[index] == value.Id) return;
            Blocks[index] = value.Id;
            _generatedPalette = false;

        }
    }

    private bool _generatedPalette;
    private ushort[] _palette;

    public ushort[] GetPalette()
    {
        if (_generatedPalette)
        {
            return _palette;
        }

        var palette = new List<ushort>();

        foreach (var block in Blocks)
        {
            if (!palette.Contains(block))
            {
                palette.Add(block);
            }
        }

        if (_palette.Length > 255)
        {
            Console.WriteLine("There are more than 255 unique states in this chunk section");
        }
        
        _palette = palette.ToArray();
        _generatedPalette = true;
        return _palette;
    }

}