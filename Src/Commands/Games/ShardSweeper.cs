using Discord.Interactions;
using Discord;
using Kozma.net.Src.Data.Classes;
using Kozma.net.Src.Extensions;

namespace Kozma.net.Src.Commands.Games;

[DontAutoRegister]
[DefaultMemberPermissions(GuildPermission.Administrator | GuildPermission.KickMembers | GuildPermission.BanMembers)]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1814:Prefer jagged arrays over multidimensional", Justification = "Array will be completely filled")]
public class ShardSweeper() : InteractionModuleBase<SocketInteractionContext>
{
    private static readonly Random _random = new();
    private const int _shard = -1;
    private const int _size = 9;
    private const int _shardLimit = 4;

    [SlashCommand("shardsweeper", "Kozma's Backpack staff only.")] // TODO write description
    [ComponentInteraction("shardsweeper-reload")]
    public async Task ExecuteAsync()
    {
        var field = new int[_size, _size];
        var finalField = "";

        SetShards(field);

        for (int row = 0; row < _size; row++)
        {
            for (int col = 0; col < _size; col++)
            {
                finalField += TranslateToEmote(field[row, col]).PutSpoiler();
            }

            finalField += "\n";
        }

        var components = new ComponentBuilder()
            .WithButton(emote: new Emoji(Emotes.Repeat), customId: "shardsweeper-reload", style: ButtonStyle.Secondary)
            .WithButton(emote: new Emoji(Emotes.QMark), customId: "shardsweeper-info", style: ButtonStyle.Primary);

        await ModifyOriginalResponseAsync(msg =>
        {
            msg.Content = finalField;
            msg.Components = components.Build();
            msg.Embed = null;
        });
    }

    private static void SetShards(int[,] field)
    {
        var maxShards = 10;
        var shardCount = 0;

        while (shardCount <= maxShards)
        {
            var rowIndex = _random.Next(0, _size);
            var colIndex = _random.Next(0, _size);
            if (field[rowIndex, colIndex] == _shard) continue;

            // Check adjacent fields in row above
            if (IsValidPos(rowIndex - 1, colIndex - 1, _size) && field[rowIndex - 1, colIndex - 1] == _shardLimit) continue;
            if (IsValidPos(rowIndex - 1, colIndex, _size) && field[rowIndex - 1, colIndex] == _shardLimit) continue;
            if (IsValidPos(rowIndex - 1, colIndex + 1, _size) && field[rowIndex - 1, colIndex + 1] == _shardLimit) continue;

            // Check adjacent fields in same row
            if (IsValidPos(rowIndex, colIndex - 1, _size) && field[rowIndex, colIndex - 1] == _shardLimit) continue;
            if (IsValidPos(rowIndex, colIndex + 1, _size) && field[rowIndex, colIndex + 1] == _shardLimit) continue;

            // Check adjacent fields in row below
            if (IsValidPos(rowIndex + 1, colIndex - 1, _size) && field[rowIndex + 1, colIndex - 1] == _shardLimit) continue;
            if (IsValidPos(rowIndex + 1, colIndex, _size) && field[rowIndex + 1, colIndex] == _shardLimit) continue;
            if (IsValidPos(rowIndex + 1, colIndex + 1, _size) && field[rowIndex + 1, colIndex + 1] == _shardLimit) continue;

            // Shard can be placed
            field[rowIndex, colIndex] = _shard;
            shardCount++;

            // Increase indicators in adjacent fields
            if (IsValidPos(rowIndex - 1, colIndex - 1, _size) && field[rowIndex - 1, colIndex - 1] != _shard) field[rowIndex - 1, colIndex - 1]++;
            if (IsValidPos(rowIndex - 1, colIndex, _size) && field[rowIndex - 1, colIndex] != _shard) field[rowIndex - 1, colIndex]++;
            if (IsValidPos(rowIndex - 1, colIndex + 1, _size) && field[rowIndex - 1, colIndex + 1] != _shard) field[rowIndex - 1, colIndex + 1]++;
            if (IsValidPos(rowIndex, colIndex - 1, _size) && field[rowIndex, colIndex - 1] != _shard) field[rowIndex, colIndex - 1]++;
            if (IsValidPos(rowIndex, colIndex + 1, _size) && field[rowIndex, colIndex + 1] != _shard) field[rowIndex, colIndex + 1]++;
            if (IsValidPos(rowIndex + 1, colIndex - 1, _size) && field[rowIndex + 1, colIndex - 1] != _shard) field[rowIndex + 1, colIndex - 1]++;
            if (IsValidPos(rowIndex + 1, colIndex, _size) && field[rowIndex + 1, colIndex] != _shard) field[rowIndex + 1, colIndex]++;
            if (IsValidPos(rowIndex + 1, colIndex + 1, _size) && field[rowIndex + 1, colIndex + 1] != _shard) field[rowIndex + 1, colIndex + 1]++;
        }
    }

    private static bool IsValidPos(int row, int col, int size) =>
        row >= 0 && col >= 0 && row <= size - 1 && col <= size - 1;

    private static string TranslateToEmote(int num)
    {
        return num switch
        {
            _shard => Emotes.Shard,
            1 => Emotes.One,
            2 => Emotes.Two,
            3 => Emotes.Three,
            _shardLimit => Emotes.Four,
            _ => Emotes.Square
        };
    }
}