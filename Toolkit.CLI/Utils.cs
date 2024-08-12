namespace Toolkit.CLI
{
    internal static class Utils
    {
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
