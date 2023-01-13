using Beacon.API.Worlds;
using Beacon.API.Worlds.Generators;

namespace Beacon.Server.Worlds.Generators;

public class FlatStoneWorldGenerator : IWorldGenerator
{
    private const int VoidLevel = -1;
    private const int BedrockLevel = 0;
    private const int StoneUpperLevel = 5;
    
    public BlockType GetBlock(int x, int y, int z)
    {
        return y switch
        {
            <= VoidLevel => BlockType.Void,
            BedrockLevel => BlockType.Bedrock,
            <= StoneUpperLevel => BlockType.Stone,
            _ => BlockType.Air
        };
    }
}