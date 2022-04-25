namespace MinecraftServer.Data;

public class ChunkSection
{
    
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
            return BlockState.Air;
        }
    }
    
}