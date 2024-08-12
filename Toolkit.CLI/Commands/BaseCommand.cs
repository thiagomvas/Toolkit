﻿using System.CommandLine;

namespace Toolkit.CLI.Commands
{
    public abstract class BaseCommand : Command
    {
        protected BaseCommand(string name, string? description = null) : base(name, description)
        {
        }

        public abstract void Setup(RootCommand root);
    }
}
