namespace Toolkit.CLI
{
    internal static class Logger
    {
        private const string InfoTag = "[INFO] ";
        private const string ErrorTag = "[ERROR] ";
        private const string WarningTag = "[WARNING] ";
        private const string SuccessTag = "[SUCCESS] ";

        public static void LogInformation(string message)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write(InfoTag);
            Console.ResetColor();
            Console.WriteLine(message);
        }

        public static void LogError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(ErrorTag);
            Console.ResetColor();
            Console.WriteLine(message);
        }

        public static void LogWarning(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(WarningTag);
            Console.ResetColor();
            Console.WriteLine(message);
        }

        public static void LogSuccess(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(SuccessTag);
            Console.ResetColor();
            Console.WriteLine(message);
        }

        public static async Task DisplayLoadingAnimation(CancellationToken token)
        {
            var animation = new[] { "|", "/", "-", "\\" };
            int counter = 0;

            while (!token.IsCancellationRequested)
            {
                Console.Write(animation[counter++ % animation.Length]);
                await Task.Delay(100);
                Console.Write("\b");
            }

        }
    }
}
