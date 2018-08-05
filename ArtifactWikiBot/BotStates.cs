using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;

namespace ArtifactWikiBot
{
	/// <summary>
	/// This class provides functionality to change the bots "game" to communicate 
	/// the state of the bot to the user.
	/// </summary>
	public static class BotStates
	{
		private static DiscordClient Client => Bot.INSTANCE.Client;
		private static bool IsReady => Bot.INSTANCE.IsReady;

		// Playing ArtifactWiki.com
		static DiscordGame READY = new DiscordGame
		{
			Name = "ArtifactWiki.com",
			Details = "Nothing to do.",
			State = "Online"
		};

		// Playing Updating...
		static DiscordGame UPDATING = new DiscordGame
		{
			Name = "Updating...",
			Details = "Updating Json files.",
			State = "DoNotDisturb"
		};

		// Playing Working...
		static DiscordGame WORKING = new DiscordGame
		{
			Name = "Working...",
			Details = "Processing user request.",
			State = "DoNotDisturb"
		};

		// Playing Shutting  Down...
		static DiscordGame SHUTDOWN = new DiscordGame
		{
			Name = "Shutting Down...",
			Details = "Shutting down.",
			State = "DoNotDisturb"
		};

		// Signal that the bot is online and ready to work
		public static Task SetReady()
		{
			if (Client == null || !IsReady)
				return Task.CompletedTask;
			Client.UpdateStatusAsync(READY, UserStatus.Online, DateTime.Now);
			return Task.CompletedTask;
		}

		// Signal that the bot is updating its data base
		public static Task SetUpdating()
		{
			if (Client == null || !IsReady)
				return Task.CompletedTask;
			Client.UpdateStatusAsync(UPDATING, UserStatus.DoNotDisturb, DateTime.Now);
			return Task.CompletedTask;
		}

		// Signal that the bot is processing a user request
		public static Task SetWorking()
		{
			if (Client == null || !IsReady)
				return Task.CompletedTask;
			Client.UpdateStatusAsync(WORKING, UserStatus.DoNotDisturb, DateTime.Now);
			return Task.CompletedTask;
		}

		// Signal that the bot is shutting down
		public static Task SetShutDown()
		{
			if (Client == null || !IsReady)
				return Task.CompletedTask;
			Client.UpdateStatusAsync(SHUTDOWN, UserStatus.Invisible, DateTime.Now);
			return Task.CompletedTask;
		}
	}
}
