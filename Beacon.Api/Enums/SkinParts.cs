namespace Beacon.Api.Enums;

[Flags]
public enum SkinParts : byte
{
    Cape = 0x01,
    Jacket = 0x02,
    LeftSleeve = 0x04,
    RightSleeve = 0x08,
    LeftPantsLeg = 0x10,
    RightPantsLeg = 0x20,
    Hat = 0x40
}