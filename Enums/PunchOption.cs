using Discord.Interactions;
using System.Runtime.Serialization;

namespace Kozma.net.Enums;

public enum PunchOption
{
    Brandish,
    [ChoiceDisplay("Overcharged Mixmaster")]
    [EnumMember(Value = "Overcharged Mixmaster")]
    Mixmaster,
    [ChoiceDisplay("Blast Bomb")]
    [EnumMember(Value = "Blast Bomb")]
    Bomb,
    [ChoiceDisplay("Swiftstrike Buckler")]
    [EnumMember(Value = "Swiftstrike Buckler")]
    Shield,
    [ChoiceDisplay("Black Kat Cowl")]
    [EnumMember(Value = "Black Kat Cowl")]
    Helmet
}
