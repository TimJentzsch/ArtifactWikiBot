using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Newtonsoft.Json.Linq;
using System;

namespace ArtifactWikiBot.Wiki
{
	public class HeroCard : Card
	{
		public static HeroCard[] List { get; set; }

		public string Color { get; }
		public string Icon { get; }
		public string Image { get; }
		public AbilityCard Ability { get; }
		public int Attack { get; }
		public int Armor { get; }
		public int Health { get; }

		public HeroCard(JToken hero)
		{
			Name = (string)hero["Title"];
			Color = ((string)hero["Color"]).ToLower();
			Icon = (string)hero["Icon"];
			Image = (string)hero["Image"];
			Ability = AbilityCard.GetByName((string)hero["Ability"], out AbilityCard ability) ? ability : null;
			Attack = Int32.TryParse((string)hero["Attack"], out int attack) ? attack : 0;
			Armor = Int32.TryParse((string)hero["Armor"], out int armor) ? armor : 0;
			Health = Int32.TryParse((string)hero["Health"], out int health) ? health : 0;
		}

		public static bool GetHeroIcon(string name, out string image)
		{
			if(GetByName(name, out HeroCard result))
			{
				image =  result.Icon;
				return true;
			}
			else
			{
				image = null;
				return false;
			}
		}

		public static bool GetByName(string name, out HeroCard result)
		{
			bool exists = List.SearchCard(name, out HeroCard _result);
			result = _result;
			return exists;
		}

		public override DiscordEmbed ToDiscordEmbed(CommandContext ctx)
		{
			DiscordEmoji attack_emoji = Util.GetEmoji(ctx, "attack");
			DiscordEmoji armor_emoji = Util.GetEmoji(ctx, "armor");
			DiscordEmoji health_emoji = Util.GetEmoji(ctx, "health");

			DiscordColor discordColor = Util.GetColor(Color);

			var embed = new DiscordEmbedBuilder
			{
				Title = $"**{Name}** ({Color})",
				Url = Util.GetWikiURL(Name),
				Description = $"{Attack} {attack_emoji}, {Armor} {armor_emoji}, {Health} {health_emoji}.",
				ThumbnailUrl = Util.GetImageURL(Icon),
				ImageUrl = Util.GetImageURL(Image),
				Color = discordColor
			};

			Console.WriteLine(Ability);

			if(Ability != null)
			{
				embed = Ability.AddToEmbed(embed);
			}

			return embed;
		}

		public override string ToString()
		{
			return $"{Name} ({Color} Hero): {Attack}ATK / {Armor}DEF / {Health} HP";
		}
	}
}
