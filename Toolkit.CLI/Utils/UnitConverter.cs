namespace Toolkit.CLI.Utils;

internal static class UnitConverter
{
    // Conversion factors for length units to meters
    private static readonly Dictionary<LengthUnit, double> lengthConversionFactors = new Dictionary<LengthUnit, double>
    {
        { LengthUnit.Meter, 1.0 },
        { LengthUnit.Kilometer, 1000.0 },
        { LengthUnit.Decameter, 10.0 },
        { LengthUnit.Hectometer, 100.0 },
        { LengthUnit.Centimeter, 0.01 },
        { LengthUnit.Millimeter, 0.001 },
        { LengthUnit.Micrometer, 1e-6 },
        { LengthUnit.Nanometer, 1e-9 },
        { LengthUnit.Foot, 0.3048 },
        { LengthUnit.Yard, 0.9144 },
        { LengthUnit.Mile, 1609.34 },
        { LengthUnit.Inch, 0.0254 }
    };

    // Conversion factors for mass units to grams
    private static readonly Dictionary<MassUnit, double> massConversionFactors = new Dictionary<MassUnit, double>
    {
        { MassUnit.Gram, 1.0 },
        { MassUnit.Kilogram, 1000.0 },
        { MassUnit.Milligram, 0.001 },
        { MassUnit.Tonne, 1e6 },
        { MassUnit.Pound, 453.592 },
        { MassUnit.Ounce, 28.3495 }
    };


    // Conversion factors for volume units to liters
    private static readonly Dictionary<VolumeUnit, double> volumeConversionFactors = new Dictionary<VolumeUnit, double>
    {
        { VolumeUnit.Liter, 1.0 },
        { VolumeUnit.Milliliter, 0.001 },
        { VolumeUnit.CubicMeter, 1000.0 },
        { VolumeUnit.CubicFoot, 28.3168 },
        { VolumeUnit.CubicInch, 0.0163871 },
        { VolumeUnit.CubicCentimeter, 0.001 },
        { VolumeUnit.CubicMillimeter, 1e-6 },
        { VolumeUnit.Gallon, 3.78541 },
        { VolumeUnit.Quart, 0.946353 },
        { VolumeUnit.Pint, 0.473176 },
        { VolumeUnit.Cup, 0.236588 },
        { VolumeUnit.FluidOunce, 0.0295735 },
        { VolumeUnit.Tablespoon, 0.0147868 },
        { VolumeUnit.Teaspoon, 0.00492892 }
    };


    // Conversion function for length
    public static double ConvertLength(double value, LengthUnit fromUnit, LengthUnit toUnit)
    {
        double valueInMeters = value * lengthConversionFactors[fromUnit]; // Convert from original unit to meters
        return valueInMeters / lengthConversionFactors[toUnit];           // Convert from meters to target unit
    }

    // Conversion function for mass
    public static double ConvertMass(double value, MassUnit fromUnit, MassUnit toUnit)
    {
        double valueInGrams = value * massConversionFactors[fromUnit]; // Convert from original unit to grams
        return valueInGrams / massConversionFactors[toUnit];           // Convert from grams to target unit
    }

    // Conversion function for temperature
    public static double ConvertTemperature(double value, TemperatureUnit fromUnit, TemperatureUnit toUnit)
    {
        if (fromUnit == toUnit)
        {
            return value;
        }

        double celsiusValue = fromUnit switch
        {
            TemperatureUnit.Celsius => value,
            TemperatureUnit.Fahrenheit => (value - 32) * 5 / 9,
            TemperatureUnit.Kelvin => value - 273.15,
            _ => throw new ArgumentException("Invalid temperature unit", nameof(fromUnit))
        };

        return toUnit switch
        {
            TemperatureUnit.Celsius => celsiusValue,
            TemperatureUnit.Fahrenheit => celsiusValue * 9 / 5 + 32,
            TemperatureUnit.Kelvin => celsiusValue + 273.15,
            _ => throw new ArgumentException("Invalid temperature unit", nameof(toUnit))
        };
    }

    // Conversion function for volume
    public static double ConvertVolume(double value, VolumeUnit fromUnit, VolumeUnit toUnit)
    {
        double valueInLiters = value * volumeConversionFactors[fromUnit]; // Convert from original unit to liters
        return valueInLiters / volumeConversionFactors[toUnit];           // Convert from liters to target unit
    }
}

public enum LengthUnit
{
    Meter,
    Kilometer,
    Decameter,
    Hectometer,
    Centimeter,
    Millimeter,
    Micrometer,
    Nanometer,
    Foot,
    Yard,
    Mile,
    Inch
}

public enum MassUnit
{
    Gram,
    Kilogram,
    Milligram,
    Tonne,
    Pound,
    Ounce
}

public enum TemperatureUnit
{
    Celsius,
    Fahrenheit,
    Kelvin
}

public enum VolumeUnit
{
    Liter,
    Milliliter,
    CubicMeter,
    CubicFoot,
    CubicInch,
    CubicCentimeter,
    CubicMillimeter,
    Gallon,
    Quart,
    Pint,
    Cup,
    FluidOunce,
    Tablespoon,
    Teaspoon
}