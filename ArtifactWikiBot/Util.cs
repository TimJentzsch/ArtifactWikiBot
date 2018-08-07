using System;
using System.Net;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using System.Text.RegularExpressions;
using ArtifactWikiBot.Wiki;

namespace ArtifactWikiBot
{
	public static class Util
	{
		public static bool IsEmpty(this string s)
		{
			if (s == null) return true;
			else
			{
				return Regex.IsMatch(s, @"/s");
			}
		}

		public static void Sort<T>(this T[] array) where T: Card
		{
			// Insertionsort
			for(int i = 1; i < array.Length; i++)
			{
				int j;
				// Save element
				T temp = array[i];
				for(j = i; j > 0 && array[j-1].CompareTo(temp) > 0; j--)
				{
					array[j] = array[j - 1];
				}
				// Insert element at the correct position
				array[j] = temp; 
			}
		}

		public static bool SearchCard<T>(this T[] array, string name, out T result) where T: Card
		{
			int left = 0;
			int right = array.Length - 1;
			
			// Binary Search
			while (left <= right)
			{
				int mid = (left + right) / 2;
				T element = array[mid];
				// Name bigger, search right half
				if(element.Name.CompareTo(name) > 0) { right = mid - 1; }
				// Name smaller, search left half
				else if(element.Name.CompareTo(name) < 0) { left = mid + 1; }
				// Name equal, return element
				else { result = element; return true; }
			}
			// Name not found
			result = null;
			return false;
		}

		public static string GetWikiURL(string link)
		{
			link = link.Replace(' ', '_');
			return $"https://artifactwiki.com/wiki/{link}";
		}

		// Get a Discord emoji representing an icon
		public static DiscordEmoji GetEmoji(CommandContext ctx, string name)
		{
			DiscordClient client = ctx.Client;
			name = name.ToLower();

			switch (name)
			{
				case "weapon":
					return DiscordEmoji.FromName(ctx.Client, ":crossed_swords:");
				case "attack":
					return DiscordEmoji.FromName(ctx.Client, ":crossed_swords:");
				case "armor":
					return DiscordEmoji.FromName(ctx.Client, ":shield:");
				case "accessory":
					return DiscordEmoji.FromName(ctx.Client, ":heart:");
				case "health":
					return DiscordEmoji.FromName(ctx.Client, ":heart:");
				case "consumable":
					// Until we find something better...
					return DiscordEmoji.FromName(ctx.Client, ":hamburger:");
				case "left":
					return DiscordEmoji.FromName(ctx.Client, ":arrow_backward:");
				case "right":
					return DiscordEmoji.FromName(ctx.Client, ":arrow_forward:");
				case "none":
					return null;
				default:
					return DiscordEmoji.FromName(client, ":question:");
			}
		}

		// Get a specified color
		public static DiscordColor GetColor(string name)
		{
			name = name.ToLower();

			switch (name)
			{
				case "red":
					return new DiscordColor(255, 0, 0);
				case "blue":
					return new DiscordColor(0, 0, 255);
				case "green":
					return new DiscordColor(0, 255, 0);
				case "black":
					return new DiscordColor(0, 0, 0);
				case "gold":
					return new DiscordColor(255, 215, 0);
				default:
					return new DiscordColor(255, 0, 255);
			}
		}

		// Remove Wiki syntax from a string
		public static string WikiDecode(string s)
		{
			// Decode HTML
			s = WebUtility.HtmlDecode(s);
			// Remove HTML tags
			s = Regex.Replace(s, @"<[^>]*>", String.Empty);
			// Remove Wiki Links
			s = s.Replace("[[", "").Replace("]]", "");

			return s;
		}

		// Retrieves the URL to an image on the Wiki
		public static string GetImageURL(string name)
		{
			if (name.StartsWith("File:") || name.StartsWith("file:"))
			{
				name = name.Substring(5);
			}
			name = name.Replace(' ', '_');
			return $"https://artifactwiki.com/wiki/Special:Redirect/file/{name}";
		}

		// Reconstruct an array argument into a single string argument
		public static string ReconstructArgument(string[] args)
		{
			string result = args[0];
			for (int i = 1; i < args.Length; i++)
			{
				result += " " + args[i];
			}
			return result;
		}
	}
}
