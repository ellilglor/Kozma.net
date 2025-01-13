using Discord;
using Discord.Interactions;
using Kozma.net.Src.Data.Classes;

namespace Kozma.net.Src.Commands.Games;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1814:Prefer jagged arrays over multidimensional", Justification = "Array will be completely filled")]
public class ShardSweeper() : InteractionModuleBase<SocketInteractionContext>
{
    private static readonly Random _random = new();
    private const int _shard = -1;
    private const int _size = 9;
    private const int _shardLimit = 4;

    private static readonly List<string> _templateFields =
    [
        "\U0001f7e61️⃣<:kbpdarkshard:839985279061721098>2️⃣<:kbpdarkshard:839985279061721098><:kbpdarkshard:839985279061721098><:kbpdarkshard:839985279061721098>1️⃣\U0001f7e6\n\U0001f7e61️⃣2️⃣4️⃣4️⃣4️⃣2️⃣2️⃣1️⃣\n\U0001f7e6\U0001f7e61️⃣<:kbpdarkshard:839985279061721098><:kbpdarkshard:839985279061721098>2️⃣\U0001f7e61️⃣<:kbpdarkshard:839985279061721098>\n\U0001f7e6\U0001f7e61️⃣3️⃣<:kbpdarkshard:839985279061721098>2️⃣\U0001f7e61️⃣1️⃣\n\U0001f7e6\U0001f7e6\U0001f7e61️⃣1️⃣1️⃣\U0001f7e6\U0001f7e6\U0001f7e6\n\U0001f7e61️⃣1️⃣1️⃣\U0001f7e6\U0001f7e6\U0001f7e6\U0001f7e6\U0001f7e6\n\U0001f7e61️⃣<:kbpdarkshard:839985279061721098>1️⃣\U0001f7e6\U0001f7e6\U0001f7e6\U0001f7e6\U0001f7e6\n\U0001f7e61️⃣1️⃣2️⃣1️⃣1️⃣\U0001f7e6\U0001f7e6\U0001f7e6\n\U0001f7e6\U0001f7e6\U0001f7e61️⃣<:kbpdarkshard:839985279061721098>1️⃣\U0001f7e6\U0001f7e6\U0001f7e6\n",
        "\U0001f7e61️⃣<:kbpdarkshard:839985279061721098>1️⃣\U0001f7e6\U0001f7e6\U0001f7e61️⃣<:kbpdarkshard:839985279061721098>\n\U0001f7e61️⃣1️⃣2️⃣2️⃣2️⃣2️⃣2️⃣2️⃣\n\U0001f7e61️⃣1️⃣2️⃣<:kbpdarkshard:839985279061721098><:kbpdarkshard:839985279061721098>2️⃣<:kbpdarkshard:839985279061721098>1️⃣\n\U0001f7e62️⃣<:kbpdarkshard:839985279061721098>3️⃣2️⃣2️⃣2️⃣1️⃣1️⃣\n\U0001f7e62️⃣<:kbpdarkshard:839985279061721098>2️⃣\U0001f7e6\U0001f7e6\U0001f7e6\U0001f7e6\U0001f7e6\n1️⃣2️⃣1️⃣1️⃣\U0001f7e6\U0001f7e6\U0001f7e61️⃣1️⃣\n<:kbpdarkshard:839985279061721098>1️⃣\U0001f7e6\U0001f7e6\U0001f7e6\U0001f7e6\U0001f7e61️⃣<:kbpdarkshard:839985279061721098>\n1️⃣2️⃣1️⃣1️⃣\U0001f7e6\U0001f7e6\U0001f7e61️⃣1️⃣\n\U0001f7e61️⃣<:kbpdarkshard:839985279061721098>1️⃣\U0001f7e6\U0001f7e6\U0001f7e6\U0001f7e6\U0001f7e6\n",
        "2️⃣<:kbpdarkshard:839985279061721098>1️⃣\U0001f7e6\U0001f7e6\U0001f7e6\U0001f7e61️⃣<:kbpdarkshard:839985279061721098>\n<:kbpdarkshard:839985279061721098>2️⃣1️⃣\U0001f7e6\U0001f7e6\U0001f7e6\U0001f7e61️⃣1️⃣\n1️⃣1️⃣\U0001f7e6\U0001f7e6\U0001f7e6\U0001f7e6\U0001f7e6\U0001f7e6\U0001f7e6\n\U0001f7e6\U0001f7e6\U0001f7e6\U0001f7e6\U0001f7e61️⃣1️⃣1️⃣\U0001f7e6\n\U0001f7e6\U0001f7e6\U0001f7e6\U0001f7e6\U0001f7e61️⃣<:kbpdarkshard:839985279061721098>2️⃣1️⃣\n\U0001f7e6\U0001f7e61️⃣1️⃣2️⃣2️⃣2️⃣2️⃣<:kbpdarkshard:839985279061721098>\n\U0001f7e6\U0001f7e61️⃣<:kbpdarkshard:839985279061721098>3️⃣<:kbpdarkshard:839985279061721098>1️⃣1️⃣1️⃣\n1️⃣1️⃣3️⃣3️⃣<:kbpdarkshard:839985279061721098>2️⃣1️⃣\U0001f7e6\U0001f7e6\n1️⃣<:kbpdarkshard:839985279061721098>2️⃣<:kbpdarkshard:839985279061721098>2️⃣1️⃣\U0001f7e6\U0001f7e6\U0001f7e6\n",
        "\U0001f7e61️⃣1️⃣2️⃣1️⃣2️⃣<:kbpdarkshard:839985279061721098>1️⃣\U0001f7e6\n\U0001f7e61️⃣<:kbpdarkshard:839985279061721098>3️⃣<:kbpdarkshard:839985279061721098>2️⃣1️⃣1️⃣\U0001f7e6\n1️⃣2️⃣3️⃣<:kbpdarkshard:839985279061721098>2️⃣1️⃣\U0001f7e6\U0001f7e6\U0001f7e6\n1️⃣<:kbpdarkshard:839985279061721098>2️⃣1️⃣1️⃣\U0001f7e6\U0001f7e61️⃣1️⃣\n1️⃣1️⃣1️⃣\U0001f7e6\U0001f7e6\U0001f7e6\U0001f7e62️⃣<:kbpdarkshard:839985279061721098>\n\U0001f7e6\U0001f7e6\U0001f7e6\U0001f7e6\U0001f7e6\U0001f7e6\U0001f7e62️⃣<:kbpdarkshard:839985279061721098>\n1️⃣2️⃣1️⃣1️⃣\U0001f7e6\U0001f7e6\U0001f7e62️⃣2️⃣\n<:kbpdarkshard:839985279061721098>2️⃣<:kbpdarkshard:839985279061721098>1️⃣\U0001f7e6\U0001f7e6\U0001f7e61️⃣<:kbpdarkshard:839985279061721098>\n1️⃣2️⃣1️⃣1️⃣\U0001f7e6\U0001f7e6\U0001f7e61️⃣1️⃣\n",
        "\U0001f7e6\U0001f7e6\U0001f7e6\U0001f7e6\U0001f7e6\U0001f7e6\U0001f7e6\U0001f7e6\U0001f7e6\n\U0001f7e6\U0001f7e6\U0001f7e6\U0001f7e6\U0001f7e6\U0001f7e61️⃣1️⃣1️⃣\n\U0001f7e6\U0001f7e6\U0001f7e61️⃣1️⃣1️⃣1️⃣<:kbpdarkshard:839985279061721098>1️⃣\n\U0001f7e6\U0001f7e6\U0001f7e61️⃣<:kbpdarkshard:839985279061721098>1️⃣1️⃣1️⃣1️⃣\n1️⃣1️⃣1️⃣2️⃣3️⃣3️⃣1️⃣\U0001f7e6\U0001f7e6\n2️⃣<:kbpdarkshard:839985279061721098>1️⃣1️⃣<:kbpdarkshard:839985279061721098><:kbpdarkshard:839985279061721098>2️⃣1️⃣1️⃣\n<:kbpdarkshard:839985279061721098>3️⃣1️⃣1️⃣2️⃣3️⃣3️⃣<:kbpdarkshard:839985279061721098>1️⃣\n<:kbpdarkshard:839985279061721098>2️⃣\U0001f7e61️⃣1️⃣2️⃣<:kbpdarkshard:839985279061721098>2️⃣1️⃣\n1️⃣1️⃣\U0001f7e61️⃣<:kbpdarkshard:839985279061721098>2️⃣1️⃣1️⃣\U0001f7e6\n"
    ];

