using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace ArtifactWikiBot.Feed
{
	public class FeedChannel
	{
		private int Iteration { get; set; }

		public bool IsFiller { get; set; }

		public int Id { get; set; }
		public ulong ChannelID { get; set; }
		public DateTime TimeJoined { get; set; }

		public FeedChannel()
		{
			Iteration = 0;
			IsFiller = true;
			Id = 0;
			ChannelID = 0;
			TimeJoined = default(DateTime);
		}

		public FeedChannel(DiscordChannel channel, int iteration)
		{
			IsFiller = false;
			Iteration = iteration;
			ChannelID = channel.Id;
			Id = GetHashID(ChannelID, Iteration);
			TimeJoined = DateTime.Now;
		}

		public FeedChannel(ulong channelID, DateTime timeJoined, int iteration)
		{
			IsFiller = false;
			Iteration = iteration;
			ChannelID = channelID;
			Id = GetHashID(ChannelID, Iteration);
			TimeJoined = timeJoined;
		}

		public FeedChannel(int id)
		{
			IsFiller = true;
			Iteration = 0;
			ChannelID = 0;
			Id = id;
			TimeJoined = default(DateTime);
		}

		/// <summary>
		/// Convert the FeedChannel back to a DiscordChannel.
		/// </summary>
		/// <returns></returns>
		public Task<DiscordChannel> ToDiscordChannel()
		{
			return Bot.INSTANCE.Client.GetChannelAsync(ChannelID);
		}

		public override string ToString()
		{
			return $"{ChannelID} joined on {TimeJoined}";
		}

		/// <summary>
		/// Converts the ulong Id of a DiscordChannel in an int via linear hashing.
		/// </summary>
		/// <param name="channelID">The ID to convert</param>
		/// <param name="iteration">The iteration of the hashing</param>
		/// <returns>The converted int ID</returns>
		public static int GetHashID(ulong channelID, int iteration)
		{
			return (int)(((channelID % Int32.MaxValue) + (ulong)iteration) & Int32.MaxValue);
		}
	}
}
