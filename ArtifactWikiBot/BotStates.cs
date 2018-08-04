using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;

namespace ArtifactWikiBot
{
    public static class BotStates
    {
        private static DiscordClient Client { get; set; }

        static DiscordGame READY = new DiscordGame
        {
            Name = "ArtifactWiki.com",
            Details = "Nothing to do.",
            State = "Online"
        };

        static DiscordGame UPDATING = new DiscordGame
        {
            Name = "Updating...",
            Details = "Updating Json files.",
            State = "DoNotDisturb"
        };

        static DiscordGame WORKING = new DiscordGame
        {
            Name = "Working...",
            Details = "Processing user request.",
            State = "DoNotDisturb"
        };

        static DiscordGame SHUTDOWN = new DiscordGame
        {
            Name = "Shutting Down...",
            Details = "Shutting down.",
            State = "DoNotDisturb"
        };

        public static Task SetReady()
        {
            if (Client == null)
                return Task.CompletedTask;
            Client.UpdateStatusAsync(READY, UserStatus.Online, DateTime.Now);
            return Task.CompletedTask;
        }

        public static Task SetUpdating()
        {
            if (Client == null)
                return Task.CompletedTask;
            Client.UpdateStatusAsync(UPDATING, UserStatus.DoNotDisturb, DateTime.Now);
            return Task.CompletedTask;
        }

        public static Task SetWorking()
        {
            if (Client == null)
                return Task.CompletedTask;
            Client.UpdateStatusAsync(WORKING, UserStatus.DoNotDisturb, DateTime.Now);
            return Task.CompletedTask;
        }

        public static Task SetShutDown()
        {
            if (Client == null)
                return Task.CompletedTask;
            Client.UpdateStatusAsync(SHUTDOWN, UserStatus.Invisible, DateTime.Now);
            return Task.CompletedTask;
        }

        public static void Init(DiscordClient client)
        {
            Client = client;
        }
    }
}