    [SlashCommand(CommandIds.ShardSweeper, "Clear the field without exposing a Dark Shard.")]
    [ComponentInteraction(ComponentIds.ShardSweepReload)]
    public async Task ExecuteAsync()
    {
        await SendAnimationAsync();

        var field = new int[_size, _size];
        var finalField = "";

        SetShards(field);
        var (startRow, startCol) = DetermineStartCoords(field);

        for (int row = 0; row < _size; row++)
        {
            for (int col = 0; col < _size; col++)
            {
                var emote = TranslateToEmote(field[row, col]);
                finalField += row == startRow && col == startCol ? emote : Format.Spoiler(emote);
            }

            finalField += "\n";
        }

        var components = new ComponentBuilder()
            .WithButton(emote: new Emoji(Emotes.Repeat), customId: ComponentIds.ShardSweepReload, style: ButtonStyle.Secondary)
            .WithButton(emote: new Emoji(Emotes.QMark), customId: ComponentIds.ShardSweepInfo, style: ButtonStyle.Primary);

        await ModifyOriginalResponseAsync(msg =>
        {
            msg.Content = finalField;
            msg.Components = components.Build();
        });
    }

    private async Task SendAnimationAsync()
    {
        var fields = _templateFields.OrderBy(f => _random.Next()).Take(3);

        foreach (var field in fields)
        {
            await ModifyOriginalResponseAsync(msg =>
            {
                msg.Content = field;
                msg.Embed = null;
                msg.Components = null;
            });

            await Task.Delay(TimeSpan.FromMilliseconds(100));
        }
    }

