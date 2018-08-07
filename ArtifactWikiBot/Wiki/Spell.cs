using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Newtonsoft.Json.Linq;
using System;

namespace ArtifactWikiBot.Wiki
{
	public class SpellCard : Card
	{
		public static SpellCard[] List { get; set; }

		public string Color { get; }
		public int Mana { get; }
		public LaneType Lane { get; }
		public string Image { get; }
		public HeroCard Hero { get; }
		public string Description { get; }

		public SpellCard(JToken spell)
		{
			Name = (string)spell["Title"];
			Color = ((string)spell["Color"]).ToLower();
			Mana = Int32.TryParse((string)spell["Mana"], out int mana) ? mana : 0;
			Image = (string)spell["Image"];
			Hero = HeroCard.GetByName((string)spell["Hero"], out HeroCard hero) ? hero : null;
			Description = Util.WikiDecode((string)spell["Description"]);

			switch(((string)spell["Lane"]).ToLower())
			{
				case "any":
					Lane = LaneType.Any;
					break;
				default:
					Lane = LaneType.Single;
					break;
			}
		}

		public static bool GetByName(string name, out SpellCard result)
		{
			bool exists = List.SearchCard(name, out SpellCard _result);
			result = _result;
			return exists;
		}

		public override DiscordEmbed ToDiscordEmbed(CommandContext ctx)
		{
			DiscordColor discordColor = Util.GetColor(Color);

			string imageUrl = Util.GetImageURL(Image);
			string iconUrl = Hero != null ? Util.GetImageURL(Hero.Icon) : "";

			DiscordEmoji leftLane = Lane == LaneType.Any ? Util.GetEmoji(ctx, "left") : null; // Left Arrow
			DiscordEmoji rightLane = Lane == LaneType.Any ? Util.GetEmoji(ctx, "right") : null;	// Right Arrow

			var embed = new DiscordEmbedBuilder
			{
				Title = $"{leftLane}{rightLane}[{Mana}] **{Name}** ({Color})",
				Url = Util.GetWikiURL(Name),
				Description = Description,
				ImageUrl = imageUrl,
				ThumbnailUrl = iconUrl,
				Color = discordColor
			};

			return embed;
		}

		public override string ToString()
		{
			return $"{Name} ({Mana} Mana, {Lane}): {Description}";
		}
	}
}
