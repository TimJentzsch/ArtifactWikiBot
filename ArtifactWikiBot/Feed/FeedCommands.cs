using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using Newtonsoft.Json.Linq;
using LiteDB;
using System.Collections.Generic;

namespace ArtifactWikiBot.Feed
{
	public class FeedCommands
	{
		[Command("subscribe")]
		[Description("Subscribe to the feed.")]
		public async Task Subscribe(CommandContext ctx)
		{
			if(!ctx.Channel.IsPrivate && !ctx.Member.PermissionsIn(ctx.Channel).HasPermission(Permissions.Administrator))
			{
				var emoji = DiscordEmoji.FromName(ctx.Client, ":no_entry:");
				await ctx.RespondAsync($"{emoji} You are not an administrator in this channel! Please contact an admin to register.\n" +
					"You can also PM me ``!subscribe`` to stay up to date yourself.");
				return;
			}

			DiscordChannel channel = ctx.Channel;

			bool hasAdded = FeedManager.AddChannel(channel);
			
			if(hasAdded)
			{
				await ctx.RespondAsync("You will be notified when something happens!");
			}
			else
			{
				await ctx.RespondAsync("You already registered.");
			}
		}

		[Command("unsubscribe")]
		[Description("Unsubscribe to the feed.")]
		public async Task Unsubscribe(CommandContext ctx)
		{
			if (!ctx.Channel.IsPrivate && !ctx.Member.PermissionsIn(ctx.Channel).HasPermission(Permissions.Administrator))
			{
				var emoji = DiscordEmoji.FromName(ctx.Client, ":no_entry:");
				await ctx.RespondAsync($"{emoji} You are not an administrator in this channel! Please contact an admin to unsubscribe.");
				return;
			}

			DiscordChannel channel = ctx.Channel;

			bool hasDeleted = FeedManager.RemoveChannel(channel);

			if (hasDeleted)
			{
				await ctx.RespondAsync("You will not recieve any more notifications.");
			}
			else
			{
				await ctx.RespondAsync("You didn't register in the first place!");
			}
		}

		[Command("notify")]
		[Hidden]
		[RequireOwner]
		[Description("Notify users.")]
		public async Task Notify(CommandContext ctx)
		{
			System.Console.WriteLine("Getting channels");
			IEnumerable<FeedChannel> channels = FeedManager.GetFeedChannels();
			System.Console.WriteLine("Got channels.");

			string message = !ctx.RawArgumentString.IsEmpty() ? ctx.RawArgumentString : "Test Notification!";

			foreach (FeedChannel fc in channels)
			{
				System.Console.WriteLine($"Id: {fc.ChannelID} Time: {fc.TimeJoined}");
				await Bot.INSTANCE.Client.SendMessageAsync(fc.ToDiscordChannel().Result, message);
			}
		}

		[Command("test")]
		[Hidden]
		[RequireOwner]
		[Description("Test things.")]
		public async Task Test(CommandContext ctx)
		{
			System.Console.WriteLine(ctx.RawArgumentString);
		}
	}
}
