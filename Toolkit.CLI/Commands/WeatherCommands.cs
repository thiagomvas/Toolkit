using System.CommandLine;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Toolkit.CLI.Commands
{
    internal class WeatherCommands : BaseCommand
    {
        private const string ConfigFilePath = "config.json";
        private const string ApiKeyWebsite = "https://www.weatherapi.com/";

        public WeatherCommands() : base("weather", "Fetch current weather data for a specified city and state using the WeatherAPI.")
        {
        }

        public override void Setup(RootCommand root)
        {
            AddCommand(new CurrentWeatherCommand());
            AddCommand(new ApiKeyInfoCommand());
            root.AddCommand(this);
        }

        internal class CurrentWeatherCommand : Command
        {
            public CurrentWeatherCommand() : base("current", "Get the current weather for a specified city and optionally state.")
            {
                var cityOption = new Option<string>("--city", "The name of the city to get the weather for.")
                {
                    IsRequired = true
                };
                var stateOption = new Option<string>("--state", "The state or region code (e.g., CA for California).");
                var countryOption = new Option<string>("--country", "The country code (e.g., US for the United States).");

                AddOption(cityOption);
                AddOption(stateOption);
                AddOption(countryOption);

                this.SetHandler(ExecuteAsync, cityOption, stateOption, countryOption);
            }

            private async Task ExecuteAsync(string city, string? state, string? country)
            {
                try
                {
                    var apiKey = LoadApiKey("weatherapikey");
                    if (string.IsNullOrWhiteSpace(apiKey))
                    {
                        Logger.LogError("API key for weather data is not set. Use 'config set-key' to set it.");
                        Logger.LogInformation($"Obtain an API key from: {ApiKeyWebsite}");
                        return;
                    }

                    var location = $"{city}{(state != null ? $",{state}" : "")}{(country != null ? $",{country}" : "")}";
                    var apiUrl = $"http://api.weatherapi.com/v1/current.json?q={Uri.EscapeDataString(location)}&key={apiKey}";

                    using var httpClient = new HttpClient();
                    var response = await httpClient.GetStringAsync(apiUrl);
                    var data = JsonDocument.Parse(response).RootElement;

                    if (data.TryGetProperty("current", out var current) &&
                        current.TryGetProperty("condition", out var condition) &&
                        condition.TryGetProperty("text", out var text) &&
                        current.TryGetProperty("temp_c", out var temp))
                    {
                        var weatherDescription = text.GetString();
                        var temperature = temp.GetDecimal();

                        Logger.LogInformation($"Current weather in {city}{(state != null ? $", {state}" : "")}{(country != null ? $", {country}" : "")}: {weatherDescription}");
                        Logger.LogSuccess($"Temperature: {temperature}°C");
                    }
                    else
                    {
                        Logger.LogError($"Could not retrieve weather data for {city}{(state != null ? $", {state}" : "")}{(country != null ? $", {country}" : "")}.");
                    }
                }
                catch (HttpRequestException ex)
                {
                    Logger.LogError($"Failed to fetch weather data: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Logger.LogError($"An error occurred: {ex.Message}");
                }
            }

            private static string LoadApiKey(string keyName)
            {
                if (!File.Exists(ConfigFilePath))
                {
                    return null;
                }

                var json = File.ReadAllText(ConfigFilePath);
                var config = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
                config.TryGetValue(keyName, out var apiKey);
                return apiKey;
            }
        }

        internal class ApiKeyInfoCommand : Command
        {
            public ApiKeyInfoCommand() : base("api-key-info", "Get information on how to obtain an API key for weather data.")
            {
            }

            private void Execute()
            {
                Logger.LogInformation($"To obtain a weather API key, visit: {ApiKeyWebsite}");
            }
        }
    }
}
