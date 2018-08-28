using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using LiteDB;
using RedditSharp;
using RedditSharp.Things;

namespace ArtifactWikiBot.Feed
{

	public class FeedManager
	{
		public static bool AddChannel(DiscordChannel channel)
		{
			using(LiteDatabase UserDatabase = new LiteDatabase("channels.db"))
			{
				var col = UserDatabase.GetCollection<FeedChannel>("channels");

				int i = 0;
				Console.WriteLine("Query");
				FeedChannel query = col.FindById(FeedChannel.GetHashID(channel.Id, i));
				Console.WriteLine("Queried");
				while (query != null &&  query.ChannelID != channel.Id)
				{
					Console.WriteLine("Next..");
					query = col.FindById(FeedChannel.GetHashID(channel.Id, i));
					i++;
				}

				if(query == null)
				{
					Console.WriteLine("New Channel");
					if(query != null && query.IsFiller)
					{
						Console.WriteLine("Delete filler");
						col.Delete(FeedChannel.GetHashID(channel.Id, i));
					}
					Console.WriteLine("Create Feed Channel");
					var fc = new FeedChannel(channel, i);
					Console.WriteLine("Insert Channel");
					col.Insert(fc);
					Console.WriteLine("Ensure Index");
					col.EnsureIndex("Id");
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		public static bool RemoveChannel(DiscordChannel channel)
		{
			using (LiteDatabase UserDatabase = new LiteDatabase("channels.db"))
			{
				// Get user collection
				var col = UserDatabase.GetCollection<FeedChannel>("channels");

				int i = 0;
				FeedChannel query = col.FindById(FeedChannel.GetHashID(channel.Id, i));
				while (query != null && query.ChannelID != channel.Id)
				{
					query = col.FindById(FeedChannel.GetHashID(channel.Id, i));
					i++;
				}

				bool hasDeleted = false;

				if(query != null && !query.IsFiller)
				{
					Console.WriteLine("Delete..");
					hasDeleted = col.Delete(FeedChannel.GetHashID(channel.Id, i));
					Console.WriteLine("Deleted");
				}

				if (hasDeleted)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		public static IEnumerable<FeedChannel> GetFeedChannels()
		{
			using(LiteDatabase UserDatabase = new LiteDatabase("channels.db"))
			{
				// Get user collection
				var channels = UserDatabase.GetCollection<FeedChannel>("channels");

				return channels.FindAll();
			}
		}

		public static void GetRedditPost()
		{
			Reddit r = null;

			RedditUser u = r.GetUser("Magesunite");
			var posts = u.Posts;

			var post = posts.First();
		}
	}
}
