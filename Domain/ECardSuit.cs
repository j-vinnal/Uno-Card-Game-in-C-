using System.ComponentModel;

namespace Domain;

public enum ECardSuit
{
    [Description("🟥")] Red,
    [Description("🟦")] Blue,
    [Description("🟨")] Yellow,
    [Description("🟩")] Green,
    [Description("🌟")] Wild,
    Undefined
}