﻿using System.CommandLine;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Toolkit.CLI.Commands
{
    internal class WeatherCommands : BaseCommand
    {
        private const string ConfigFilePath = "config.json";
        private const string ApiKeyWebsite = "https://home.openweathermap.org/users/sign_up";

        public WeatherCommands() : base("weather", "Fetch current weather data for a specified city and optionally state using the OpenWeatherMap API.")
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
                    IsRequired = false
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
                    var config = LoadConfig();
                    var apiKey = config.ContainsKey("weatherapikey") ? config["weatherapikey"] : null;

                    if (string.IsNullOrWhiteSpace(apiKey))
                    {
                        Logger.LogError("API key for weather data is not set. Use 'config set-key' to set it.");
                        Logger.LogInformation($"Obtain an API key from: {ApiKeyWebsite}");
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(city))
                    {
                        city = config.ContainsKey("cachedCity") ? config["cachedCity"] : null;

                        if (string.IsNullOrWhiteSpace(city))
                        {
                            Logger.LogError("No city provided and no cached city found.");
                            return;
                        }
                    }

                    var location = $"{city}{(state != null ? $",{state}" : "")}{(country != null ? $",{country}" : "")}";
                    var apiUrl = $"https://api.openweathermap.org/data/2.5/weather?q={Uri.EscapeDataString(location)}&appid={apiKey}&units=metric";

                    using var httpClient = new HttpClient();
                    var response = await httpClient.GetStringAsync(apiUrl);
                    var data = JsonDocument.Parse(response).RootElement;

                    if (data.TryGetProperty("weather", out var weatherArray) &&
                        weatherArray[0].TryGetProperty("description", out var description) &&
                        data.TryGetProperty("main", out var main) &&
                        main.TryGetProperty("temp", out var temp))
                    {
                        var weatherDescription = description.GetString();
                        var temperature = temp.GetDecimal();

                        Logger.LogInformation($"Current weather in {city}{(state != null ? $", {state}" : "")}{(country != null ? $", {country}" : "")}: {weatherDescription}");
                        Logger.LogSuccess($"Temperature: {temperature}°C");

                        if(main.TryGetProperty("feels_like", out var feelsLike))
                            Logger.LogSuccess($"Feels like: {feelsLike.GetDecimal()}°C");

                        if (main.TryGetProperty("temp_max", out var maxTemp) && main.TryGetProperty("temp_min", out var minTemp)&&
                            maxTemp.GetDecimal() != minTemp.GetDecimal())
                            Logger.LogSuccess($"Max/Min Temp: {maxTemp.GetDecimal()}°C/{minTemp.GetDecimal()}°C");
                        else
                            Logger.LogWarning($"Max/Min temperature data not available.");

                        if (main.TryGetProperty("humidity", out var humidity))
                            Logger.LogSuccess($"Humidity: {humidity.GetDecimal()}%");
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
