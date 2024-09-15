using System.CommandLine;
using System.Security.Cryptography.X509Certificates;
using Toolkit.CLI.Utils;

namespace Toolkit.CLI.Commands;

internal class ConvertCommand : BaseCommand
{
    public ConvertCommand() : base("convert", "Convert between different units, including metric and imperial units")
    {
    }

    public override void Setup(RootCommand root)
    {
        var fromOption = new Option<string>(["--from", "-f"], "The source currency code (e.g., USD).")
        {
            IsRequired = true
        };
        var toOption = new Option<string>(["--to", "-t"], "The target currency code (e.g., EUR).")
        {
            IsRequired = true
        };
        var amountArg = new Argument<double>("amount", "The amount to convert.");

        AddArgument(amountArg);
        AddOption(fromOption);
        AddOption(toOption);

        this.SetHandler(Execute, fromOption, toOption, amountArg);
        root.AddCommand(this);
    }

    public void Execute(string from, string to, double amount)
    {
        double result = 0;
        if(UnitParser.TryParseLengthUnit(from, out var fromUnit) && UnitParser.TryParseLengthUnit(to, out var toUnit))
        {
            result = UnitConverter.ConvertLength(amount, fromUnit, toUnit);
            LogResult(amount, fromUnit.ToString(), result, toUnit.ToString());
        }
        else if(UnitParser.TryParseMassUnit(from, out var fromMassUnit) && UnitParser.TryParseMassUnit(to, out var toMassUnit))
        {
            result = UnitConverter.ConvertMass(amount, fromMassUnit, toMassUnit);
            LogResult(amount, fromMassUnit.ToString(), result, toMassUnit.ToString());
        }
        else if (UnitParser.TryParseTemperatureUnit(from, out var fromTemperatureUnit) && UnitParser.TryParseTemperatureUnit(to, out var toTemperatureUnit))
        {
            result = UnitConverter.ConvertTemperature(amount, fromTemperatureUnit, toTemperatureUnit);
            LogResult(amount, fromTemperatureUnit.ToString(), result, toTemperatureUnit.ToString());
        }
        else if (UnitParser.TryParseVolumeUnit(from, out var fromVolumeUnit) && UnitParser.TryParseVolumeUnit(to, out var toVolumeUnit))
        {
            result = UnitConverter.ConvertVolume(amount, fromVolumeUnit, toVolumeUnit);
            LogResult(amount, fromVolumeUnit.ToString(), result, toVolumeUnit.ToString());
        }
        else
        {
            Logger.LogError($"Conversion between {from} and {to} is not supported.");
        }
    }

    private void LogResult(double amount, string fromUnit, double result, string toUnit)
    {
        Logger.LogInformation($"{amount} {fromUnit}{(amount == 1 || fromUnit[^1] == 's' ? "" : "s")} is equal to {result:0.#####} {toUnit}{(amount == 1 || fromUnit[^1] == 's' ? "" : "s")}");
    }
}
