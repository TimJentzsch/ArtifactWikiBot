using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using Newtonsoft.Json.Linq;

namespace ArtifactWikiBot.Wiki
{
	/// <summary>
	/// This class provides all commands available to the Bot.
	/// </summary>
	public class WikiCommands
	{
		[Command("page")]
		[Description("Get a page from the wiki")]
		public async Task Page(CommandContext ctx)
		{
			string name = ctx.RawArgumentString;
			WikiPage page = await APIManager.GetPage(name);

			if(page == null)
			{
				var embed = new DiscordEmbedBuilder
				{
					Title = $"**{name}**",
					Url = Util.GetWikiURL(name),
					Description = $"The page **{name}** doesn't exist yet!\nThink it's missing? Create it {Util.GetDiscordWikiEditLink(name, "here")}."
				};
				await ctx.RespondAsync(embed: embed);
			}
			else
			{
				await ctx.RespondAsync(embed: page.ToDiscordEmbed());
			}
		}

		[Command("changes")]
		[Description("Get the most recent changes")]
		public async Task RecentChanges(CommandContext ctx)
		{
			// Get the changes from the API Manager
			WikiChange[] changes = APIManager.RecentChanges;

			// Create a list to categorize the changes by date
			LinkedList<LinkedList<WikiChange>> dateChanges = new LinkedList<LinkedList<WikiChange>>();
			foreach(WikiChange c in changes)
			{
				bool newDate = true;
				foreach(LinkedList<WikiChange> cl in dateChanges)
				{
					// Check if a change with the same date is already enqueued
					if(cl.First.Value.Timestamp.Day.Equals(c.Timestamp.Day))
					{
						// Add the change to the list with the same date
						newDate = false;
						cl.AddLast(c);
						break;
					}
				}
				if(newDate)
				{
					// First change with this date, create a new list
					LinkedList<WikiChange> newList = new LinkedList<WikiChange>();
					newList.AddLast(c);
					dateChanges.AddLast(newList);
				}
			}

			// Basic embed without the changes
			var embed = new DiscordEmbedBuilder
			{
				Title = $"**Recent Changes**",
				Url = Util.GetWikiURL("Special:RecentChanges"),
				ThumbnailUrl = Util.GetImageURL("Artifact_Cutout.png"),
				Color = new DiscordColor(255, 211, 50)
			};

			// Add a field for every date with the matching changes
			foreach(LinkedList<WikiChange> cl in dateChanges)
			{
				// Format the date
				string date = cl.First.Value.Timestamp.ToString("yyyy-MM-dd");
				// Generate the field description
				string description = "";
				foreach (WikiChange c in cl)
				{
					// Format the changes for this date
					string changeString = c.ToDiscordDescription() + "\n";
					if ((description.Length + changeString.Length) < 1024)
					{
						// Add it to the description if it doesn't exceed the character limit
						description += changeString;
					}
					else
					{
						description += "...";
						break;
					}
				}
				// Add the field to the embed
				embed.AddField(date, description);
			}
			
			// Send the embed to the user
			await ctx.RespondAsync(embed: embed);
		}

		[Command("hero")]
		[Description("Get a hero from the database")]
		public async Task GetHeroCard(CommandContext ctx, [Description("Name of the hero.")] params string[] name)
		{
			string _name = Util.ReconstructArgument(name);

			if (HeroCard.GetByName(_name, out HeroCard hero)) 
			{
				// Found name
				// Respond with embed
				await ctx.RespondAsync(embed: hero.ToDiscordEmbed(ctx));
			}
			else 
			{
				// Didn't find name
				// Notify user
				await ctx.RespondAsync($"We don't know a hero named {_name}.");
			}
		}

		[Command("creature")]
		[Description("Get a creature from the database")]
		public async Task GetCreatureCard(CommandContext ctx, [Description("Name of the creature.")] params string[] name)
		{
			string _name = Util.ReconstructArgument(name);

			if (CreatureCard.GetByName(_name, out CreatureCard creature))
			{
				// Found name
				// Respond with embed
				await ctx.RespondAsync(embed: creature.ToDiscordEmbed(ctx));
			}
			else
			{
				// Didn't find name
				// Notify user
				await ctx.RespondAsync($"We don't know a creature named {_name}.");
			}
		}

