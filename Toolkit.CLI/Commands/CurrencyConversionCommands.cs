using System.CommandLine;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Toolkit.CLI.Commands
{
    internal class CurrencyConversionCommands : BaseCommand
    {
        private const string ConfigFilePath = "config.json";
        private const string ApiKeyWebsite = "https://www.exchangerate-api.com/";

        public CurrencyConversionCommands() : base("currency", "Convert amounts between different currencies using the ExchangeRate API.")
        {
        }

        public override void Setup(RootCommand root)
        {
            AddCommand(new ConvertCommand());
            AddCommand(new ApiKeyInfoCommand());
            root.AddCommand(this);
        }

        internal class ConvertCommand : Command
        {
            public ConvertCommand() : base("convert", "Convert an amount from one currency to another.")
            {
                var fromOption = new Option<string>("--from", "The source currency code (e.g., USD).")
                {
                    IsRequired = true
                };
                var toOption = new Option<string>("--to", "The target currency code (e.g., EUR).")
                {
                    IsRequired = true
                };
                var amountOption = new Option<decimal>("--amount", "The amount to convert.")
                {
                    IsRequired = true
                };

                AddOption(fromOption);
                AddOption(toOption);
                AddOption(amountOption);

                this.SetHandler(ExecuteAsync, fromOption, toOption, amountOption);
            }

            private async Task ExecuteAsync(string from, string to, decimal amount)
            {
                try
                {
                    var apiKey = LoadApiKey("currencyapikey");
                    if (string.IsNullOrWhiteSpace(apiKey))
                    {
                        Logger.LogError("API key for currency conversion is not set. Use 'config set-key' to set it.");
                        Logger.LogInformation($"Obtain an API key from: {ApiKeyWebsite}");
                        return;
                    }

                    using var httpClient = new HttpClient();
                    var apiUrl = $"https://api.exchangerate-api.com/v4/latest/{from.ToUpper()}"; // Example URL, adjust as needed

                    var response = await httpClient.GetStringAsync(apiUrl);
                    var data = JsonDocument.Parse(response).RootElement;

                    if (data.TryGetProperty("rates", out var rates) &&
                        rates.TryGetProperty(to.ToUpper(), out var rateToken) &&
                        rateToken.TryGetDecimal(out var rate))
                    {
                        var convertedAmount = amount * rate;
                        Logger.LogInformation($"Conversion rate from {from.ToUpper()} to {to.ToUpper()}: {rate}");
                        Logger.LogSuccess($"{amount} {from.ToUpper()} = {convertedAmount} {to.ToUpper()}");
                    }
                    else
                    {
                        Logger.LogError($"Currency code {to} is invalid or not available.");
                    }
                }
                catch (HttpRequestException ex)
                {
                    Logger.LogError($"Failed to fetch exchange rates: {ex.Message}");
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
            public ApiKeyInfoCommand() : base("api-key-info", "Get information on how to obtain an API key for currency conversion.")
            {
                this.SetHandler(Execute);
            }

            private void Execute()
            {
                Logger.LogInformation($"To obtain a currency conversion API key, visit: {ApiKeyWebsite}");
            }
        }
    }
}
