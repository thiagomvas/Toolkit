
using System.CommandLine;
using Toolkit.CLI.Commands;

var root = new RootCommand("Utility functions in your terminal");

// Get all commands using reflection
var commandTypes = typeof(Program).Assembly.GetTypes()
    .Where(t => t.IsSubclassOf(typeof(BaseCommand)));

// Run setup
foreach (var commandType in commandTypes)
{
    var command = (BaseCommand)Activator.CreateInstance(commandType);
    command.Setup(root);
}
return await root.InvokeAsync(args);