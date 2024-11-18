using Discord.Interactions;
using System.Runtime.Serialization;

namespace Kozma.net.Src.Enums;

public enum PunchOption
{
    Brandish,
    [ChoiceDisplay("Overcharged Mixmaster")]
    Mixmaster,
    [ChoiceDisplay("Blast Bomb")]
    Bomb,
    [ChoiceDisplay("Swiftstrike Buckler")]
    Shield,
    [ChoiceDisplay("Black Kat Cowl")]
    Helmet
}