		[Command("item")]
		[Description("Get an item from the database")]
		public async Task Item(CommandContext ctx, [Description("Name of the item.")] params string[] name)
		{
			string _name = Util.ReconstructArgument(name);

			if (ItemCard.GetByName(_name, out ItemCard item))
			{
				// Found name
				// Respond with embed
				await ctx.RespondAsync(embed: item.ToDiscordEmbed(ctx));
			}
			else
			{
				// Didn't find name
				// Notify user
				await ctx.RespondAsync($"We don't know an item named {_name}.");
			}
		}


		[Command("spell")]
		[Description("Get a spell from the database")]
		public async Task Spell(CommandContext ctx, [Description("Name of the spell.")] params string[] name)
		{
			string _name = Util.ReconstructArgument(name);

			if (SpellCard.GetByName(_name, out SpellCard spell))
			{
				// Found name
				// Respond with embed
				await ctx.RespondAsync(embed: spell.ToDiscordEmbed(ctx));
			}
			else
			{
				// Didn't find name
				// Notify user
				await ctx.RespondAsync($"We don't know a spell named {_name}.");
			}
		}

		[Command("improvement")]
		[Description("Get an improvement from the database")]
		public async Task Improvement(CommandContext ctx, [Description("Name of the improvement.")] params string[] name)
		{
			string _name = Util.ReconstructArgument(name);

			if (ImprovementCard.GetByName(_name, out ImprovementCard improvement))
			{
				// Found name
				// Respond with embed
				await ctx.RespondAsync(embed: improvement.ToDiscordEmbed(ctx));
			}
			else
			{
				// Didn't find name
				// Notify user
				await ctx.RespondAsync($"We don't know an improvement named {_name}.");
			}
		}


		[Command("mpcovcd")]
		[Hidden]
		[Description("Our lord and savior")]
		public async Task Mpcovcd(CommandContext ctx)
		{
			// Pay tribute to the server owner
			DiscordUserConverter converter = new DiscordUserConverter();
			if(converter.TryConvert("mpcovcd#3820", ctx, out DiscordUser result))
				await ctx.RespondAsync($"All hail {result.Mention}, our lord and savior!");
		}

		/// <summary>
		/// Searches for the name in the specified data base
		/// </summary>
		/// <param name="ctx">The CommandContext</param>
		/// <param name="json">The json array to search in</param>
		/// <param name="_name">The (unformatted) name to search for</param>
		/// <param name="title">The name of the command</param>
		/// <returns></returns>
		static async Task<Tuple<bool, JToken, string>> FindJToken(CommandContext ctx, JToken[] json, string[] _name, string title)
		{
			// Appear as typing
			await ctx.TriggerTypingAsync();

			// Check for name
			if (_name == null || _name.Length < 1)
			{
				await ctx.RespondAsync($"Try ``!{title} <{title} name>``.");
				return Tuple.Create<bool, JToken, string>(false, null, "");
			}

			// Reformat name
			string name = _name[0];
			for (int i = 1; i < _name.Length; i++)
			{
				name += " " + _name[i];
			}

			// Load Data
			JToken[] tokens = json;

			bool contains = false;
			JToken token = null;

			// Search for name
			for (int i = 0; i < tokens.Length; i++)
			{
				if (name.Equals((string)tokens[i]["Title"]))
				{
					contains = true;
					token = tokens[i];
					break;
				}
			}

			return Tuple.Create(contains, token, name);
		}

		[Group("admin")]
		[Description("Administrative commands.")]
		[Hidden]
		[RequirePermissions(Permissions.Administrator)]
		public class AdminCommands
		{
			[Command("update"), Description("Manually updates the Json Files."), Hidden, RequireOwner]
			public async Task Update(CommandContext ctx)
			{
				TimeSpan duration = await APIManager.UpdateAllCards();
				await ctx.RespondAsync($"Updated Json Files ({duration}).");
			}

			[Command("shutdown"), Description("Terminates the bot."), Hidden, RequireOwner]
			public async Task Shutdown(CommandContext ctx)
			{
				await ctx.RespondAsync($"Shutting Down...");
				await BotStates.SetShutDown();
				Bot.INSTANCE.Client.Dispose();
				System.Environment.Exit(1);
			}
		}
	}
}
