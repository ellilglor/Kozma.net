using Discord.Interactions;
using Kozma.net.Factories;
using Kozma.net.Helpers;
using Kozma.net.Services;

namespace Kozma.net.Commands.Information;

public class FindLogs(IEmbedFactory embedFactory, ITradeLogService tradeLogService, IContentHelper contentHelper) : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ItemData _data = new();

    // TODO? change choice options to bool
    [SlashCommand("findlogs", "Search the tradelog database for any item.")]
    public async Task ExecuteAsync(
        [Summary(description: "Item the bot should look for."), MinLength(3), MaxLength(69)] string item,
        [Summary(description: "How far back the bot should search. Default: 6 months."), MinValue(1), MaxValue(120)] int months = 6,
        [Summary(description: "Check for color variants / item family tree. Default: yes."), Choice("Yes", "variant-search"), Choice("No", "single-search")] string? variants = null,
        [Summary(description: "Filter out high value uvs. Default: no."), Choice("Yes", "clean-search"), Choice("No", "dirty-search")] string? clean = null,
        [Summary(description: "Check the mixed-trades channel. Default: yes."), Choice("Yes", "mixed-search"), Choice("No", "mixed-ignore")] string? mixed = null)
    {
        var checkVariants = string.IsNullOrEmpty(variants) || variants == "variant-search";
        var checkClean = !string.IsNullOrEmpty(clean) && clean == "clean-search";
        var checkMixed = !string.IsNullOrEmpty(mixed) && mixed == "mixed-search";

        var embed = embedFactory.GetEmbed($"Searching for __{item}__, I will dm you what I can find.")
            .WithDescription("### Info & tips when searching:\n- **Slime boxes**:\ncombination followed by *slime lockbox*\nExample: QQQ Slime Lockbox\n" +
                "- **UV's**:\nuse asi / ctr + med / high / very high / max\n" +
                "The bot automatically swaps asi & ctr so you don't have to search twice.\nExample: Brandish ctr very high asi high\n" +
                "- **Equipment**:\nThe bot looks for the entire family tree of your item!\n" +
                "So when you lookup *brandish* it will also match on *Combuster* & *Acheron*\n" +
                "- **Color Themes**:\ncertain colors with (expected) similar value are grouped for more results." +
                " Some examples include *Divine* & *Volcanic*, tech colors, standard colors, etc.\n" +
                "- **Sprite pods**:\ntype out as seen in game\nExample: Drakon Pod (Divine)")
            .Build();

        await ModifyOriginalResponseAsync(msg => msg.Embed = embed);
        await SearchLogsAsync(item, months, checkVariants, checkClean, checkMixed);
    }

    private async Task SearchLogsAsync(string item, int months, bool checkVariants, bool checkClean, bool checkMixed)
    {
        var copy = item;
        var items = new List<string>() { contentHelper.FilterContent(item) };
        var reverse = new List<string>();
        var stopHere = DateTime.Now.AddMonths(-months);

        AttachUvsToBack(items);
        if (checkVariants) AddVariants(items);
    }

    private void AttachUvsToBack(List<string> items)
    {
        var input = items[0].Split(" ");
        for (var i = 0; i < input.Length; i++)
        {
            foreach (var type in _data.UVTerms.Types)
            {
                if (string.Equals(input[i], type, StringComparison.OrdinalIgnoreCase))
                {
                    foreach (var grade in _data.UVTerms.Grades)
                    {
                        if (i + 1 < input.Length && string.Equals(input[i + 1], grade, StringComparison.OrdinalIgnoreCase))
                        {
                            var uv = string.Equals(grade, "very") && (i + 2 < input.Length && string.Equals(input[i + 2], "high", StringComparison.OrdinalIgnoreCase)) ? type + " very high" : type + " " + grade;
                            items[0] = (items[0].Replace(uv, string.Empty) + " " + uv).Replace("  ", " ").Trim();
                        }
                    }
                }
            }
        }
    }

    private void AddVariants(List<string> items)
    {
        var item = items[0];
        var exceptions = new List<string> { "drakon", "maskeraith", "nog" };
        if (exceptions.Any(keyword => item.Contains(keyword))) return;

        var family = _data.equipmentFamilies.FirstOrDefault(f => f.Value.Any(name => item.Contains(name)));

        if (!family.Equals(default(KeyValuePair<string, List<string>>)))
        {
            var match = family.Value.First(name => item.Contains(name));
            var uvs = item.Replace(match, string.Empty).Trim();

            items.Clear();
            family.Value.ForEach(entry => items.Add($"{entry} {uvs}".Trim()));

            return;
        }

        foreach (var set in _data.colorSets)
        {
            foreach (var color in set.Value)
            {
                if (!item.Contains(color)) continue;
                if (string.Equals(set.Key, "gems") && _data.GemExceptions.Any(exception => item.Contains(exception))) break;
                if (string.Equals(set.Key, "snipes") && (item.Contains("slime") || item.Contains("plume"))) break;

                var template = item.Replace(color, string.Empty).Trim();
                if (string.Equals(color, "rose") && ((template.Contains("tabard") || template.Contains("chapeau")) || _data.Roses.Any(rose => string.Equals(template, rose)))) break;

                items.Clear();
                if (set.Key.Contains("obsidian") || set.Key.Contains("rose"))
                {
                    set.Value.ForEach(value => items.Add($"{template} {value}".Trim()));
                } else
                {
                    set.Value.ForEach(value => items.Add($"{value} {template}".Trim()));
                }

                return;
            }
        }
    }

    private class ItemData
    {
        public readonly List<string> CleanFilter =
        [
            "ctr high", "ctr very high", "asi high", "asi very high", "normal high", "normal max",
            "shadow high", "shadow max", "fire high", "fire max", "shock high", "shock max"
        ];

        public readonly List<string> CommonFeatured =
        [
            "gloweyes", "scissor", "prime bombhead", "medieval war", "mixmaster", "spiral soaker", "tricorne", "shogun",
            "tabard of the coral", "chapeau of the coral", "tabard of the violet", "chapeau of the violet",
            "moonlight leafy", "dead leafy", "gatecrasher helm", "snarblepup", "gun pup", "love puppy", "restored",
            "sniped stranger", "iron shackles", "tails tails", "metal sonic"
        ];

        public readonly List<string> Spreadsheet =
        [
            "brand", "glacius", "combuster", "voltedge", "flourish", "snarble barb", "thorn blade",
            "sealed sword", "avenger", "faust", "black kat", "autogun", "needle", "chaingun", "grim reapater",
            "alchemer", "driver", "magnus", "tundrus", "winter grave", "iron slug", "callahan",
            "blaster", "riftlocker", "phantamos", "arcana", "valiance", "pulsar", "wildfire", "permafroster", "supernova", "polaris",
            "antigua", "raptor", "silversix", "blackhawk", "gilded griffin", "obsidian carbine", "sentenza", "argent peacemaker",
            "blast bomb", "electron", "graviton", "spine cone", "spike shower", "dark briar barrage", "nitronome",
            "vaporizer", "haze", "capacitor", "smogger", "atomizer", "slumber squall",
            "stagger storm", "voltaic tempest", "venom veiler", "torpor tantrum", "shivermist buster", "ash of agni"
        ];

        public readonly List<string> Roses =
        [
            "black", "red", "white", "blue", "gold", "green", "coral", "violet", "moonstone", "malachite",
            "garnet", "amethyst", "citrine", "prismatic", "aquamarine", "turquoise"
        ];

        public readonly List<string> GemExceptions =
        [
            "bout", "rose", "tabard", "chaeau", "buckled", "clover", "pipe", "lumberfell"
        ];

        public readonly Dictionary<string, List<string>> equipmentFamilies = new()
        {
            { "brandishes", new List<string>
                {
                    "shockburst brandish", "iceburst brandish", "fireburst brandish", "boltbrand", "silent nightblade",
                    "blizzbrand", "blazebrand", "advanced cautery sword", "obsidian edge", "voltedge", "glacius", "combuster",
                    "amputator", "acheron", "brandish", "nightblade", "cautery sword"
                }
            },
            { "flourishes", new List<string>
                {
                    "twisted snarble barb", "swift flourish", "dark thorn blade", "grand flourish",
                    "fierce flamberge", "daring rigadoon", "barbarous thorn blade", "furious flamberge", "final flourish",
                    "fearless rigadoon", "flourish", "snarble barb", "rigadoon", "flamberge"
                }
            },
            { "troikas", new List<string>
                {
                    "troika", "grintovec", "kamarin", "jalovec", "khorovod", "triglav", "sudaruska"
                }
            },
            { "spurs", new List<string>
                {
                    "spur", "arc razor", "winmillion", "turbillion"
                }
            },
            { "cutters", new List<string>
                {
                    "cutter", "vile striker", "dread venom striker", "wild hunting blade", "striker", "hunting blade"
                }
            },
            { "caliburs", new List<string>
                {
                    "tempered calibur", "cold iron carver", "ascended calibur", "leviathan blade", "cold iron vanquisher",
                    "calibur"
                }
            },
            { "sealed swords", new List<string>
                {
                    "sealed sword", "gran faust", "faust", "divine avenger", "avenger"
                }
            },
            { "pulars", new List<string>
                {
                    "freezing pulsar", "flaming pulsar", "kilowatt pulsar", "heavy pulsar", "frozen pulsar",
                    "blazing pulsar", "radiant pulsar", "gigawatt pulsar", "wildfire", "permafroster", "supernova",
                    "polaris", "pulsar"
                }
            },
            { "catalyzers", new List<string>
                {
                    "toxic catalyzer", "industrial catalyzer", "volatile catalyzer", "virulent catalyzer", "neutralizer",
                    "biohazard", "catalyzer"
                }
            },
            { "alchemers", new List<string>
                {
                    "alchemer", "shadowtech alchemer mk ii", "prismatech alchemer mk ii", "firotech alchemer mk ii",
                    "cryotech alchemer mk ii", "volt driver", "shadow driver", "prisma driver", "firo driver",
                    "cryo driver", "umbra driver", "storm driver", "nova driver", "magma driver", "hail driver",
                    "voltech alchemer", "shadowtech alchemer", "prismatech alchemer", "firotech alchemer", "cryotech alchemer"
                }
            },
            { "autoguns", new List<string>
                {
                    "autogun", "dark chaingun", "toxic needle", "needle shot", "black chaingun", "blight needle",
                    "strike needle", "fiery pepperbox", "grim repeater", "plague needle", "volcanic pepperbox",
                    "blitz needle", "pepperbox"
                }
            },
            { "blasters", new List<string>
                {
                    "shadow blaster", "pierce blaster", "elemental blaster", "super blaster", "umbral blaster",
                    "fusion blaster", "breach blaster", "master blaster", "riftlocker", "phantamos", "arcana",
                    "valiance", "blaster"
                }
            },
            { "magnuses", new List<string>
                {
                    "mega tundrus", "mega magnus", "iron slug", "winter grave", "callahan", "tundrus", "magnus"
                }
            },
            { "torto guns", new List<string>
                {
                    "wild buster", "stoic buster", "primal buster", "grim buster", "nether cannon", "mighty cannon",
                    "feral cannon", "barrier cannon", "savage tortofist", "omega tortofist", "grand tortofist", "gorgofist"
                }
            },
            { "antiguas", new List<string>
                {
                    "antigua", "raptor", "silversix", "blackhawk", "gilded griffin", "obsidian carbine", "sentenza",
                    "argent peacemaker"
                }
            },
            { "shard bombs", new List<string>
                {
                    "super splinter bomb", "super shard bomb", "super dark matter bomb", "super crystal bomb",
                    "rock salt bomb", "radiant sun shards", "ionized salt bomb", "heavy splinter bomb",
                    "heavy shard bomb", "heavy dark matter bomb", "heavy crystal bomb", "shocking salt bomb",
                    "scintillating sun shards", "deadly splinter bomb", "deadly shard bomb", "deadly dark matter bomb",
                    "deadly crystal bomb", "splinter bomb", "shard bomb", "dark matter bomb", "crystal bomb",
                    "sun shards", "shard bomb"
                }
            },
            { "mist bombs", new List<string>
                {
                    "haze bomb mk ii", "lightning capacitor", "toxic vaporizer mk ii", "slumber smogger mk ii",
                    "freezing vaporizer mk ii", "fiery vaporizer mk ii", "haze burst", "plasma capacitor",
                    "toxic atomizer", "slumber squall", "freezing atomizer", "fiery atomizer", "stagger storm",
                    "voltaic tempest", "venom veiler", "torpor tantrum", "shivermist buster", "ash of agni",
                    "haze bomb", "static capacitor", "toxic vaporizer", "slumber smogger", "freezing vaporizer",
                    "fiery vaporizer", "mist bomb"
                }
            },
            { "snarb bombs", new List<string>
                {
                    "twisted spine cone", "spike shower", "dark briar barrage", "spine cone"
                }
            },
            { "blast bombs", new List<string>
                {
                    "super blast bomb", "master blast bomb", "irontech bomb", "heavy deconstructor",
                    "nitronome", "irontech destroyer", "big angry bomb", "blast bomb", "deconstructor"
                }
            },
            { "vortexes 1", new List<string>
                {
                    "electron charge", "electron bomb", "electron vortex"
                }
            },
            { "vortexes 2", new List<string>
                {
                    "graviton charge", "graviton bomb", "obsidian crusher", "graviton vortex"
                }
            },
            { "wolver sets", new List<string>
                {
                    "wolver coat", "padded hunting coat", "dusker coat", "quilted hunting coat",
                    "ash tail coat", "vog cub coat", "starlit hunting coat", "snarbolax coat",
                    "skolver coat", "wolver cap", "padded hunting cap", "dusker cap",
                    "quilted hunting cap", "ash tail cap", "vog cub cap", "starlit hunting cap",
                    "snarbolax cap", "skolver cap"
                }
            },
            { "cloak sets", new List<string>
                {
                    "magic cloak", "elemental cloak", "miracle cloak", "chaos cloak",
                    "divine mantle", "grey feather mantle", "magic hood", "elemental hood",
                    "miracle hood", "chaos cowl", "divine veil", "grey feather cowl"
                }
            },
            { "kat sets", new List<string>
                {
                    "kat hiss cloak", "kat hiss mail", "kat hiss raiment", "kat hiss hood",
                    "kat hiss mask", "kat hiss cowl", "kat eye cloak", "kat eye mail",
                    "kat eye raiment", "kat eye hood", "kat eye mask", "kat eye cowl",
                    "kat claw cloak", "kat claw mail", "kat claw raiment", "kat claw hood",
                    "kat claw mask", "kat claw cowl"
                }
            },
            { "bkat cloaks", new List<string>
                {
                    "black kat cloak", "black kat mail", "black kat raiment"
                }
            },
            { "gunslinger sets", new List<string>
                {
                    "gunslinger sash", "sunset duster", "deadshot mantle", "justifier jacket",
                    "nameless poncho", "shadowsun slicker", "gunslinger hat", "sunset stetson",
                    "deadshot chapeau", "justifier hat", "nameless hat", "shadowsun stetson"
                }
            },
            { "demo sets", new List<string>
                {
                    "spiral demo suit", "fused demo suit", "padded demo suit", "heavy demo suit",
                    "quilted demo suit", "bombastic demo suit", "mad bomber suit",
                    "mercurial demo suit", "volcanic demo suit", "starlit demo suit",
                    "spiral demo helm", "fused demo helm", "padded demo helm", "heavy demo helm",
                    "quilted demo helm", "bombastic demo helm", "mad bomber mask",
                    "mercurial demo helm", "volcanic demo helm", "starlit demo helm"
                }
            },
            { "skelly sets", new List<string>
                {
                    "skelly suit", "scary skelly suit", "sinister skelly suit", "dread skelly suit",
                    "skelly mask", "scary skelly mask", "sinister skelly mask", "dread skelly mask"
                }
            },
            { "cobalt sets", new List<string>
                {
                    "solid cobalt armor", "mighty cobalt armor", "azure guardian armor",
                    "almirian crusador armor", "cobalt armor", "solid cobalt helm",
                    "mighty cobalt helm", "azure guardian helm", "almirian crusader helm",
                    "cobalt helm"
                }
            },
            { "jelly sets", new List<string>
                {
                    "brute jelly mail", "rock jelly mail", "ice queen mail", "royal jelly mail",
                    "brute jelly helm", "rock jelly helm", "ice queen crown", "royal jelly crown",
                    "jelly helm", "jelly mail", "charged quicksilver mail", "mercurial mail",
                    "charged quicksilver helm", "mercurial helm", "quicksilver mail",
                    "quicksilver helm"
                }
            },
            { "plate sets", new List<string>
                {
                    "spiral plate mail", "boosted plate mail", "heavy plate mail",
                    "ironmight plate mail", "volcanic plate mail", "spiral plate helm",
                    "boosted plate helm", "heavy plate helm", "ironmight plate helm",
                    "volcanic plate helm"
                }
            },
            { "chroma sets", new List<string>
                {
                    "chroma suit", "arcane salamander suit", "volcanic salamander suit",
                    "deadly virulisk suit", "salamander suit", "virulisk suit",
                    "chroma mask", "arcane salamander mask", "volcanic salamander mask",
                    "deadly virulisk mask", "salamander mask", "virulisk mask"
                }
            },
            { "angelic sets", new List<string>
                {
                    "angelic raiment", "seraphic mail", "armor of the fallen",
                    "heavenly iron armor", "valkyrie mail", "angelic helm",
                    "seraphic helm", "crown of the fallen", "heavenly iron helm",
                    "valkyrie helm"
                }
            },
            { "scale sets", new List<string>
                {
                    "drake scale mail", "wyvern scale mail", "silvermail", "dragon scale mail",
                    "radiant silvermail", "drake scale helm", "wyvern scale helm",
                    "dragon scale helm"
                }
            },
            { "pathfinder sets", new List<string>
                {
                    "woven falcon pathfinder armor", "woven firefly pathfinder armor", "woven grizzle pathfinder armor",
                    "woven snakebite pathfinder armor", "plated falcon pathfinder armor", "plated firefly pathfinder armor",
                    "plated grizzly pathfinder armor", "plated snakebite pathfinder armor", "sacred falcon pathfinder armor",
                    "sacred falcon guerrila armor", "sacred falcon hazard armor", "sacred firefly pathfinder armor",
                    "sacred firefly guerilla armor", "sacred firefly hazard armor", "sacred grizzly pathfinder armor",
                    "sacred grizzly guerilla armor", "sacred grizzly hazard armor", "sacred snakebite pathfinder armor",
                    "sacred snakebite guerilla armor", "sacred snakebite hazard armor", "woven falcon pathfinder helm",
                    "woven firefly pathfinder helm", "woven grizzly pathfinder helm", "woven snakebite pathfinder helm",
                    "plated falcon pathfinder helm", "plated firefly pathfinder helm", "plated grizzly pathfinder helm",
                    "plated snakebite pathfinder helm", "sacred falcon pathfinder helm", "sacred falcon guerilla helm",
                    "sacred falcon hazard helm", "sacred firefly pathfinder helm", "sacred firefly guerilla helm",
                    "sacred firefly hazard helm", "sacred grizzly pathfinder helm", "sacred grizzly guerilla helm",
                    "sacred grizzly hazard helm", "sacred snakebite pathfinder helm", "sacred snakebite guerilla helm",
                    "sacred snakebite hazard helm", "pathfinder armor", "pathfinder helm"
                }
            },
            { "sentinel sets", new List<string>
                {
                    "woven falcon sentinel armor", "woven firefly sentinel armor", "woven grizzly sentinel armor",
                    "woven snakebite sentinel armor", "plated falcon sentinel armor", "plated firefly sentinel armor",
                    "plated grizzly sentinel armor", "plated snakebite sentinel armor", "sacred falcon sentinel armor",
                    "sacred falcon keeper armor", "sacred falcon wraith armor", "sacred firefly sentinel armor",
                    "sacred firefly keeper armor", "sacred firefly wraith armor", "sacred grizzly sentinel armor",
                    "sacred grizzly keeper armor", "sacred grizzly wraith armor", "sacred snakebite sentinel armor",
                    "sacred snakebite keeper armor", "sacred snakebite wraith armor", "woven falcon sentinel helm",
                    "woven firefly sentinel helm", "woven grizzly sentinel helm", "woven snakebite sentinel helm",
                    "plated falcon sentinel helm", "plated firefly sentinel helm", "plated grizzly sentinel helm",
                    "plated snakebite sentinel helm", "sacred falcon sentinel helm", "sacred falcon keeper helm",
                    "sacred falcon wraith helm", "sacred firefly sentinel helm", "sacred firefly keeper helm",
                    "sacred firefly wraith helm", "sacred grizzly sentinel helm", "sacred grizzly keeper helm",
                    "sacred grizzly wraith helm", "sacred snakebite sentinel helm", "sacred snakebite keeper helm",
                    "sacred snakebite wraith helm", "sentinel armor", "sentinel helm"
                }
            },
            { "shade sets", new List<string>
                {
                    "woven falcon shade armor", "woven firefly shade armor", "woven grizzly shade armor",
                    "woven snakebite shade armor", "plated falcon shade armor", "plated firefly shade armor",
                    "plated grizzly shade armor", "plated snakebite shade armor", "sacred falcon shade armor",
                    "sacred falcon ghost armor", "sacred falcon hex armor", "sacred firefly shade armor",
                    "sacred firefly ghost armor", "sacred firefly hex armor", "sacred grizzly shade armor",
                    "sacred grizzly ghost armor", "sacred grizzly hex armor", "sacred snakebite shade armor",
                    "sacred snakebite ghost armor", "sacred snakebite hex armor", "woven falcon shade helm",
                    "woven firefly shade helm", "woven grizzly shade helm", "woven snakebite shade helm",
                    "plated falcon shade helm", "plated firefly shade helm", "plated grizzly shade helm",
                    "plated snakebite shade helm", "sacred falcon shade helm", "sacred falcon ghost helm",
                    "sacred falcon hex helm", "sacred firefly shade helm", "sacred firefly ghost helm",
                    "sacred firefly hex helm", "sacred grizzly shade helm", "sacred grizzly ghost helm",
                    "sacred grizzly hex helm", "sacred snakebite shade helm", "sacred snakebite ghost helm",
                    "sacred snakebite hex helm", "shade armor", "shade helm"
                }
            },
            { "snarb shields", new List<string>
                {
                    "bristling buckler", "twisted targe", "dark thorn shield", "barbarous thorn shield"
                }
            },
            { "skelly shields", new List<string>
                {
                    "scary skelly shield", "sinister skelly shield", "dread skelly shield", "skelly shield"
                }
            },
            { "plate shields", new List<string>
                {
                    "boosted plate shield", "heavy plate shield", "ironmight plate shield",
                    "volcanic plate shield", "plate shield"
                }
            },
            { "owlite shields", new List<string>
                {
                    "horned owlite shield", "wise owlite shield", "grey owlite shield", "owlite shield"
                }
            },
            { "jelly shields", new List<string>
                {
                    "brute jelly shield", "rock jelly shield", "royal jelly shield", "jelly shield"
                }
            },
            { "defenders", new List<string>
                {
                    "great defender", "mighty defender", "aegis", "heater shield", "defender"
                }
            },
            { "scale shields", new List<string>
                {
                    "drake scale shield", "wyvern scale shield", "stone tortoise",
                    "dragon scale shield", "omega shell"
                }
            },
            { "torto shields", new List<string>
                {
                    "wild shell", "feral shell", "savage tortoise", "grim shell",
                    "nether shell", "gorgomega", "stoic shell", "mighty shell",
                    "grand tortoise", "primal shell", "barrier shell", "omegaward"
                }
            }
        };

        public readonly Dictionary<string, List<string>> colorSets = new()
        {
            { "standard", new List<string>
                {
                    "cool", "dusky", "fancy", "heavy", "military", "regal", "toasty"
                }
            },
            { "div volc", new List<string>
                {
                    "divine", "volcanic"
                }
            },
            { "gems", new List<string>
                {
                    "ruby", "peridot", "sapphire", "opal", "citrine", "turquoise",
                    "garnet", "amethyst", "aquamarine", "diamond", "emerald", "pearl"
                }
            },
            { "shadowtech", new List<string>
                {
                    "shadowtech blue", "shadowtech green", "shadowtech orange", "shadowtech pink"
                }
            },
            { "tech", new List<string>
                {
                    "tech blue", "tech green", "tech orange", "tech pink"
                }
            },
            { "snipes", new List<string>
                {
                    "fern", "lavender", "lemon", "peach", "rose", "sky", "vanilla",
                    "cocoa", "cherry", "lime", "mint", "plum", "sage", "wheat"
                }
            },
            { "winterfest onesies", new List<string>
                {
                    "candy striped", "festively striped", "holly striped", "joyous striped"
                }
            },
            { "winterfest pullovers", new List<string>
                {
                    "flashy winter", "garish winter", "gaudy winter", "tacky winter"
                }
            },
            { "kat suits", new List<string>
                {
                    "feral kat", "primal kat", "savage kat", "wild kat"
                }
            },
            { "polar colors", new List<string>
                {
                    "polar day", "polar night", "polar twilight"
                }
            },
            { "buhgok tails", new List<string>
                {
                    "black buhgok", "brown buhgok", "gold buhgok"
                }
            },
            { "fowls", new List<string>
                {
                    "black fowl", "brown fowl", "gold fowl"
                }
            },
            { "buckled coats", new List<string>
                {
                    "ancient gold", "hunter gold", "peridot gold"
                }
            },
            { "clovers", new List<string>
                {
                    "ancient clover", "hunter clover", "peridot clover"
                }
            },
            { "lapel clover", new List<string>
                {
                    "ancient lapel", "hunter lapel", "peridot lapel"
                }
            },
            { "lucky toppers", new List<string>
                {
                    "ancient lucky", "hunter lucky", "peridot lucky"
                }
            },
            { "lucky pipes", new List<string>
                {
                    "ancient pipe", "hunter pipe", "peridot pipe"
                }
            },
            { "lucky beards", new List<string>
                {
                    "autumn lumberfell", "citrine lumberfell", "dazed lumberfell"
                }
            },
            { "obsidian", new List<string>
                {
                    "influence", "devotion", "rituals", "sight"
                }
            },
            { "raider", new List<string>
                {
                    "winter raider", "squall raider", "firestorm raider"
                }
            },
            { "battle chef 1", new List<string>
                {
                    "black battle", "white battle"
                }
            },
            { "battle chef 2", new List<string>
                {
                    "blue battle", "red battle"
                }
            },
            { "battle chef 3", new List<string>
                {
                    "pink battle", "purple battle", "yellow battle"
                }
            },
            { "rose 1", new List<string>
                {
                    "the black rose", "the white rose"
                }
            },
            { "rose 2", new List<string>
                {
                    "the gold rose", "the green rose", "the blue rose", "the red rose"
                }
            },
            { "rose 3", new List<string>
                {
                    "the amethyst rose", "the aquamarine rose", "the citrine rose",
                    "the garnet rose", "the malachite rose", "the moonstone rose",
                    "the turquoise rose"
                }
            },
            { "rose 4", new List<string>
                {
                    "the coral rose", "the violet rose"
                }
            }
        };

        public readonly UVTermsData UVTerms = new();

        public class UVTermsData
        {
            public readonly List<string> Types = ["ctr", "asi", "normal", "shadow", "fire", "shock", "poison", "stun", "freeze", "elemental", "piercing"];
            public readonly List<string> Grades = ["low", "med", "high", "very", "max"];
        }
    }
}
