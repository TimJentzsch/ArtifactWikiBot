using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Newtonsoft.Json.Linq;
using System;

namespace ArtifactWikiBot.Wiki
{
	public class ItemCard : Card
	{
		public static ItemCard[] List { get; set; }
		
		public ItemCategory Category { get; }
		public string Image { get; }
		public string Description { get; }
		public int Cost { get; }
		public AbilityCard Ability { get; }

		public ItemCard(JToken creature)
		{
			// Assign values
			Name = (string)creature["Title"];
			Image = (string)creature["Image"];
			Description = Util.WikiDecode((string)creature["Description"]);
			Ability = AbilityCard.GetByName((string)creature["Ability"], out AbilityCard ability) ? ability : null;
			Cost = Int32.TryParse((string)creature["Cost"], out int cost) ? cost : 0;

			// Convert category into enum
			switch (((string)creature["Category"]).ToLower())
			{
				case "weapon":
					Category = ItemCategory.Weapon;
					break;
				case "armor":
					Category = ItemCategory.Armor;
					break;
				case "accessory":
					Category = ItemCategory.Accessory;
					break;
				case "consumable":
					Category = ItemCategory.Consumable;
					break;
				default:
					Category = ItemCategory.None;
					break;
			}
		}

		public static bool GetByName(string name, out ItemCard result)
		{
			bool exists = List.SearchCard(name, out ItemCard _result);
			result = _result;
			return exists;
		}

		public override DiscordEmbed ToDiscordEmbed(CommandContext ctx)
		{
			DiscordEmoji icon = Util.GetEmoji(ctx, Category.ToString());

			DiscordColor discordColor = Util.GetColor("gold");

			string imageUrl = Util.GetImageURL(Image);

			var embed = new DiscordEmbedBuilder
			{
				Title = $"{icon} **{Name}** ({Cost} Gold)",
				Url = Util.GetWikiURL(Name),
				Description = Description,
				ImageUrl = imageUrl,
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
			return $"{Name} ({Cost} Gold, {Category}): {Description}";
		}
	}

	public enum ItemCategory
	{
		Weapon, Armor, Accessory, Consumable, None
	}
}
