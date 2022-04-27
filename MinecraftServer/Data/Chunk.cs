namespace MinecraftServer.Data;

public class Chunk
{

    private readonly ChunkSection[] _sections;
    public int SectionsLength => _sections.Length;

    public readonly List<Tuple<int, int, int>> ChunkUpdates = new();

    public ChunkSection this[int sectionY] {
        get {
            if (sectionY < 0 || sectionY > _sections.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(sectionY), $"argument should be in range from 0 to {_sections.Length - 1}");
            }
            return _sections[sectionY];
        }
    }

    public BlockState this[(int x, int y, int z) location] {
        get => this[location.y / 16][(location.x, location.y % 16, location.z)];
        set {
            if (this[location].Id == value.Id)
            {
                return;
            }
            _sections[location.y / 16][(location.x, location.y % 16, location.z)] = value;
            var locTuple = location.ToTuple();
            if (!ChunkUpdates.Contains(locTuple))
            {
                ChunkUpdates.Add(locTuple);
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
    }
    
}