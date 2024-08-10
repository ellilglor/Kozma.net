using Microsoft.Extensions.Configuration;

namespace Kozma.net.Factories;

public interface IConfigFactory
{
    IConfigurationRoot GetConfig();
}
