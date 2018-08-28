using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;

namespace ArtifactWikiBot.Feed
{
	public class FeedUser
	{
		public ulong ID { get; }
		public DateTime TimeJoined { get; }

		public FeedUser(ulong id, DateTime timeJoined)
		{
			ID = id;
			TimeJoined = timeJoined;
		}

		public DiscordUser ToDiscordUser()
		{
			return Bot.INSTANCE.Client.GetUserAsync(ID).Result;
		}
	}
}
