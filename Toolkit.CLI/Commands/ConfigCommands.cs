using System.CommandLine;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Toolkit.CLI.Commands
{
    internal class ConfigCommands : BaseCommand
    {
        private const string ConfigFilePath = "config.json";

        public ConfigCommands() : base("config", "Manage configuration settings such as API keys and cached city.")
        {
        }

        public override void Setup(RootCommand root)
        {
            AddCommand(new SetApiKeyCommand());
            AddCommand(new GetApiKeyCommand());
            AddCommand(new SetCachedCityCommand()); // New command for setting cached city
            root.AddCommand(this);
        }

        internal class SetApiKeyCommand : Command
        {
            public SetApiKeyCommand() : base("set-key", "Set an API key for a specific service.")
            {
                var serviceOption = new Option<string>("--service", "The service to set the API key for (e.g., weather, currency).")
                {
                    IsRequired = true
                };
                var apiKeyOption = new Option<string>("--key", "The API key to set.")
                {
                    IsRequired = true
                };

                AddOption(serviceOption);
                AddOption(apiKeyOption);

                this.SetHandler(ExecuteAsync, serviceOption, apiKeyOption);
            }

            private async Task ExecuteAsync(string service, string key)
            {
                try
                {
                    var config = LoadConfig();

                    if (service.Equals("weather", StringComparison.OrdinalIgnoreCase))
                    {
                        config["weatherapikey"] = key;
                    }
                    else if (service.Equals("currency", StringComparison.OrdinalIgnoreCase))
                    {
                        config["currencyapikey"] = key;
                    }
                    else if(service.Equals("news", StringComparison.OrdinalIgnoreCase))
                    {
                        config["newsapikey"] = key;
                    }
                    else
                    {
                        Logger.LogError("Unknown service. Use 'weather' or 'currency'.");
                        return;
                    }

                    SaveConfig(config);
                    Logger.LogSuccess($"API key for '{service}' set successfully.");
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Failed to set API key: {ex.Message}");
                }
            }
        }

        internal class GetApiKeyCommand : Command
        {
            public GetApiKeyCommand() : base("get-key", "Get the API key for a specific service.")
            {
                var serviceOption = new Option<string>("--service", "The service to get the API key for (e.g., weather, currency).")
                {
                    IsRequired = true
                };

                AddOption(serviceOption);

                this.SetHandler(ExecuteAsync, serviceOption);
            }

            private async Task ExecuteAsync(string service)
            {
                try
                {
                    var config = LoadConfig();

                    if (service.Equals("weather", StringComparison.OrdinalIgnoreCase))
                    {
                        if (config.TryGetValue("weatherapikey", out var key))
                        {
                            Logger.LogInformation($"API key for '{service}': {key}");
                        }
                        else
                        {
                            Logger.LogError($"No API key found for '{service}'.");
                        }
                    }
                    else if (service.Equals("currency", StringComparison.OrdinalIgnoreCase))
                    {
                        if (config.TryGetValue("currencyapikey", out var key))
                        {
                            Logger.LogInformation($"API key for '{service}': {key}");
                        }
                        else
                        {
                            Logger.LogError($"No API key found for '{service}'.");
                        }
                    }
                    else if (service.Equals("news", StringComparison.OrdinalIgnoreCase))
                    {
                        if (config.TryGetValue("newsapikey", out var key))
                        {
                            Logger.LogInformation($"API key for '{service}': {key}");
                        }
                        else
                        {
                            Logger.LogError($"No API key found for '{service}'.");
                        }
                    }
                    else
                    {
                        Logger.LogError("Unknown service. Use 'weather','currency' or 'news'.");
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Failed to get API key: {ex.Message}");
                }
            }
        }

        // New command to set the cached city
        internal class SetCachedCityCommand : Command
        {
            public SetCachedCityCommand() : base("set-city", "Set the cached city for weather queries.")
            {
                var cityArg = new Argument<string>("city", "The name of the city to cache for weather queries.");

                AddArgument(cityArg);

                this.SetHandler(ExecuteAsync, cityArg);
            }

            private async Task ExecuteAsync(string city)
            {
                try
                {
                    var config = LoadConfig();
                    config["cachedCity"] = city;
                    SaveConfig(config);

                    Logger.LogSuccess($"Cached city set to '{city}'.");
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Failed to set cached city: {ex.Message}");
                }
            }
        }

        private static Dictionary<string, string> LoadConfig()
        {
            if (!File.Exists(ConfigFilePath))
            {
                return new Dictionary<string, string>();
            }

            var json = File.ReadAllText(ConfigFilePath);
            return JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
        }

        private static void SaveConfig(Dictionary<string, string> config)
        {
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ConfigFilePath, json);
        }
    }
}
