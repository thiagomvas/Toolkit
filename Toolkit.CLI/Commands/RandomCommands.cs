using System.CommandLine;

namespace Toolkit.CLI.Commands
{
    internal class RandomCommands : BaseCommand
    {
        public RandomCommands() : base("random", "Generate random values")
        {
        }

        public override void Setup(RootCommand root)
        {
            AddCommand(new DiceCommand());
            AddCommand(new IntCommand());
            AddCommand(new GuidCommand());
            AddCommand(new StringCommand());

            root.AddCommand(this);
        }

        internal class DiceCommand : Command
        {
            public DiceCommand() : base("dice", "Roll dice of multiple ranges. Use AdN where A is the number of dice and N is the upper bound of the dice.")
            {
                var diceArg = new Argument<string>("dice", () => "1d6", "The dice to roll (e.g., 1d6 = one 6-sided die, 2d20 = two 20-sided dice, etc.)");
                AddArgument(diceArg);
                this.SetHandler(Execute, diceArg);
            }

            public void Execute(string die)
            {
                try
                {
                    var parts = die.Trim().ToLower().Split('d');
                    if (parts.Length != 2 || !int.TryParse(parts[0], out int amount) || !int.TryParse(parts[1], out int bound))
                    {
                        Logger.LogError("Invalid dice format. Use the format AdN (e.g., 1d6, 2d20).");
                        return;
                    }

                    Logger.LogInformation($"Rolling {amount}d{bound}...");

                    var random = new Random();
                    var results = new List<int>();
                    for (int i = 0; i < amount; i++)
                    {
                        int roll = random.Next(1, bound + 1);
                        results.Add(roll);
                    }

                    Logger.LogSuccess($"Results: {string.Join(", ", results)} (Total: {results.Sum()})");
                }
                catch (Exception ex)
                {
                    Logger.LogError($"An error occurred while rolling the dice: {ex.Message}");
                }
            }
        }

        public class IntCommand : Command
        {
            public IntCommand() : base("int", "Generate a random integer")
            {
                var minOption = new Option<int>("--min", () => 0, "The lower bound of the random number returned (inclusive)");
                var maxOption = new Option<int>("--max", () => 100, "The upper bound of the random number returned (inclusive). Default is 100");
                var amountOption = new Option<int>("--amount", () => 1, "The number of random integers to generate");

                AddOption(minOption);
                AddOption(maxOption);
                AddOption(amountOption);

                this.SetHandler(Execute, minOption, maxOption, amountOption);
            }

            public void Execute(int min, int max, int amount)
            {
                var random = new Random();
                for (int i = 0; i < amount; i++)
                {
                    Console.WriteLine(random.Next(min, max + 1));
                }
            }
        }

        public class GuidCommand : Command
        {
            public GuidCommand() : base("guid", "Generate a random GUID")
            {
                var amountOption = new Option<int>("--amount", () => 1, "The number of GUIDs to generate");
                AddOption(amountOption);
                this.SetHandler(Execute, amountOption);
            }

            public void Execute(int amount)
            {
                for (int i = 0; i < amount; i++)
                {
                    Console.WriteLine(Guid.NewGuid());
                }
            }
        }

        public class StringCommand : Command
        {
            public StringCommand() : base("string", "Generate a random string of specified length")
            {
                var lengthOption = new Option<int>("--length", () => 10, "The length of the random string");
                var amountOption = new Option<int>("--amount", () => 1, "The number of random strings to generate");
                AddOption(lengthOption);
                AddOption(amountOption);
                this.SetHandler(Execute, lengthOption, amountOption);
            }

            public void Execute(int length, int amount)
            {
                var random = new Random();
                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

                for (int i = 0; i < amount; i++)
                {
                    var result = new string(Enumerable.Repeat(chars, length)
                      .Select(s => s[random.Next(s.Length)]).ToArray());
                    Console.WriteLine(result);
                }
            }
        }
    }
}
