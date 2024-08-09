using Microsoft.Extensions.Configuration;

namespace Kozma.net.Services;

public interface IConfigFactory
{
    IConfigurationRoot GetConfig();
}
