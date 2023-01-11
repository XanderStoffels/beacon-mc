using Microsoft.IO;

namespace Beacon.Server.Utils;

internal static class MemoryStreaming
{
    private static RecyclableMemoryStreamManager? _manager;
    public static RecyclableMemoryStreamManager Manager => _manager ??= new();
}