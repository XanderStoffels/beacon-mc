namespace Beacon.API.Models;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
public class ServerStatus
{
    public ServerVersionModel Version { get; set; }
    public OnlinePlayersModel Players { get; set; }
    public DescriptionModel Description { get; set; }
    public string Favicon { get; set; }
}

public class ServerVersionModel
{
    public string Name { get; set; }
    public int Protocol { get; set; }
}

public class OnlinePlayersModel
{
    public int Max { get; set; }
    public int Online { get; set; }
    public OnlinePlayerModel[] Sample { get; set; }
}

public class OnlinePlayerModel
{
    public string Name { get; set; }
    public string Id { get; set; }
}

public class DescriptionModel
{
    public string Text { get; set; }
}

