using System.ComponentModel;

namespace Domain;

public enum ECardValue
{
    [Description("𝟎")] Zero,
    [Description("𝟏")] One,
    [Description("𝟐")] Two,
    [Description("𝟑")] Three,
    [Description("𝟒")] Four,
    [Description("𝟓")] Five,
    [Description("𝟔")] Six,
    [Description("𝟕")] Seven,
    [Description("𝟖")] Eight,
    [Description("𝟗")] Nine,
    [Description("🛇")] Skip,
    [Description("↶")] Reverse,
    [Description("⧉")] DrawTwo,
    [Description("𝐖")] Wild,
    [Description("D4")] WildDrawFour,
    CardBack,
    ChooseColor,
    Undefined
}







