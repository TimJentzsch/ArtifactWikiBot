using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Newtonsoft.Json.Linq;
using System;

namespace ArtifactWikiBot.Wiki
{
	public class AbilityCard : Card
	{
		public static AbilityCard[] List { get; set; }
		
		public int Cooldown { get; }
		public string Icon { get; }
		public AbilityType Type { get; } 
		public string Description { get; }

		public AbilityCard(JToken ability)
		{
			Name = (string)ability["Title"];
			Icon = (string)ability["Icon"];
			Description = Util.WikiDecode((string)ability["Description"]);
			Cooldown = Int32.TryParse((string)ability["Cooldown"], out int cooldown) ? cooldown : 0;

			switch (((string)ability["Type"]).ToLower())
			{
				case "active":
					Type = AbilityType.Active;
					break;
				case "reactive":
					Type = AbilityType.Reactive;
					break;
				case "continuous":
					Type = AbilityType.Continuous;
					break;
				case "improvement":
					Type = AbilityType.Improvement;
					break;
				default:
					Type = AbilityType.Active;
					break;
			}
		}

		public static bool GetByName(string name, out AbilityCard result)
		{
			bool exists = List.SearchCard(name, out AbilityCard _result);
			result = _result;
			return exists;
		}

		public bool IsEmpty()
		{
			return Description.IsEmpty();
		}

		public override DiscordEmbed ToDiscordEmbed(CommandContext ctx)
		{
			string cooldownText = Cooldown != 0 ? $",{Cooldown} Cooldown" : "";
			var embed = new DiscordEmbedBuilder
			{
				Title = $"**{Name}** ({Type} Ability {cooldownText})",
				Url = Util.GetWikiURL(Name),
				Description = Description,
				ThumbnailUrl = Util.GetImageURL(Icon)
			};

			return embed;
		}

		public DiscordEmbedBuilder AddToEmbed(DiscordEmbedBuilder embed)
		{
			string cooldownText = Cooldown != 0 ? $" [{Cooldown}]" : "";
			string title = $"{Name} ({Type}{cooldownText})";
			string abilityDescription = !IsEmpty() ? Description : "<unknown effect>";
			embed.AddField(title, abilityDescription);
			return embed;
		}

		public override string ToString()
		{
			return $"{Name} ({Type} Ability [{Cooldown}]): {Description}";
		}
	}

	public enum AbilityType
	{
		Active, Reactive, Continuous, Improvement
	}
}
