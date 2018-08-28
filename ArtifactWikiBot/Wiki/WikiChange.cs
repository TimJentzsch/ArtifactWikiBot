using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ArtifactWikiBot.Wiki
{
	public class WikiChange : IComparable<WikiChange>
	{
		[JsonProperty("timestamp")]
		public DateTime Timestamp { get; private set; }

		[JsonProperty("type")]
		public string Type { get; private set; }

		[JsonProperty("user")]
		public string User { get; private set; }

		[JsonProperty("title")]
		public string Title { get; private set; }

		[JsonProperty("parsedcomment")]
		public string Comment { get; private set; }

		[JsonProperty("logaction")]
		public string Logaction { get; private set; }

		public override string ToString()
		{
			switch (Type)
			{
				case "log":
					switch (Logaction)
					{
						case "block":
							return $"*L:* **User:{User}** blocked **{Title}**.";
						case "delete":
							return $"*L:* **User:{User}** deleted the page **{Title}**.";
						case "create":
							return $"*L:* **User:{User}** joined the Wiki!";
						default:
							return $"Unknown logaction: {Logaction}.";
					}
				case "edit":
					string comment = "";
					if (!Comment.Equals(""))
					{
						comment = $"\n>> *{Util.WikiDecode(Comment)}*";
					}
					return $"*E:* **User:{User}** edited the page **{Title}**.{comment}";
				case "new":
					return $"*N:* **User:{User}** created the page **{Title}**.";
				default:
					return $"Unknown type: {Type}";
			}
		}

		public string ToDiscordDescription()
		{
			switch (Type)
			{
				case "log":
					switch (Logaction)
					{
						case "block":
							return $"**L:** {Util.GetDiscordWikiLink($"User:{User}")} blocked **{Util.GetDiscordWikiLink(Title)}**.";
						case "delete":
							return $"**L:** {Util.GetDiscordWikiLink($"User:{User}")} deleted the page **{Util.GetDiscordWikiLink(Title)}**.";
						case "create":
							return $"**L:** {Util.GetDiscordWikiLink($"User:{User}")} joined the Wiki!";
						default:
							return $"Unknown logaction: {Logaction}.";
					}
				case "edit":
					string comment = "";
					if (!Comment.Equals(""))
					{
						comment = $"\n>> *{Util.WikiDecode(Comment)}*";
					}
					return $"**E:** {Util.GetDiscordWikiLink($"User:{User}")} edited the page **{Util.GetDiscordWikiLink(Title)}**.{comment}";
				case "new":
					return $"**N:** {Util.GetDiscordWikiLink($"User:{User}")} created the page **{Util.GetDiscordWikiLink(Title)}**.";
				default:
					return $"Unknown type: {Type}";
			}
		}

		public int CompareTo(WikiChange other)
		{
			return other.Timestamp.CompareTo(Timestamp);
		}
	}
}
