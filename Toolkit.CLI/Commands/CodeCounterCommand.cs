using SharpTables;
using System.CommandLine;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Toolkit.CLI.Commands
{
    public partial class CodeCounterCommand : BaseCommand
    {
        private readonly Dictionary<string, string> extensionToName = new();
        private readonly HashSet<string> extensions = new();
        private readonly Dictionary<string, string> FileNames = new();
        private readonly Dictionary<string, int> lineCountPerType = new();
        private readonly Dictionary<string, int> lineCountPerFile = new();
        private int totalLinesOfCode = 0;
        private string Query = string.Empty;
        private string[] excludedFolders;
        private string[] excludedExtensions;

        public CodeCounterCommand() : base("codecount", "Count lines of code")
        {
            // Load language mappings from the JSON file
            string json = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data/ProgrammingLanguages.json"));
            var languages = JsonSerializer.Deserialize<List<ProgrammingLanguage>>(json)
                .Where(l => l.Extensions != null);

            if (languages == null)
            {
                Logger.LogError("Failed to load programming languages from JSON file.");
                return;
            }


            foreach (var lang in languages)
            {
                string name = lang.Name;
                if (lang.Extensions.Length == 0) continue;
                foreach (string extension in lang.Extensions)
                {
                    extensionToName.TryAdd(extension, name);
                    extensions.Add(extension);
                }
            }
        }

        public override void Setup(RootCommand root)
        {
            var excludeFoldersOption = new Option<string>(["--excluded-folders", "-ef"],
                "A comma-separated list of folders to ignore");
            var excludeExtensionsOption = new Option<string>(["--excluded-extensions", "-ee"],
                "A comma-separated list of file extensions to ignore");

            this.AddOption(excludeFoldersOption);
            this.AddOption(excludeExtensionsOption);

            this.SetHandler(Execute, excludeFoldersOption, excludeExtensionsOption);
            root.AddCommand(this);
        }
        public void Execute(string excludedFoldersOption, string excludedExtensionsOption)
        {
            if (!string.IsNullOrWhiteSpace(excludedFoldersOption))
                excludedFolders = excludedFoldersOption.Split(",");
            else excludedFolders = [];

            if (!string.IsNullOrWhiteSpace(excludedExtensionsOption))
                excludedExtensions = excludedExtensionsOption.Split(",");
            else excludedExtensions = [];

            var executingDirectory = Directory.GetCurrentDirectory();
            GetAllData(executingDirectory);
            LogSummary();
            //LogLinesPerFile(executingDirectory, false);
        }
        public string[] QueryToArray() => Query.Replace(" ", "").Split(",");

        public void GetAllData(string folder)
        {
            Logger.LogInformation("Reading files...");
            string[] files = GetFiles(folder);


            Logger.LogInformation($"Found {files.Length} files...");
            RunCounter(files);
        }


        public void RunCounter(string[] files)
        {
            foreach (string file in files)
            {
                string fileType = Path.GetExtension(file);
                string key = extensionToName.TryGetValue(fileType, out string name) ? name.Trim() : fileType;
                int lineCount = CountLines(file);

                if (lineCountPerType.ContainsKey(key))
                    lineCountPerType[key] += lineCount;
                else
                    lineCountPerType.Add(key, lineCount);

                if (lineCountPerFile.ContainsKey(file))
                    lineCountPerFile[file] += lineCount;
                else
                    lineCountPerFile.Add(file, lineCount);
            }
        }

        public void LogSummary()
        {
            int total = lineCountPerType.Values.Sum();
            Logger.LogInformation("Summary of Languages / File Types:");


            var table = new Table()
                .SetHeader("Language", "Lines", "Percentage");

            foreach (var (key, value) in lineCountPerType.OrderByDescending(x => x.Value))
            {
                table.AddRow(key, value.ToString(), Percentage(value, total).ToString());
            }

            table.UseSettings(new TableSettings()
            {
                CellPreset = c =>
                {
                    if (c.Position.X == 2)
                    {
                        c.Color = ConsoleColor.Yellow;
                        c.Text += "%";
                    }
                },
            })
                .UseFormatting(TableFormatting.Minimalist)
                .Write();
            Logger.LogInformation($"Total lines of code: {totalLinesOfCode}");

        }

        public void LogLinesPerFile(string rootDirectory, bool shortFileNames)
        {
            Logger.LogInformation("Lines of code per file:");
            foreach ((string key, int value) in lineCountPerFile.OrderByDescending(x => x.Value))
            {
                string fileType = Path.GetFileName(key).Split(".").Last();
                string fileName = shortFileNames ? Path.GetFileName(key) : Path.GetRelativePath(rootDirectory, key);
                Logger.LogInformation($"{fileName}: {value} lines, Type: {(FileNames.TryGetValue(fileType, out var name) ? name : fileType)}");
            }
        }

        public string[] GetFiles(string directory)
        {
            List<string> found = new();

            try
            {
                var excludedFoldersSet = new HashSet<string>(excludedFolders ?? []);
                var excludedExtensionsSet = new HashSet<string>(excludedExtensions.Select(e => e.TrimStart('.')) ?? []);
                foreach (string f in Directory.GetFiles(directory))
                {
                    string ext = Path.GetExtension(f).TrimStart('.');
                    if (!excludedExtensionsSet.Contains(ext))
                    {
                        found.Add(f);
                    }
                }
                foreach (string d in Directory.GetDirectories(directory))
                {
                    string dirName = Path.GetFileName(d);
                    if (!excludedFoldersSet.Contains(dirName))
                    {
                        found.AddRange(GetFiles(d));
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"Error when searching for files:\n{e}");
            }

            return found.ToArray();
        }

        public int CountLines(string file)
        {
            int count = 0;

            try
            {
                using (StreamReader r = new StreamReader(file))
                {
                    while (r.ReadLine() is { } rawline)
                    {
                        string line = rawline.Trim();
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        if (!CommentRegex().Match(line).Success)
                            count++;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"Error when counting lines in file {file}:\n{e}");
            }
            totalLinesOfCode += count;
            return count;
        }

        private float Percentage(int value, int total)
        {
            return MathF.Round((float)value / total * 100 * 100) / 100;
        }

        [GeneratedRegex(@"^(///|//|/\*|\*/|-->|<!--|#|\*)")]
        private static partial Regex CommentRegex();
    }
}
