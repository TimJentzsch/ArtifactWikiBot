using System;
using System.Text;
using System.Text.RegularExpressions;
using DSharpPlus.Entities;

namespace ArtifactWikiBot.Wiki
{
	public class WikiPage
	{
		public string Title { get; }
		public string Content { get; }
		public string ParsedContent { get; }

		public string[] Categories { get; }

		public DateTime LastChangeDate { get; }
		public string LastChangeUser { get; }

		/// <summary>
		/// Create a new WikiPage.
		/// </summary>
		/// <param name="title">The title of the page.</param>
		/// <param name="content">The (unparsed) content of the page.</param>
		/// <param name="categories">The page categories.</param>
		/// <param name="lastChangeDate">The date of the last revision.</param>
		/// <param name="lastChangeUser">The user who made the last page edit.</param>
		public WikiPage(string title, string content, string[] categories, string lastChangeDate, string lastChangeUser)
		{
			Title = title;
			Content = content;
			ParsedContent = ParseContent(content);
			Categories = categories;
			LastChangeDate = DateTime.Parse(lastChangeDate);
			LastChangeUser = lastChangeUser;
		}

		/// <summary>
		/// Parse the raw Wiki content to a Discord compatible format.
		/// </summary>
		/// <param name="content">The content to parse.</param>
		/// <returns>The parsed page content.</returns>
		public static string ParseContent(string content)
		{
			StringBuilder builder = new StringBuilder(content);
			//builder.Replace("\n", " ");

			string parsedContent = builder.ToString();

			// Delete Category links
			parsedContent = Regex.Replace(parsedContent, @"\[\[Category:[\w\s#]*\]\]", "");
			// Replace Lists
			parsedContent = Regex.Replace(parsedContent, @"\n\s*\*\*\*\s*", "\n--- ");
			parsedContent = Regex.Replace(parsedContent, @"\n\s*\*\*\s*", "\n-- ");
			parsedContent = Regex.Replace(parsedContent, @"\n\s*\*\s*", "\n- ");
			// Delete Comments
			parsedContent = Regex.Replace(parsedContent, @"<!--.*?-->", "", RegexOptions.Singleline);
			// Replace Bold
			parsedContent = Regex.Replace(parsedContent, @"'''(?<bold>.+?)'''", "**${bold}**");
			// Replace Cursive
			parsedContent = Regex.Replace(parsedContent, @"''(?<cursive>.+?)''", "*${cursive}*");
			// Replace Headers
			parsedContent = Regex.Replace(parsedContent, @"===(?<h3>.+?)===", "**${h3}**");
			parsedContent = Regex.Replace(parsedContent, @"==(?<h2>.+?)==", "__**${h2}**__");
			parsedContent = Regex.Replace(parsedContent, @"=(?<h1>.+?)=", "__**${h1}**__");
			// Replace Templates
			parsedContent = Regex.Replace(parsedContent, @"{{(Template:)?\s*(?<name>[^{}|]+)\s*(\s*\|(?<arg1>(([^{}|]+)=)?([^{}|]*)))?(\s*\|(([^{}|]+)=)?([^{}|]*))*}}", new MatchEvaluator(WikiTemplateReplacer));
			// Replace inner Links
			parsedContent = Regex.Replace(parsedContent, @"\[\[(?<page>.+?)(\|(?<title>.+?))?\]\]", new MatchEvaluator(WikiInnerLinkReplacer));
			// Replace outer Links
			parsedContent = Regex.Replace(parsedContent, @"\[(?<page>https?:\/\/\S+)\s(?<title>[^\]]+)?\]", new MatchEvaluator(WikiOuterLinkReplacer));
			// Replace linebreaks
			parsedContent = Regex.Replace(parsedContent, @"\n{3,}", "\n\n");

			return parsedContent;
		}
		/// <summary>
		/// Replace Wiki links to Wiki pages.
		/// </summary>
		/// <param name="match">The Regex match</param>
		/// <returns>The reformatted link.</returns>
		private static string WikiInnerLinkReplacer(Match match)
		{
			if (match.Groups["title"].Success)
			{
				return Util.GetDiscordWikiLink(match.Groups["page"].Value, match.Groups["title"].Value);
			}
			else
			{
				return Util.GetDiscordWikiLink(match.Groups["page"].Value);
			}
		}

