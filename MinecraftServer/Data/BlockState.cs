namespace MinecraftServer.Data;

public class BlockState
{
    // TODO Use a code generator and an array for all possible IDs, and maybe a big switch case for all possible String Id -> Numeric Id cases

    internal BlockState()
    {
        
    }
    
    public static readonly BlockState Air = new();

}