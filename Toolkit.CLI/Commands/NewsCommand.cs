namespace Toolkit.CLI.Commands
{
    using System.CommandLine;
    using System.IO;
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading.Tasks;

    namespace Toolkit.CLI.Commands
    {
        internal class NewsCommands : BaseCommand
        {
            private const string ConfigFilePath = "config.json";
            private const string ApiKeyWebsite = "https://newsapi.org/";

            public NewsCommands() : base("news", "Fetch the latest news headlines from various sources.")
            {
            }

            public override void Setup(RootCommand root)
            {
                AddCommand(new LatestHeadlinesCommand());
                AddCommand(new ApiKeyInfoCommand());
                root.AddCommand(this);
            }
            internal class LatestHeadlinesCommand : Command
            {
                public LatestHeadlinesCommand() : base("latest", "Get the latest headlines from specified categories or sources.")
                {
                    var categoryOption = new Option<string>("--category", "The news category (e.g., technology, sports).")
                    {
                        IsRequired = false
                    };
                    var sourceOption = new Option<string>("--source", "The news source (e.g., BBC News, CNN).")
                    {
                        IsRequired = false
                    };

                    AddOption(categoryOption);
                    AddOption(sourceOption);
                    this.SetHandler(ExecuteAsync, categoryOption, sourceOption);
                }

                private async Task ExecuteAsync(string? category, string? source)
                {
                    try
                    {
                        var apiKey = GetApiKey();
                        if (string.IsNullOrWhiteSpace(apiKey))
                        {
                            Logger.LogError("API key for news is not set. Use 'news api-key-info' to get information on how to obtain one.");
                            Logger.LogInformation($"Obtain an API key from: {ApiKeyWebsite}");
                            return;
                        }

                        var apiUrl = $"https://newsapi.org/v2/top-headlines?apiKey={apiKey}" +
                            $"{(!string.IsNullOrWhiteSpace(category) ? $"&category={Uri.EscapeDataString(category)}" : "")}" +
                            $"{(!string.IsNullOrWhiteSpace(source) ? $"&sources={Uri.EscapeDataString(source)}" : "")}";

                        Logger.LogInformation($"Fetching news from: {apiUrl}");

                        using var httpClient = new HttpClient();
                        var response = await httpClient.GetAsync(apiUrl);

                        if (!response.IsSuccessStatusCode)
                        {
                            Logger.LogError($"Failed to fetch news data: {response.StatusCode} ({response.ReasonPhrase})");
                            return;
                        }

                        var responseContent = await response.Content.ReadAsStringAsync();
                        var data = JsonDocument.Parse(responseContent).RootElement;

                        if (data.TryGetProperty("articles", out var articles) && articles.GetArrayLength() > 0)
                        {
                            foreach (var article in articles.EnumerateArray())
                            {
                                var title = article.GetProperty("title").GetString();
                                var description = article.GetProperty("description").GetString();

                                Logger.LogInformation($"Title: {title}");
                                Logger.LogInformation($"Description: {description}");
                                Logger.LogInformation("----------------------------");
                            }
                        }
                        else
                        {
                            Logger.LogWarning("No news articles found.");
                        }
                    }
                    catch (HttpRequestException ex)
                    {
                        Logger.LogError($"Failed to fetch news data: {ex.Message}");
                    }
                    catch (JsonException ex)
                    {
                        Logger.LogError($"Error parsing response: {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"An error occurred: {ex.Message}");
                    }
                }

                private static string? GetApiKey()
                {
                    if (!File.Exists(ConfigFilePath))
                    {
                        return null;
                    }

                    var json = File.ReadAllText(ConfigFilePath);
                    var config = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
                    return config.ContainsKey("newsapikey") ? config["newsapikey"] : null;
                }
            }

            internal class ApiKeyInfoCommand : Command
            {
                public ApiKeyInfoCommand() : base("api-key-info", "Get information on how to obtain an API key for news data.")
                {
                }

                private void Execute()
                {
                    Logger.LogInformation($"To obtain a news API key, visit: {ApiKeyWebsite}");
                }
            }
        }
    }

}
