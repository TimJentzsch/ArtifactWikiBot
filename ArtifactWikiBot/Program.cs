using DSharpPlus;
using DSharpPlus.Interactivity;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Threading;

namespace ArtifactWikiBot
{
    class Program
    {
        static void Main(string[] args)
        {
            // Run the bot
            Bot.INSTANCE.RunAsync().GetAwaiter().GetResult();
        }
    }
}
