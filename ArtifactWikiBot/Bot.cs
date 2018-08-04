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
using DSharpPlus.Net.WebSocket;
using System.Threading;

namespace ArtifactWikiBot
{
	/// <summary>
	/// The Discord Bot.
	/// </summary>
	public class Bot
	{
		// Static instance of the bot
		public static Bot INSTANCE = new Bot();

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
				Token = cfgjson.Token,
				TokenType = TokenType.Bot,

				AutoReconnect = true,
				LogLevel = LogLevel.Debug,
				UseInternalLogHandler = true
			};
			
			// Instantiate Client
			Client = new DiscordClient(cfg);

			// Initialize and update Json files
			await APIManager.Init(Client, 10, 1);
			Thread cardUpdater = new Thread(async () => await APIManager.CardUpdater());
			cardUpdater.Start();
			Thread changesUpdater = new Thread(async () => await APIManager.ChangesUpdater());
			changesUpdater.Start();


			// General Events
			Client.Ready += Client_Ready;
			Client.GuildAvailable += Client_GuildAvailable;
			Client.ClientErrored += Client_ClientError;

			// Configurate commands
			var ccfg = new CommandsNextConfiguration
			{
				StringPrefix = cfgjson.CommandPrefix,   // Command prefix (set in config.json)
				EnableDms = true,                       // Responding in DMs
				EnableMentionPrefix = true              // Mentioning the bot as command prefix
			};
			this.Commands = this.Client.UseCommandsNext(ccfg);

			// Command events
			Commands.CommandExecuted += Commands_CommandExecuted;
			Commands.CommandErrored += Commands_CommandErrored;

			// Wiki commands
			Commands.RegisterCommands<Commands>();

			// set up our custom help formatter
			Commands.SetHelpFormatter<SimpleHelpFormatter>();

			// Connect and log in
			await Client.ConnectAsync();

			BotStates.Init(Client);

			// Prevent premature quitting
			await Task.Delay(-1);
		}

		#region Events
		private Task Client_Ready(ReadyEventArgs e)
		{
			// let's log the fact that this event occured
			e.Client.DebugLogger.LogMessage(LogLevel.Info, "ArtifactWikiBot", "Client is ready to process events.", DateTime.Now);
			BotStates.SetReady();
			return Task.CompletedTask;
		}

		private Task Client_GuildAvailable(GuildCreateEventArgs e)
		{
			e.Client.DebugLogger.LogMessage(LogLevel.Info, "ArtifactWikiBot", $"Group available: {e.Guild.Name}", DateTime.Now);
			return Task.CompletedTask;
		}

		private Task Client_ClientError(ClientErrorEventArgs e)
		{
			e.Client.DebugLogger.LogMessage(LogLevel.Error, "ArtifactWikiBot", $"Exception occured: {e.Exception.GetType()}: {e.Exception.Message}", DateTime.Now);
			return Task.CompletedTask;
		}

		private Task Commands_CommandExecuted(CommandExecutionEventArgs e)
		{
			e.Context.Client.DebugLogger.LogMessage(LogLevel.Info, "ArtifactWikiBot", $"{e.Context.User.Username} successfully executed '{e.Command.QualifiedName}'", DateTime.Now);
			return Task.CompletedTask;
		}

		private async Task Commands_CommandErrored(CommandErrorEventArgs e)
		{
			e.Context.Client.DebugLogger.LogMessage(LogLevel.Error, "ArtifactWikiBot", $"{e.Context.User.Username} tried executing '{e.Command?.QualifiedName ?? "<unknown command>"}' but it errored: {e.Exception.GetType()}: {e.Exception.Message ?? "<no message>"}", DateTime.Now);
			if (e.Exception is ChecksFailedException ex)
			{
				// yes, the user lacks required permissions, 
				// let them know

				var emoji = DiscordEmoji.FromName(e.Context.Client, ":no_entry:");

				// let's wrap the response into an embed
				var embed = new DiscordEmbedBuilder
				{
					Title = "Access denied",
					Description = $"{emoji} You do not have the permissions required to execute this command.",
					Color = new DiscordColor(0xFF0000) // red
					// there are also some pre-defined colors available
					// as static members of the DiscordColor struct
				};
				await e.Context.RespondAsync("", embed: embed);
			}
		}
		#endregion
	}

	public struct ConfigJson
	{
		[JsonProperty("token")]
		public string Token { get; private set; }

		[JsonProperty("prefix")]
		public string CommandPrefix { get; private set; }
	}
}
