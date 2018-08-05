using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using Newtonsoft.Json.Linq;

namespace ArtifactWikiBot
{
	/// <summary>
	/// This class provides all commands available to the Bot.
	/// </summary>
	public class WikiCommands
	{
		[Command("changes")]
		[Description("Get the most recent changes")]
		public async Task RecentChanges(CommandContext ctx)
		{
			WikiChange[] changes = APIManager.RecentChanges;

			string output = changes[0].ToString();
			for(int i = 1; i < changes.Length; i++)
			{
				output += "\n" + changes[i].ToString();
			}

			await ctx.RespondAsync(output);
		}

		[Command("hero")]
		[Description("Get a hero from the database")]
		public async Task Hero(CommandContext ctx, [Description("Name of the hero.")] params string[] _name)
		{
			Tuple<bool, JToken, string> result = FindJToken(ctx, APIManager.Heroes, _name, "hero").Result;
			string name = result.Item3;
			Console.WriteLine(name);
			if (result.Item1) // Found name
			{
				JToken hero = result.Item2;

				string color = (string)hero["Color"];
				string icon = (string)hero["Icon"];
				string image = (string)hero["Image"];
				string ability = (string)hero["Ability"];
				Int32.TryParse((string)hero["Attack"], out int attack);
				Int32.TryParse((string)hero["Armor"], out int armor);
				Int32.TryParse((string)hero["Health"], out int health);

				var attack_emoji = GetEmoji(ctx, "attack");
				var armor_emoji = GetEmoji(ctx, "armor");
				var health_emoji = GetEmoji(ctx, "health");

				var discordColor = GetColor(color);

				// Convert the link into the right format
				string link = name.Replace(' ', '_');

				var embed = new DiscordEmbedBuilder
				{
					Title = $"**{name}** ({color})",
					Url = $"https://artifactwiki.com/wiki/{link}",
					Description = $"{attack} {attack_emoji}, {armor} {armor_emoji}, {health} {health_emoji}.",
					ThumbnailUrl = GetImageURL(icon),
					ImageUrl = GetImageURL(image),
					Color = discordColor
				};

				await ctx.RespondAsync(embed: embed);
			}
			else // Didn't find name
			{
				await ctx.RespondAsync($"We don't know a hero named {name}.");
			}
		}

		[Command("creature")]
		[Description("Get a creature from the database")]
		public async Task Creature(CommandContext ctx, [Description("Name of the creature.")] params string[] _name)
		{

			Tuple<bool, JToken, string> result = FindJToken(ctx, APIManager.Creatures, _name, "creature").Result;
			string name = result.Item3;
			if (result.Item1) // Found name
			{
				JToken creature = result.Item2;

				string color = (string)creature["Color"];
				string image = (string)creature["Image"];
				Int32.TryParse((string)creature["Attack"], out int attack);
				Int32.TryParse((string)creature["Armor"], out int armor);
				Int32.TryParse((string)creature["Health"], out int health);

				var attack_emoji = GetEmoji(ctx, "attack");
				var armor_emoji = GetEmoji(ctx, "armor");
				var health_emoji = GetEmoji(ctx, "health");

				var discordColor = GetColor(color);

				// Convert the link into the right format
				string link = name.Replace(' ', '_');

				var embed = new DiscordEmbedBuilder
				{
					Title = $"**{name}** ({color})",
					Url = $"https://artifactwiki.com/wiki/{link}",
					Description = $"{attack} {attack_emoji}, {armor} {armor_emoji}, {health} {health_emoji}.",
					ImageUrl = GetImageURL(image),
					Color = discordColor
				};

				await ctx.RespondAsync(embed: embed);
			}
			else // Didn't find name
			{
				await ctx.RespondAsync($"We don't know a creature named {name}.");
			}
			await BotStates.SetReady();
		}

		[Command("item")]
		[Description("Get an item from the database")]
		public async Task Item(CommandContext ctx, [Description("Name of the item.")] params string[] _name)
		{
			Tuple<bool, JToken, string> result = FindJToken(ctx, APIManager.Items, _name, "item").Result;
			string name = result.Item3;
			if (result.Item1) // Found name
			{
				JToken item = result.Item2;
				string category = (string)item["Category"];
				string image = (string)item["Image"];
				string ability = (string)item["Active"];
				Int32.TryParse((string)item["Cost"], out int cost);
				string description = (string)item["Description"];
				description = WikiDecode(description);

				var emoji = GetEmoji(ctx, category);

				var discordColor = GetColor("gold");

				// Convert the link into the right format
				string link = name.Replace(' ', '_');

				var embed = new DiscordEmbedBuilder
				{
					Title = $"{emoji} **{name}** ({cost} Gold)",
					Url = $"https://artifactwiki.com/wiki/{link}",
					Description = $"*{description}*",
					ImageUrl = GetImageURL(image),
					Color = discordColor
				};

				await ctx.RespondAsync(embed: embed);
			}
			else // Didn't find name
			{
				await ctx.RespondAsync($"We don't know an item named {name}.");
			}
			await BotStates.SetReady();
		}


