using System.CommandLine;
using System.Management;
using System.Threading.Tasks;

namespace Toolkit.CLI.Commands
{
    internal class SystemInfoCommand : BaseCommand
    {
        public SystemInfoCommand() : base("systeminfo", "Display detailed system information.")
        {
        }

        public override void Setup(RootCommand root)
        {
            AddCommand(new SystemInfoDisplayCommand());
            root.AddCommand(this);
        }

        public class SystemInfoDisplayCommand : Command
        {
            public SystemInfoDisplayCommand() : base("display", "Show detailed system information such as CPU, RAM, and GPU details.")
            {
                this.SetHandler(ExecuteAsync);
            }

            private Task ExecuteAsync()
            {
                DisplayProcessorInfo();
                DisplayRamInfo();
                DisplayGpuInfo();
                DisplayBiosInfo();

                return Task.CompletedTask;
            }

            private void DisplayProcessorInfo()
            {
                try
                {
                    using var searcher = new ManagementObjectSearcher("select * from Win32_Processor");
                    foreach (var obj in searcher.Get())
                    {
                        Logger.LogInformation("Processor Information:");
                        PrintIfNotNull("Model", obj["Name"]?.ToString());
                        PrintIfNotNull("Number of Cores", obj["NumberOfCores"]?.ToString());
                        PrintIfNotNull("Clock Speed", obj["MaxClockSpeed"]?.ToString() + " MHz");
                        PrintIfNotNull("Architecture", GetArchitectureDescription(Convert.ToUInt16(obj["Architecture"])));
                        PrintIfNotNull("L2 Cache Size", obj["L2CacheSize"]?.ToString() + " KB");
                        PrintIfNotNull("L3 Cache Size", obj["L3CacheSize"]?.ToString() + " KB");
                        Logger.LogInformation(""); // Empty line for separation
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Failed to retrieve processor information: {ex.Message}");
                }
            }

            private void DisplayRamInfo()
            {
                try
                {
                    using var searcher = new ManagementObjectSearcher("select * from Win32_PhysicalMemory");
                    foreach (var obj in searcher.Get())
                    {
                        Logger.LogInformation("RAM Information:");
                        PrintIfNotNull("Capacity", FormatBytes(ConvertToUInt64(obj["Capacity"])));
                        PrintIfNotNull("Speed", obj["Speed"]?.ToString() + " MHz");
                        PrintIfNotNull("Manufacturer", obj["Manufacturer"]?.ToString());
                        PrintIfNotNull("Part Number", obj["PartNumber"]?.ToString());
                        PrintIfNotNull("Form Factor", GetFormFactorDescription(Convert.ToUInt32(obj["FormFactor"])));
                        PrintIfNotNull("Memory Type", GetMemoryTypeDescription(Convert.ToUInt32(obj["MemoryType"])));
                        Logger.LogInformation(""); // Empty line for separation
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Failed to retrieve RAM information: {ex.Message}");
                }
            }

            private void DisplayGpuInfo()
            {
                try
                {
                    using var searcher = new ManagementObjectSearcher("select * from Win32_VideoController");
                    foreach (var obj in searcher.Get())
                    {
                        Logger.LogInformation("GPU Information:");
                        PrintIfNotNull("Name", obj["Name"]?.ToString());
                        PrintIfNotNull("Adapter RAM", FormatBytes(ConvertToUInt64(obj["AdapterRAM"])));
                        PrintIfNotNull("Driver Version", obj["DriverVersion"]?.ToString());
                        PrintIfNotNull("Video Processor", obj["VideoProcessor"]?.ToString());
                        PrintIfNotNull("Video Mode Description", obj["VideoModeDescription"]?.ToString());
                        Logger.LogInformation(""); // Empty line for separation
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Failed to retrieve GPU information: {ex.Message}");
                }
            }

            private void DisplayBiosInfo()
            {
                try
                {
                    using var searcher = new ManagementObjectSearcher("select * from Win32_BIOS");
                    foreach (var obj in searcher.Get())
                    {
                        Logger.LogInformation("BIOS Information:");
                        PrintIfNotNull("Manufacturer", obj["Manufacturer"]?.ToString());
                        PrintIfNotNull("Version", obj["Version"]?.ToString());
                        PrintIfNotNull("Release Date", ManagementDateTimeConverter.ToDateTime(obj["ReleaseDate"]?.ToString()).ToString("yyyy-MM-dd HH:mm:ss"));
                        PrintIfNotNull("Serial Number", obj["SerialNumber"]?.ToString());
                        Logger.LogInformation(""); // Empty line for separation
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Failed to retrieve BIOS information: {ex.Message}");
                }
            }

            private ulong ConvertToUInt64(object value)
            {
                if (value is uint uintValue)
                {
                    return uintValue;
                }
                else if (value is ulong ulongValue)
                {
                    return ulongValue;
                }
                else
                {
                    throw new InvalidCastException($"Cannot convert {value.GetType()} to UInt64");
                }
            }

            private string FormatBytes(ulong bytes)
            {
                string[] sizes = { "B", "KB", "MB", "GB", "TB" };
                double len = bytes;
                int order = 0;
                while (len >= 1024 && order < sizes.Length - 1)
                {
                    order++;
                    len = len / 1024;
                }
                return $"{len:0.00} {sizes[order]}";
            }

            private void PrintIfNotNull(string text, string val)
            {
                if (string.IsNullOrWhiteSpace(val))
                {
                    Logger.LogWarning($"{text}: Not Available");
                }
                else
                {
                    Logger.LogInformation($"{text}: {val}");
                }
            }

            private string GetArchitectureDescription(ushort architecture)
            {
                return architecture switch
                {
                    0 => "x86",
                    1 => "MIPS",
                    2 => "Alpha",
                    3 => "PowerPC",
                    5 => "ARM",
                    6 => "Itanium",
                    9 => "x64",
                    _ => "Unknown"
                };
            }

            private string GetFormFactorDescription(uint formFactor)
            {
                return formFactor switch
                {
                    0 => "Other",
                    1 => "Unknown",
                    2 => "Desktop",
                    3 => "Laptop",
                    4 => "Notebook",
                    5 => "Workstation",
                    6 => "Server",
                    7 => "Handheld",
                    8 => "Docking Station",
                    9 => "All-in-One",
                    10 => "Sub Notebook",
                    11 => "Slate",
                    12 => "Convertible",
                    13 => "Detachables",
                    14 => "Mini PC",
                    15 => "Stick PC",
                    _ => "Unknown"
                };
            }

            private string GetMemoryTypeDescription(uint memoryType)
            {
                string outValue = string.Empty;

                switch (memoryType)
                {
                    case 0x0: outValue = "Unknown"; break;
                    case 0x1: outValue = "Other"; break;
                    case 0x2: outValue = "DRAM"; break;
                    case 0x3: outValue = "Synchronous DRAM"; break;
                    case 0x4: outValue = "Cache DRAM"; break;
                    case 0x5: outValue = "EDO"; break;
                    case 0x6: outValue = "EDRAM"; break;
                    case 0x7: outValue = "VRAM"; break;
                    case 0x8: outValue = "SRAM"; break;
                    case 0x9: outValue = "RAM"; break;
                    case 0xa: outValue = "ROM"; break;
                    case 0xb: outValue = "Flash"; break;
                    case 0xc: outValue = "EEPROM"; break;
                    case 0xd: outValue = "FEPROM"; break;
                    case 0xe: outValue = "EPROM"; break;
                    case 0xf: outValue = "CDRAM"; break;
                    case 0x10: outValue = "3DRAM"; break;
                    case 0x11: outValue = "SDRAM"; break;
                    case 0x12: outValue = "SGRAM"; break;
                    case 0x13: outValue = "RDRAM"; break;
                    case 0x14: outValue = "DDR"; break;
                    case 0x15: outValue = "DDR2"; break;
                    case 0x16: outValue = "DDR2 FB-DIMM"; break;
                    case 0x17: outValue = "Undefined 23"; break;
                    case 0x18: outValue = "DDR3"; break;
                    case 0x19: outValue = "FBD2"; break;
                    case 0x1a: outValue = "DDR4"; break;
                    default: outValue = "Undefined"; break;
                }

                return outValue;
            }
        }
    }
}
