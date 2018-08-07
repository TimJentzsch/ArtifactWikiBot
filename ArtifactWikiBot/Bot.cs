using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Newtonsoft.Json;
using System.Threading;
using ArtifactWikiBot.Wiki;

namespace ArtifactWikiBot
{
	/// <summary>
	/// The Discord Bot.
	/// </summary>
	public class Bot
	{
		// Static instance of the bot
		public static Bot INSTANCE = new Bot();

		public bool IsReady { get; set; }
		public DiscordClient Client { get; set; }
		public CommandsNextModule Commands { get; set; }
		public APIManager Manager { get; set; }

		public async Task RunAsync()
		{
			// Load config.json to string
			string json = "";
			using (FileStream fs = File.OpenRead("config.json"))
			using (StreamReader sr = new StreamReader(fs, new UTF8Encoding(false)))
				json = await sr.ReadToEndAsync();

			// Convert string to Discord Configuration
			ConfigJson cfgjson = JsonConvert.DeserializeObject<ConfigJson>(json);
			DiscordConfiguration cfg = new DiscordConfiguration
			{
				Token = cfgjson.DiscordToken,			// Assign the token
				TokenType = TokenType.Bot,		// It's a bot

				AutoReconnect = true,			// Reconnect automatically
				LogLevel = LogLevel.Debug,		// Show debug logs as well
				UseInternalLogHandler = true	// Use the internal log handler
			};
			
			// Instantiate Client
			Client = new DiscordClient(cfg);

			// Initialize and update Json files
			await APIManager.Init(Client, 15, 1);
			Thread cardUpdater = new Thread(async () => await APIManager.CardUpdater());
			cardUpdater.Start();
			Thread changesUpdater = new Thread(async () => await APIManager.ChangesUpdater());
			changesUpdater.Start();

			// Configurate commands
			var ccfg = new CommandsNextConfiguration
			{
				StringPrefix = cfgjson.CommandPrefix,   // Command prefix (set in config.json)
				EnableDms = true,                       // Responding in DMs
				EnableMentionPrefix = true              // Mentioning the bot as command prefix
			};
			this.Commands = this.Client.UseCommandsNext(ccfg);

			// General Events
			Client.Ready += Client_Ready;
			Client.GuildAvailable += Client_GuildAvailable;
			Client.ClientErrored += Client_ClientError;
			// Command events
			Commands.CommandExecuted += Commands_CommandExecuted;
			Commands.CommandErrored += Commands_CommandErrored;

			// Wiki commands
			Commands.RegisterCommands<WikiCommands>();

			// Connect and log in
			await Client.ConnectAsync();

			// Prevent premature quitting
			await Task.Delay(-1);
		}

		#region Events
		private Task Client_Ready(ReadyEventArgs e)
		{
			// Create a log message
			e.Client.DebugLogger.LogMessage(LogLevel.Info, "ArtifactWikiBot", "Client is online and ready.", DateTime.Now);
			// Signal that the client is ready now
			IsReady = true;
			BotStates.SetReady();
			return Task.CompletedTask;
		}

		private Task Client_GuildAvailable(GuildCreateEventArgs e)
		{
			// Create a log message
			e.Client.DebugLogger.LogMessage(LogLevel.Info, "ArtifactWikiBot", $"Group available: {e.Guild.Name}", DateTime.Now);
			return Task.CompletedTask;
		}

		private Task Client_ClientError(ClientErrorEventArgs e)
		{
			// Create a log message
			e.Client.DebugLogger.LogMessage(LogLevel.Error, "ArtifactWikiBot", $"An exception occured: {e.Exception.GetType()}: {e.Exception.Message}", DateTime.Now);
			return Task.CompletedTask;
		}

		private Task Commands_CommandExecuted(CommandExecutionEventArgs e)
		{
			// Create a log message
			e.Context.Client.DebugLogger.LogMessage(LogLevel.Info, "ArtifactWikiBot", $"{e.Context.User.Username} successfully executed the command'{e.Command.QualifiedName}'", DateTime.Now);
			return Task.CompletedTask;
		}

		private async Task Commands_CommandErrored(CommandErrorEventArgs e)
		{
			// Create a log message
			e.Context.Client.DebugLogger.LogMessage(LogLevel.Error, "ArtifactWikiBot", 
				$"{e.Context.User.Username} tried executing '{e.Command?.QualifiedName ?? "<unknown command>"}' " +
				$"but it errored: {e.Exception.GetType()}: {e.Exception.Message ?? "<no message>"}", DateTime.Now);
			if (e.Exception is ChecksFailedException ex)
			{
				// The user doesn't have permission to execute the command
				var emoji = DiscordEmoji.FromName(e.Context.Client, ":no_entry:");

				// Notify user
				var embed = new DiscordEmbedBuilder
				{
					Title = "Access denied",
					Description = $"{emoji} You do not have the permissions required to execute this command.",
					Color = new DiscordColor(255, 0, 0)
				};
				await e.Context.RespondAsync("", embed: embed);
			}
		}
		#endregion
	}

	// Struct to wrap the config.json
	public struct ConfigJson
	{
		// Your Discord Token
		// IMPORTANT: Never share this with anybody! Make sure the config.json is listed in your gitignore file.
		[JsonProperty("discordToken")]
		public string DiscordToken { get; private set; }

		// The command prefix
		[JsonProperty("prefix")]
		public string CommandPrefix { get; private set; }
	}
}