		[Command("spell")]
		[Description("Get a spell from the database")]
		public async Task Spell(CommandContext ctx, [Description("Name of the spell.")] params string[] _name)
		{
			Tuple<bool, JToken, string> result = FindJToken(ctx, APIManager.Spells, _name, "spell").Result;
			string name = result.Item3;
			if (result.Item1) // Found name
			{
				JToken spell = result.Item2;
				string color = (string)spell["Color"];
				string image = (string)spell["Image"];
				Int32.TryParse((string)spell["Mana"], out int mana);
				string hero = (string)spell["Hero"];
				string description = (string)spell["Description"];
				description = WikiDecode(description);

				var discordColor = GetColor(color);

				// Convert the link into the right format
				string link = name.Replace(' ', '_');

				var embed = new DiscordEmbedBuilder
				{
					Title = $"**{name}** ({mana} Mana)",
					Url = $"https://artifactwiki.com/wiki/{link}",
					Description = $"*{description}*",
					ImageUrl = GetImageURL(image),
					Color = discordColor
				};

				await ctx.RespondAsync(embed: embed);
			}
			else // Didn't find name
			{
				await ctx.RespondAsync($"We don't know a spell named {name}.");
			}
		}

		[Command("improvement")]
		[Description("Get a spell from the database")]
		public async Task Improvement(CommandContext ctx, [Description("Name of the improvement.")] params string[] _name)
		{
			Tuple<bool, JToken, string> result = FindJToken(ctx, APIManager.Improvements, _name, "improvement").Result;
			string name = result.Item3;
			if (result.Item1) // Found name
			{
				JToken improvement = result.Item2;
				string color = (string)improvement["Color"];
				string icon = (string)improvement["Icon"];
				string image = (string)improvement["Image"];
				string ability = (string)improvement["Reactive"];
				Int32.TryParse((string)improvement["Mana"], out int mana);
				string hero = (string)improvement["Hero"];

				var discordColor = GetColor(color);

				// Convert the link into the right format
				string link = name.Replace(' ', '_');

				var embed = new DiscordEmbedBuilder
				{
					Title = $"**{name}** ({mana} Mana)",
					Url = $"https://artifactwiki.com/wiki/{link}",
					ThumbnailUrl = GetImageURL(icon),
					ImageUrl = GetImageURL(image),
					Color = discordColor
				};

				await ctx.RespondAsync(embed: embed);
			}
			else // Didn't find name
			{
				await ctx.RespondAsync($"We don't know a spell named {name}.");
			}
		}


		[Command("mpcovcd")]
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

		// Get a Discord emoji representing an icon
		public static DiscordEmoji GetEmoji(CommandContext ctx, string name)
		{
			DiscordClient client = ctx.Client;
			name = name.ToLower();

			switch (name)
			{
				case "weapon":
					return DiscordEmoji.FromName(ctx.Client, ":crossed_swords:");
				case "attack":
					return DiscordEmoji.FromName(ctx.Client, ":crossed_swords:");
				case "armor":
					return DiscordEmoji.FromName(ctx.Client, ":shield:");
				case "accessory":
					return DiscordEmoji.FromName(ctx.Client, ":heart:");
				case "health":
					return DiscordEmoji.FromName(ctx.Client, ":heart:");
				case "consumable":
					// Until we find something better...
					return DiscordEmoji.FromName(ctx.Client, ":hamburger:");
				default:
					return DiscordEmoji.FromName(client, ":question:");
			}
		}

		// Get a specified color
		public static DiscordColor GetColor(string name)
		{
			name = name.ToLower();

			switch (name)
			{
				case "red":
					return new DiscordColor(255, 0, 0);
				case "blue":
					return new DiscordColor(0, 0, 255);
				case "green":
					return new DiscordColor(0, 255, 0);
				case "black":
					return new DiscordColor(0, 0, 0);
				case "gold":
					return new DiscordColor(255, 215, 0);
				default:
					return new DiscordColor(255, 0, 255);
			}
		}

		// Remove Wiki syntax from a string
		public static string WikiDecode(string s)
		{
			// Decode HTML
			s = WebUtility.HtmlDecode(s);
			// Remove HTML tags
			s = Regex.Replace(s, @"<[^>]*>", String.Empty);
			// Remove Wiki Links
			s = s.Replace("[[", "").Replace("]]", "");

			return s;
		}

		// Retrieves the URL to an image on the Wiki
		public static string GetImageURL(string name)
		{
			if(name.StartsWith("File:") || name.StartsWith("file:"))
			{
				name = name.Substring(5);
			}
			name = name.Replace(' ', '_');
			return $"https://artifactwiki.com/wiki/Special:Redirect/file/{name}";
		}
	}
}
