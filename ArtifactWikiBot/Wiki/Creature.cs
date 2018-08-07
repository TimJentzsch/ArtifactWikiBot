using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Newtonsoft.Json.Linq;
using System;

namespace ArtifactWikiBot.Wiki
{
	public class CreatureCard : Card
	{
		public static CreatureCard[] List { get; set; }

		public string Color { get; }
		public string Image { get; }
		public HeroCard Hero { get; }
		public AbilityCard Ability { get; }
		public int Attack { get; }
		public int Armor { get; }
		public int Health { get; }
		
		public CreatureCard(JToken creature)
		{
			Name = (string)creature["Title"];
			Color = ((string)creature["Color"]).ToLower();
			Image = (string)creature["Image"];
			Hero = HeroCard.GetByName((string)creature["Hero"], out HeroCard hero) ? hero : null;
			Ability = AbilityCard.GetByName((string)creature["Ability"], out AbilityCard ability) ? ability : null;
			Attack = Int32.TryParse((string)creature["Attack"], out int attack) ? attack : 0;
			Armor = Int32.TryParse((string)creature["Armor"], out int armor) ? armor : 0;
			Health = Int32.TryParse((string)creature["Health"], out int health) ? health : 0;
		}

		public static bool GetByName(string name, out CreatureCard result)
		{
			bool exists = List.SearchCard(name, out CreatureCard _result);
			result = _result;
			return exists;
		}

		public override DiscordEmbed ToDiscordEmbed(CommandContext ctx)
		{
			DiscordEmoji attack_emoji = Util.GetEmoji(ctx, "attack");
			DiscordEmoji armor_emoji = Util.GetEmoji(ctx, "armor");
			DiscordEmoji health_emoji = Util.GetEmoji(ctx, "health");

			DiscordColor discordColor = Util.GetColor(Color);

			string imageUrl = Util.GetImageURL(Image);
			string iconUrl = Hero != null ? Util.GetImageURL(Hero.Icon) : "";

			var embed = new DiscordEmbedBuilder
			{
				Title = $"**{Name}** ({Color})",
				Url = Util.GetWikiURL(Name),
				Description = $"{Attack} {attack_emoji}, {Armor} {armor_emoji}, {Health} {health_emoji}.",
				ImageUrl = imageUrl,
				ThumbnailUrl = iconUrl,
				Color = discordColor
			};

			if(Ability != null)
			{
				embed = Ability.AddToEmbed(embed);
			}

			return embed;
		}

		public override string ToString()
		{
			return $"{Name} ({Color} Hero): {Attack}ATK / {Armor}DEF / {Health}HP";
		}
	}
}
