﻿using Kozma.net.Models;

namespace Kozma.net.Services;

public interface ICommandService
{
    public Task<int> GetCommandUsageAsync(bool isGame);
    public Task<IEnumerable<DbStat>> GetCommandsAsync(bool isGame, int total);
}
