using System.CommandLine;
using System.Net.Http;
using System.Threading.Tasks;

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
                Console.WriteLine($"Sending GET request to {url}...");

                using var httpClient = new HttpClient();
                var response = await httpClient.GetStringAsync(url);

                Console.WriteLine("Response received:");
                Console.WriteLine(response);
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
                Console.WriteLine($"Sending POST request to {url} with content: {content}");

                using var httpClient = new HttpClient();
                var response = await httpClient.PostAsync(url, new StringContent(content));

                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Response received:");
                Console.WriteLine(responseBody);
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
                Console.WriteLine($"Sending PUT request to {url} with content: {content}");

                using var httpClient = new HttpClient();
                var response = await httpClient.PutAsync(url, new StringContent(content));

                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Response received:");
                Console.WriteLine(responseBody);
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
                Console.WriteLine($"Sending DELETE request to {url}...");

                using var httpClient = new HttpClient();
                var response = await httpClient.DeleteAsync(url);

                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Response received:");
                Console.WriteLine(responseBody);
            }
        }
    }
}
