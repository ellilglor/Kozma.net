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

    [SlashCommand("shardsweeper", "Kozma's Backpack staff only.")] // TODO write description
    public async Task ExecuteAsync()
    {
        var limit = 9;
        var field = new int[limit, limit];
        var finalField = "";

        SetShards(field, limit);

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (field[row, col] is 0) field[row, col] = 0;
                finalField += TranslateToEmote(field[row, col]).PutSpoiler();
            }

            finalField += "\n";
        }

        await ModifyOriginalResponseAsync(msg => msg.Content = finalField);
    }

    private static bool IsValidPos(int row, int col, int limit) =>
        row >= 0 && col >= 0 && row <= limit - 1 && col <= limit - 1;

    private static void SetShards(int[,] field, int limit)
    {
        var shardLimit = 4;
        var maxShards = 10;
        var shardCount = 0;

        while (shardCount <= maxShards)
        {
            var rowIndex = _random.Next(0, limit);
            var colIndex = _random.Next(0, limit);
            if (field[rowIndex, colIndex] == _shard) continue;

            // Check adjacent fields in row above
            if (IsValidPos(rowIndex - 1, colIndex - 1, limit) && field[rowIndex - 1, colIndex - 1] == shardLimit) continue;
            if (IsValidPos(rowIndex - 1, colIndex, limit) && field[rowIndex - 1, colIndex] == shardLimit) continue;
            if (IsValidPos(rowIndex - 1, colIndex + 1, limit) && field[rowIndex - 1, colIndex + 1] == shardLimit) continue;

            // Check adjacent fields in same row
            if (IsValidPos(rowIndex, colIndex - 1, limit) && field[rowIndex, colIndex - 1] == shardLimit) continue;
            if (IsValidPos(rowIndex, colIndex + 1, limit) && field[rowIndex, colIndex + 1] == shardLimit) continue;

            // Check adjacent fields in row below
            if (IsValidPos(rowIndex + 1, colIndex - 1, limit) && field[rowIndex + 1, colIndex - 1] == shardLimit) continue;
            if (IsValidPos(rowIndex + 1, colIndex, limit) && field[rowIndex + 1, colIndex] == shardLimit) continue;
            if (IsValidPos(rowIndex + 1, colIndex + 1, limit) && field[rowIndex + 1, colIndex + 1] == shardLimit) continue;

            // Shard can be placed
            field[rowIndex, colIndex] = _shard;
            shardCount++;

            // Increase indicators in adjacent fields
            if (IsValidPos(rowIndex - 1, colIndex - 1, limit) && field[rowIndex - 1, colIndex - 1] != _shard) field[rowIndex - 1, colIndex - 1]++;
            if (IsValidPos(rowIndex - 1, colIndex, limit) && field[rowIndex - 1, colIndex] != _shard) field[rowIndex - 1, colIndex]++;
            if (IsValidPos(rowIndex - 1, colIndex + 1, limit) && field[rowIndex - 1, colIndex + 1] != _shard) field[rowIndex - 1, colIndex + 1]++;
            if (IsValidPos(rowIndex, colIndex - 1, limit) && field[rowIndex, colIndex - 1] != _shard) field[rowIndex, colIndex - 1]++;
            if (IsValidPos(rowIndex, colIndex + 1, limit) && field[rowIndex, colIndex + 1] != _shard) field[rowIndex, colIndex + 1]++;
            if (IsValidPos(rowIndex + 1, colIndex - 1, limit) && field[rowIndex + 1, colIndex - 1] != _shard) field[rowIndex + 1, colIndex - 1]++;
            if (IsValidPos(rowIndex + 1, colIndex, limit) && field[rowIndex + 1, colIndex] != _shard) field[rowIndex + 1, colIndex]++;
            if (IsValidPos(rowIndex + 1, colIndex + 1, limit) && field[rowIndex + 1, colIndex + 1] != _shard) field[rowIndex + 1, colIndex + 1]++;
        }
    }

    private static string TranslateToEmote(int num)
    {
        return num switch
        {
            _shard => Emotes.Shard,
            1 => Emotes.One,
            2 => Emotes.Two,
            3 => Emotes.Three,
            4 => Emotes.Four,
            _ => Emotes.Square
        };
    }

}