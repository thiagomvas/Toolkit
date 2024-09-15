namespace Toolkit.CLI.Utils;

public static class UnitParser
{
    // Mapping of string to LengthUnit
    private static readonly Dictionary<string, LengthUnit> lengthUnitMap = new Dictionary<string, LengthUnit>(StringComparer.OrdinalIgnoreCase)
    {
        { "m", LengthUnit.Meter },
        { "meter", LengthUnit.Meter },
        { "km", LengthUnit.Kilometer },
        { "kilometer", LengthUnit.Kilometer },
        { "dam", LengthUnit.Decameter },
        { "decameter", LengthUnit.Decameter },
        { "hm", LengthUnit.Hectometer },
        { "hectometer", LengthUnit.Hectometer },
        { "cm", LengthUnit.Centimeter },
        { "centimeter", LengthUnit.Centimeter },
        { "mm", LengthUnit.Millimeter },
        { "millimeter", LengthUnit.Millimeter },
        { "µm", LengthUnit.Micrometer },
        { "micrometer", LengthUnit.Micrometer },
        { "nm", LengthUnit.Nanometer },
        { "nanometer", LengthUnit.Nanometer },
        { "ft", LengthUnit.Foot },
        { "foot", LengthUnit.Foot },
        { "yard", LengthUnit.Yard },
        { "yd", LengthUnit.Yard },
        { "mile", LengthUnit.Mile },
        { "in", LengthUnit.Inch },
        { "inch", LengthUnit.Inch }
    };

    // Mapping of string to MassUnit
    private static readonly Dictionary<string, MassUnit> massUnitMap = new Dictionary<string, MassUnit>(StringComparer.OrdinalIgnoreCase)
    {
        { "g", MassUnit.Gram },
        { "gram", MassUnit.Gram },
        { "kg", MassUnit.Kilogram },
        { "kilogram", MassUnit.Kilogram },
        { "mg", MassUnit.Milligram },
        { "milligram", MassUnit.Milligram },
        { "tonne", MassUnit.Tonne },
        { "lb", MassUnit.Pound },
        { "pound", MassUnit.Pound },
        { "oz", MassUnit.Ounce },
        { "ounce", MassUnit.Ounce }
    };

    // Mapping of string to TemperatureUnit
    private static readonly Dictionary<string, TemperatureUnit> temperatureUnitMap = new Dictionary<string, TemperatureUnit>(StringComparer.OrdinalIgnoreCase)
    {
        { "c", TemperatureUnit.Celsius },
        { "celsius", TemperatureUnit.Celsius },
        { "f", TemperatureUnit.Fahrenheit },
        { "fahrenheit", TemperatureUnit.Fahrenheit },
        { "k", TemperatureUnit.Kelvin },
        { "kelvin", TemperatureUnit.Kelvin }
    };

    // Mapping of string to VolumeUnit
    private static readonly Dictionary<string, VolumeUnit> volumeUnitMap = new Dictionary<string, VolumeUnit>(StringComparer.OrdinalIgnoreCase)
    {
        { "l", VolumeUnit.Liter },
        { "liter", VolumeUnit.Liter },
        { "ml", VolumeUnit.Milliliter },
        { "milliliter", VolumeUnit.Milliliter },
        { "cm3", VolumeUnit.CubicCentimeter },
        { "cubiccentimeter", VolumeUnit.CubicCentimeter },
        { "m3", VolumeUnit.CubicMeter },
        { "cubicmeter", VolumeUnit.CubicMeter },
        { "ft3", VolumeUnit.CubicFoot },
        { "cubicfoot", VolumeUnit.CubicFoot },
        { "in3", VolumeUnit.CubicInch },
        { "cubicinch", VolumeUnit.CubicInch }
    };

    public static bool TryParseLengthUnit(string input, out LengthUnit unit)
    {
        return lengthUnitMap.TryGetValue(input.ToLower(), out unit);
    }

    public static bool TryParseMassUnit(string input, out MassUnit unit)
    {
        return massUnitMap.TryGetValue(input.ToLower(), out unit);
    }

    public static bool TryParseTemperatureUnit(string input, out TemperatureUnit unit)
    {
        return temperatureUnitMap.TryGetValue(input.ToLower(), out unit);
    }

    public static bool TryParseVolumeUnit(string input, out VolumeUnit unit)
    {
        return volumeUnitMap.TryGetValue(input.ToLower(), out unit);
    }


}

