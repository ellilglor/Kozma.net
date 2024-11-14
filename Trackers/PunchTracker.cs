using Kozma.net.Models;
using System.Text;

namespace Kozma.net.Trackers;

public class PunchTracker : IPunchTracker
{
    private readonly Dictionary<ulong, Dictionary<string, Dictionary<string, List<TrackerItem>>>> _uvs = [];
    private readonly string _types = "Types";
    private readonly string _grades = "Grades";

    public void SetPlayer(ulong id, string key)
    {
        CheckIfIdIsPresent(id, key);
        SetItem(id, key);
    }

    public void AddEntry(ulong id, string key, string type, string grade)
    {
        CheckIfIdIsPresent(id, key);
        var existingType = _uvs[id][key][_types].Find(t => t.Name == type);
        var existingGrade = _uvs[id][key][_grades].Find(g => g.Name == grade);

        if (existingType is null)
        {
            _uvs[id][key][_types].Add(new TrackerItem(type, 1));
        }
        else
        {
            _uvs[id][key][_types][_uvs[id][key][_types].IndexOf(existingType)] = existingType with { Count = existingType.Count + 1 };
        }

        if (existingGrade is null)
        {
            _uvs[id][key][_grades].Add(new TrackerItem(grade, 1));
        }
        else
        {
            _uvs[id][key][_grades][_uvs[id][key][_grades].IndexOf(existingGrade)] = existingGrade with { Count = existingGrade.Count + 1 };
        }
    }

    public string GetData(ulong id, string key)
    {
        CheckIfIdIsPresent(id, key);

        if (!_uvs[id].TryGetValue(key, out Dictionary<string, List<TrackerItem>>? rolled) || rolled[_types].Count == 0)
        {
            return "The bot has restarted and this data is lost!";
        }

        var data = new StringBuilder("**In this session you rolled:**\n");
        var types = rolled[_types].OrderByDescending(i => i.Count);
        var grades = rolled[_grades].OrderByDescending(i => i.Count);

        data.AppendJoin("\n", types.Select(t => $"{t.Name}: {t.Count}"));
        data.AppendLine("\n\n**And got these grades:**");
        data.AppendJoin("\n", grades.Select(g => $"{g.Name}: {g.Count}"));

        return data.ToString();
    }

    private void CheckIfIdIsPresent(ulong id, string key)
    {
        if (!_uvs.ContainsKey(id)) _uvs[id] = [];
        if (!_uvs[id].ContainsKey(key)) SetItem(id, key);
    }

    private void SetItem(ulong id, string key)
    {
        _uvs[id][key] = [];
        _uvs[id][key][_types] = [];
        _uvs[id][key][_grades] = [];
    }
}