    private static void SetShards(int[,] field)
    {
        var maxShards = 10;
        var shardCount = 0;

        while (shardCount < maxShards)
        {
            var rowIndex = _random.Next(0, _size);
            var colIndex = _random.Next(0, _size);

            if (!CanBePlaced(rowIndex, colIndex, field)) continue;

            field[rowIndex, colIndex] = _shard;
            shardCount++;

            // Increase indicators in adjacent fields
            if (IsValidPos(rowIndex - 1, colIndex - 1) && field[rowIndex - 1, colIndex - 1] != _shard) field[rowIndex - 1, colIndex - 1]++;
            if (IsValidPos(rowIndex - 1, colIndex) && field[rowIndex - 1, colIndex] != _shard) field[rowIndex - 1, colIndex]++;
            if (IsValidPos(rowIndex - 1, colIndex + 1) && field[rowIndex - 1, colIndex + 1] != _shard) field[rowIndex - 1, colIndex + 1]++;
            if (IsValidPos(rowIndex, colIndex - 1) && field[rowIndex, colIndex - 1] != _shard) field[rowIndex, colIndex - 1]++;
            if (IsValidPos(rowIndex, colIndex + 1) && field[rowIndex, colIndex + 1] != _shard) field[rowIndex, colIndex + 1]++;
            if (IsValidPos(rowIndex + 1, colIndex - 1) && field[rowIndex + 1, colIndex - 1] != _shard) field[rowIndex + 1, colIndex - 1]++;
            if (IsValidPos(rowIndex + 1, colIndex) && field[rowIndex + 1, colIndex] != _shard) field[rowIndex + 1, colIndex]++;
            if (IsValidPos(rowIndex + 1, colIndex + 1) && field[rowIndex + 1, colIndex + 1] != _shard) field[rowIndex + 1, colIndex + 1]++;
        }
    }

    private static bool CanBePlaced(int rowIndex, int colIndex, int[,] field)
    {
        // Shard is already present
        if (field[rowIndex, colIndex] == _shard) return false;

        // Check adjacent fields in row above
        if (IsValidPos(rowIndex - 1, colIndex - 1) && field[rowIndex - 1, colIndex - 1] == _shardLimit) return false;
        if (IsValidPos(rowIndex - 1, colIndex) && field[rowIndex - 1, colIndex] == _shardLimit) return false;
        if (IsValidPos(rowIndex - 1, colIndex + 1) && field[rowIndex - 1, colIndex + 1] == _shardLimit) return false;

        // Check adjacent fields in same row
        if (IsValidPos(rowIndex, colIndex - 1) && field[rowIndex, colIndex - 1] == _shardLimit) return false;
        if (IsValidPos(rowIndex, colIndex + 1) && field[rowIndex, colIndex + 1] == _shardLimit) return false;

        // Check adjacent fields in row below
        if (IsValidPos(rowIndex + 1, colIndex - 1) && field[rowIndex + 1, colIndex - 1] == _shardLimit) return false;
        if (IsValidPos(rowIndex + 1, colIndex) && field[rowIndex + 1, colIndex] == _shardLimit) return false;
        if (IsValidPos(rowIndex + 1, colIndex + 1) && field[rowIndex + 1, colIndex + 1] == _shardLimit) return false;

        return true;
    }

    private static bool IsValidPos(int row, int col) =>
        row >= 0 && col >= 0 && row <= _size - 1 && col <= _size - 1;

    private static (int row, int col) DetermineStartCoords(int[,] field)
    {
        while (true)
        {
            var rowIndex = _random.Next(0, _size);
            var colIndex = _random.Next(0, _size);

            if (field[rowIndex, colIndex] != 0) continue;
            return (rowIndex, colIndex);
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
            _shardLimit => Emotes.Four,
            _ => Emotes.Square
        };
    }
}