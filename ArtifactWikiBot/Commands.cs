using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Newtonsoft.Json.Linq;

namespace ArtifactWikiBot
{
    /// <summary>
    /// This class provides all commands available to the Bot.
    /// </summary>
    public class Commands
    {
        [Command("test")]
        [Description("Testing things out")]
        public async Task Test(CommandContext ctx)
        {
            var attack_emoji = DiscordEmoji.FromName(ctx.Client, ":crossed_swords:");
            var armor_emoji = DiscordEmoji.FromName(ctx.Client, ":shield:");
            var health_emoji = DiscordEmoji.FromName(ctx.Client, ":heart:");

            var embed = new DiscordEmbedBuilder
            {
                Title = "**Axe** (red)",
                Url = "https://artifactwiki.com/wiki/Axe",
                Description = $"7 {attack_emoji}, 2 {armor_emoji}, 11 {health_emoji}.",
                Color = new DiscordColor(255,0,0)
            };

            await ctx.RespondAsync(embed: embed);
        }

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
                Int32.TryParse((string)hero["Attack"], out int attack);
                Int32.TryParse((string)hero["Armor"], out int armor);
                Int32.TryParse((string)hero["Health"], out int health);

                var attack_emoji = DiscordEmoji.FromName(ctx.Client, ":crossed_swords:");
                var armor_emoji = DiscordEmoji.FromName(ctx.Client, ":shield:");
                var health_emoji = DiscordEmoji.FromName(ctx.Client, ":heart:");

                var discordColor = new DiscordColor(0, 0, 0);

                switch(color.ToLower())
                {
                    case "red":
                        discordColor = new DiscordColor(255, 0, 0);
                        break;
                    case "blue":
                        discordColor = new DiscordColor(0, 0, 255);
                        break;
                    case "green":
                        discordColor = new DiscordColor(0, 255, 0);
                        break;
                    case "black":
                        discordColor = new DiscordColor(0, 0, 0);
                        break;
                }

                string link = name.Replace(' ', '_');

                var embed = new DiscordEmbedBuilder
                {
                    Title = $"**{name}** ({color})",
                    Url = $"https://artifactwiki.com/wiki/{link}",
                    Description = $"{attack} {attack_emoji}, {armor} {armor_emoji}, {health} {health_emoji}.",
                    Color = discordColor
                };

                await ctx.RespondAsync(embed: embed);
            }
            else // Didn't find name
            {
                await ctx.RespondAsync($"We don't know a hero named {name}.");
            }
            await BotStates.SetReady();
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
                Int32.TryParse((string)creature["Attack"], out int attack);
                Int32.TryParse((string)creature["Armor"], out int armor);
                Int32.TryParse((string)creature["Health"], out int health);

                var attack_emoji = DiscordEmoji.FromName(ctx.Client, ":crossed_swords:");
                var armor_emoji = DiscordEmoji.FromName(ctx.Client, ":shield:");
                var health_emoji = DiscordEmoji.FromName(ctx.Client, ":heart:");

                await ctx.RespondAsync($"**{name}** ({color}): {attack_emoji}{attack}, {armor_emoji}{armor}, {health_emoji}{health}.");
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
                Int32.TryParse((string)item["Cost"], out int cost);
                string description = (string)item["Description"];

                string emojiName;

                switch (category.ToLower())
                {
                    case "weapon":
                        emojiName = ":crossed_swords:";
                        break;
                    case "armor":
                        emojiName = ":shield:";
                        break;
                    case "accessory":
                        emojiName = ":heart:";
                        break;
                    default:
                        emojiName = ":grey_question:";
                        break;
                }

                var emoji = DiscordEmoji.FromName(ctx.Client, emojiName);

                await ctx.RespondAsync($"{emoji} **{name}** ({cost} Gold).\n*{description}*");
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
                Int32.TryParse((string)spell["Mana"], out int mana);
                string hero = (string)spell["Hero"];
                string description = (string)spell["Description"];
                description = WebUtility.HtmlDecode(description);
                description = Regex.Replace(description, @"<[^>]*>", String.Empty);
                description = description.Replace("[[", "").Replace("]]", "");

                await ctx.RespondAsync($"**{name}** ({color}, {mana} Mana).\n*{description}*");
            }
            else // Didn't find name
            {
                await ctx.RespondAsync($"We don't know a spell named {name}.");
            }
            await BotStates.SetReady();
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
                Int32.TryParse((string)improvement["Mana"], out int mana);
                string hero = (string)improvement["Hero"];

                await ctx.RespondAsync($"**{name}** ({color}, {mana} Mana).");
            }
            else // Didn't find name
            {
                await ctx.RespondAsync($"We don't know a spell named {name}.");
            }
            await BotStates.SetReady();
        }


        [Command("mpcovcd")]
        [Description("Our lord and savior")]
        public async Task Mpcovcd(CommandContext ctx)
        {
            await ctx.RespondAsync("All hail @mpcovcd, our lord and savior!");
        }


        static async Task<Tuple<bool, JToken, string>> FindJToken(CommandContext ctx, JToken[] json, string[] _name, string title)
        {
            await BotStates.SetWorking();
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
