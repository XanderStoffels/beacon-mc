namespace Beacon;
public record ServerStatus(
    Version version,
    Players players,
    Description description,
    string favicon,
    bool enforcesSecureChat
);

public record Version(
    string name,
    int protocol
);

public record Players(
    int max,
    int online,
    Sample[] sample
);

public record Sample(
    string name,
    string id
);

public record Description(
    string text
);