		/// <summary>
		/// Replace Wiki links going to pages outside of the Wiki.
		/// </summary>
		/// <param name="match">The Regex match</param>
		/// <returns>The reformatted link.</returns>
		private static string WikiOuterLinkReplacer(Match match)
		{
			if (match.Groups["title"].Success)
			{
				return Util.GetDiscordLink(match.Groups["page"].Value, match.Groups["title"].Value);
			}
			else
			{
				return match.Groups["page"].Value;
			}
		}

		/// <summary>
		/// Replace Wiki templates with Discord compatible content.
		/// </summary>
		/// <param name="match">The Regex match</param>
		/// <returns>The reformatted template.</returns>
		private static string WikiTemplateReplacer(Match match)
		{
			// Get and normalize name
			string name = match.Groups["name"].Value;
			name = name.Replace('_', ' ');
			name = name.Trim();
			string _name = name.ToLower();
			
			// Templates to delete completely
			string[] noprint =
			{
				"unreleased content",
				"stub",
				"test",
				"tabs/hero",
				"tabs/item",
				"tabs/spell",
				"tabs/improvement",
				"tabs/creature",
				"hero infobox",
				"item infobox",
				"spell infobox",
				"creature infobox",
				"ability infobox",
				"improvement infobox"
			};

			foreach (string s in noprint)
			{
				if (_name.Equals(s))
					return "";
			}

			// The Card ID templates, replace with a link to their name
			string[] cardIDs =
			{
				"hero id",
				"spell id",
				"improvement id",
				"item id",
				"creature id",
				"h",
				"i",
				"im",
				"c",
				"s"
			};

			foreach(string s in cardIDs)
			{
				if (_name.Equals(s))
				{
					if (match.Groups["arg1"].Success)
						return Util.GetDiscordWikiLink(match.Groups["arg1"].Value);
					else
						return "";
				}
			}

			// Templates to replace with a link to their category
			string[] categories =
			{
				"blue",
				"red",
				"black",
				"green"
			};

			foreach(string s in categories)
			{
				if(_name.Equals(s))
				{
					return Util.GetDiscordWikiLink($"Category:{name}", name);
				}
			}

			// Format date
			if(_name.Equals("date"))
			{
				if (match.Groups["arg1"].Success)
				{
					DateTime time = DateTime.Parse(match.Groups["arg1"].Value);
					return time.ToString("yyyy-MM-dd");
				}
			}

			// Format note
			if(_name.Equals("note"))
			{
				if (match.Groups["arg1"].Success)
				{
					return $"*{match.Groups["arg1"].Value}*";
				}
			}

			return match.Value;
		}

		/// <summary>
		/// Convert the WikiPage to a DiscordEmbed.
		/// </summary>
		/// <returns>A DiscordEmbed containing the info gathered about the page.</returns>
		public DiscordEmbed ToDiscordEmbed()
		{
			string categoryS = "";
			if(Categories != null && Categories.Length > 0)
			{
				categoryS = Categories[0];
				for(int i = 1; i < Categories.Length; i++)
				{
					categoryS += $" | {Categories[i]}";
				}
				categoryS += " -- ";
			}

			var embed = new DiscordEmbedBuilder
			{
				Title = $"__**{Title}**__",
				Url = Util.GetWikiURL(Title),
				Description = ParsedContent.Limit(1024),
				Color = new DiscordColor(255, 211, 50)
			};

			embed.WithFooter($"{categoryS}Last edit by User:{LastChangeUser}", Util.GetImageURL("Artifact_Cutout.png"));
			embed.WithTimestamp(LastChangeDate);

			return embed;
		}
	}
}
