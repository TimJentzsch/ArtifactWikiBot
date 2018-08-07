using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;

namespace ArtifactWikiBot.Wiki
{
	public abstract class Card : IComparable<Card>
	{
		public string Name { get; set; }

		public abstract DiscordEmbed ToDiscordEmbed(CommandContext ctx);

		public abstract override string ToString();

		public int CompareTo(Card other)
		{
			return Name.CompareTo(other.Name);
		}
	}
}
