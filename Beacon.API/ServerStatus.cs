namespace Beacon.API;

public class ServerStatus
{
    public required ProtocolVersion Version { get; set; }
    public required OnlinePlayers Players { get; set; }
    public required Motd Description { get; set; }
    public required string? Favicon { get; set; }
    public required bool PreviewsChat { get; set; }
    public required bool EnforcesSecureChat { get; set; }
    
    public class Motd
    {
        public required string Text { get; set; }
    }

    public class OnlinePlayers
    {
        public int Max { get; set; }
        public int Online { get; set; }
        public required List<PlayerSample> Sample { get; set; }
    }
    
    public class PlayerSample
    {
        public required string Name { get; set; }
        public required string Id { get; set; }
    }

    public class ProtocolVersion
    {
        public required string Name { get; set; }
        public required int Protocol { get; set; }
    }

}

