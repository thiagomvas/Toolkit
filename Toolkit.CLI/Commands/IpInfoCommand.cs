using System.CommandLine;
using System.CommandLine.Invocation;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Toolkit.CLI.Commands
{
    public class IpInfoCommand : BaseCommand
    {
        private static readonly HttpClient httpClient = new HttpClient();

        public IpInfoCommand() : base("ipinfo", "Retrieve IP information")
        {
        }

        public override void Setup(RootCommand root)
        {
            var ipArgument = new Argument<string?>("ip", "The IP address to get information for.")
            {
                Arity = ArgumentArity.ZeroOrOne
            };

            AddArgument(ipArgument);

            this.SetHandler(ExecuteAsync, ipArgument);
            root.AddCommand(this);
        }

        private async Task ExecuteAsync(string? ip)
        {
            string requestUrl = string.IsNullOrEmpty(ip)
                ? "https://ipinfo.io/json"
                : $"https://ipinfo.io/{ip}/json";

            try
            {
                var response = await httpClient.GetStringAsync(requestUrl);
                var data = JsonDocument.Parse(response).RootElement;

                // IP and Hostname
                if (data.TryGetProperty("ip", out var ipProp))
                    Logger.LogInformation($"IP: {ipProp.GetString()}");

                if (data.TryGetProperty("hostname", out var hostnameProp))
                    Logger.LogInformation($"Hostname: {hostnameProp.GetString()}");

                // Location
                if (data.TryGetProperty("city", out var cityProp))
                    Logger.LogInformation($"City: {cityProp.GetString()}");

                if (data.TryGetProperty("region", out var regionProp))
                    Logger.LogInformation($"Region: {regionProp.GetString()}");

                if (data.TryGetProperty("country", out var countryProp))
                    Logger.LogInformation($"Country: {countryProp.GetString()}");

                // Coordinates
                if (data.TryGetProperty("loc", out var locProp))
                {
                    var loc = locProp.GetString()?.Split(',');
                    if (loc?.Length == 2)
                    {
                        Logger.LogInformation($"Latitude: {loc[0]}, Longitude: {loc[1]}");
                    }
                }

                // Additional Info
                if (data.TryGetProperty("postal", out var postalProp))
                    Logger.LogInformation($"Postal: {postalProp.GetString()}");

                if (data.TryGetProperty("timezone", out var timezoneProp))
                    Logger.LogInformation($"Timezone: {timezoneProp.GetString()}");

                if (data.TryGetProperty("org", out var orgProp))
                    Logger.LogInformation($"Organization: {orgProp.GetString()}");

            }
            catch (HttpRequestException ex)
            {
                Logger.LogError($"Error fetching IP info: {ex.Message}");
            }
            catch (JsonException ex)
            {
                Logger.LogError($"Error parsing IP info: {ex.Message}");
            }
        }



        private void PrintIfNotNull(string text, string val)
        {
            if (string.IsNullOrWhiteSpace(val))
            {
                Logger.LogWarning($"{text}: Not Available");
            }
            else
            {
                Logger.LogInformation($"{text}: {val}");
            }
        }
    }
}
