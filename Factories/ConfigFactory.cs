using Microsoft.Extensions.Configuration;

namespace Kozma.net.Factories;

public class ConfigFactory : IConfigFactory
{
    private readonly IConfigurationRoot config;

    public ConfigFactory()
    {
        config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddUserSecrets<Program>()
            .Build();
    }

    public IConfigurationRoot GetConfig()
    {
        return config;
    }
}
