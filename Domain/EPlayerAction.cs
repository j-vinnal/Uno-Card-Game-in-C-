using System.ComponentModel;

namespace Domain;

public enum EPlayerAction
{
    [Description("Play a card")] 
    PlayCard,
    [Description("End your turn")] 
    EndTurn,
    [Description("Draw a card")] 
    DrawCard,
    [Description("Call Uno")] 
    CallUno,
    [Description("Save game")] 
    SaveGame,
    [Description("Skip your turn and draw four cards")] 
    SkipTurnWd4,
    [Description("Challenge the use of the Wild Draw Four")] 
    ChallengeWd4,
    [Description("Choose a new color")] 
    ChooseNewColor,
    Undefined
}