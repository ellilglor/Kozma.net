namespace Kozma.net.Src.Interfaces.Handlers;

public interface ITaskHandler
{
    Task LaunchTasksAsync();
    Task CheckIfTaskHandlerIsRunningAsync();
}
