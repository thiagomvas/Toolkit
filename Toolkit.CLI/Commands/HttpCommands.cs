using System.CommandLine;

namespace Toolkit.CLI.Commands
{
    internal class HttpCommands : BaseCommand
    {
        public HttpCommands() : base("http", "Perform HTTP requests using various methods.")
        {
        }

        public override void Setup(RootCommand root)
        {
            AddCommand(new GetCommand());
            AddCommand(new PostCommand());
            AddCommand(new PutCommand());
            AddCommand(new DeleteCommand());
            root.AddCommand(this);
        }

        public class GetCommand : Command
        {
            public GetCommand() : base("get", "Perform an HTTP GET request to a specified URL.")
            {
                var urlArgument = new Argument<string>("url", "The URL to send the GET request to.");
                AddArgument(urlArgument);
                this.SetHandler(ExecuteAsync, urlArgument);
            }

            private async Task ExecuteAsync(string url)
            {
                using var httpClient = new HttpClient();
                using var cts = new CancellationTokenSource();

                Logger.LogInformation($"Sending GET request to: {url}");
                var loadingTask = Logger.DisplayLoadingAnimation(cts.Token);

                try
                {
                    var response = await httpClient.GetStringAsync(url);
                    cts.Cancel();
                    await loadingTask;

                    Logger.LogInformation("Response received:\n");
                    Console.WriteLine(response);
                }
                catch (Exception ex)
                {
                    cts.Cancel();
                    await loadingTask;
                    Logger.LogError($"Request failed: {ex.Message}");
                }
            }
        }

        public class PostCommand : Command
        {
            public PostCommand() : base("post", "Perform an HTTP POST request to a specified URL with optional content.")
            {
                var urlArgument = new Argument<string>("url", "The URL to send the POST request to.");
                var contentArgument = new Argument<string>("content", "The content to include in the POST request.");
                AddArgument(urlArgument);
                AddArgument(contentArgument);
                this.SetHandler(ExecuteAsync, urlArgument, contentArgument);
            }

            private async Task ExecuteAsync(string url, string content)
            {
                using var httpClient = new HttpClient();
                using var cts = new CancellationTokenSource();

                Logger.LogInformation($"Sending POST request to: {url} with content: {content}");
                var loadingTask = Logger.DisplayLoadingAnimation(cts.Token);

                try
                {
                    var response = await httpClient.PostAsync(url, new StringContent(content));
                    cts.Cancel();
                    await loadingTask;

                    var responseBody = await response.Content.ReadAsStringAsync();
                    Logger.LogInformation("Response received:\n");
                    Console.WriteLine(responseBody);
                }
                catch (Exception ex)
                {
                    cts.Cancel();
                    await loadingTask;
                    Logger.LogError($"Request failed: {ex.Message}");
                }
            }
        }

        public class PutCommand : Command
        {
            public PutCommand() : base("put", "Perform an HTTP PUT request to a specified URL with optional content.")
            {
                var urlArgument = new Argument<string>("url", "The URL to send the PUT request to.");
                var contentArgument = new Argument<string>("content", "The content to include in the PUT request.");
                AddArgument(urlArgument);
                AddArgument(contentArgument);
                this.SetHandler(ExecuteAsync, urlArgument, contentArgument);
            }

            private async Task ExecuteAsync(string url, string content)
            {
                using var httpClient = new HttpClient();
                using var cts = new CancellationTokenSource();

                Logger.LogInformation($"Sending PUT request to: {url} with content: {content}");
                var loadingTask = Logger.DisplayLoadingAnimation(cts.Token);

                try
                {
                    var response = await httpClient.PutAsync(url, new StringContent(content));
                    cts.Cancel();
                    await loadingTask;

                    var responseBody = await response.Content.ReadAsStringAsync();
                    Logger.LogInformation("Response received:\n");
                    Console.WriteLine(responseBody);
                }
                catch (Exception ex)
                {
                    cts.Cancel();
                    await loadingTask;
                    Logger.LogError($"Request failed: {ex.Message}");
                }
            }
        }

        public class DeleteCommand : Command
        {
            public DeleteCommand() : base("delete", "Perform an HTTP DELETE request to a specified URL.")
            {
                var urlArgument = new Argument<string>("url", "The URL to send the DELETE request to.");
                AddArgument(urlArgument);
                this.SetHandler(ExecuteAsync, urlArgument);
            }

            private async Task ExecuteAsync(string url)
            {
                using var httpClient = new HttpClient();
                using var cts = new CancellationTokenSource();

                Logger.LogInformation($"Sending DELETE request to: {url}");
                var loadingTask = Logger.DisplayLoadingAnimation(cts.Token);

                try
                {
                    var response = await httpClient.DeleteAsync(url);
                    cts.Cancel();
                    await loadingTask;

                    var responseBody = await response.Content.ReadAsStringAsync();
                    Logger.LogInformation("Response received:\n");
                    Console.WriteLine(responseBody);
                }
                catch (Exception ex)
                {
                    cts.Cancel();
                    await loadingTask;
                    Logger.LogError($"Request failed: {ex.Message}");
                }
            }
        }
    }
}
