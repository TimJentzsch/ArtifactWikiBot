using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Newtonsoft.Json.Linq;
using System;

namespace ArtifactWikiBot.Wiki
{
	public class ImprovementCard : Card
	{
		public static ImprovementCard[] List { get; set; }

		public string Color { get; }
		public int Mana { get; }
		public LaneType Lane { get; }
		public string Image { get; }
		public string Icon { get; }
		public HeroCard Hero { get; }
		public AbilityCard Ability { get; }

		public ImprovementCard(JToken improvement)
		{
			Name = (string)improvement["Title"];
			Color = ((string)improvement["Color"]).ToLower();
			Mana = Int32.TryParse((string)improvement["Mana"], out int mana) ? mana : 0;
			Image = (string)improvement["Image"];
			Icon = (string)improvement["Icon"];
			Hero = HeroCard.GetByName((string)improvement["Hero"], out HeroCard hero) ? hero : null;
			Ability = AbilityCard.GetByName((string)improvement["Reactive"], out AbilityCard ability) ? ability : null;

			switch (((string)improvement["Lane"]).ToLower())
			{
				case "any":
					Lane = LaneType.Any;
					break;
				default:
					Lane = LaneType.Single;
					break;
			}
		}

		public static bool GetByName(string name, out ImprovementCard result)
		{
			bool exists = List.SearchCard(name, out ImprovementCard _result);
			result = _result;
			return exists;
		}

		public override DiscordEmbed ToDiscordEmbed(CommandContext ctx)
		{
			DiscordColor discordColor = Util.GetColor(Color);

			string imageUrl = Util.GetImageURL(Image);
			string heroIconUrl = Hero != null ? Util.GetImageURL(Hero.Icon) : "";

			DiscordEmoji leftLane = Lane == LaneType.Any ? Util.GetEmoji(ctx, "left") : null; // Left Arrow
			DiscordEmoji rightLane = Lane == LaneType.Any ? Util.GetEmoji(ctx, "right") : null; // Right Arrow

			var embed = new DiscordEmbedBuilder
			{
				Title = $"{leftLane}{rightLane}[{Mana}] **{Name}** ({Color})",
				Url = Util.GetWikiURL(Name),
				ImageUrl = imageUrl,
				ThumbnailUrl = heroIconUrl,
				Color = discordColor
			};

			if (Ability != null)
			{
				embed = Ability.AddToEmbed(embed);
			}

			return embed;
		}

		public override string ToString()
		{
			return $"{Name} ({Mana} Mana, {Lane}): {Ability.Name}: {Ability.Description}";
		}
	}
}
