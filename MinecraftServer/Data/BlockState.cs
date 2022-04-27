namespace MinecraftServer.Data;

public readonly partial struct BlockState
{
    
    public readonly ushort Id;
    
    public BlockState(ushort id)
    {
        Id = id;
    }

}