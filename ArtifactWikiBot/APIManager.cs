using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;


using DSharpPlus;
using Newtonsoft.Json;

namespace ArtifactWikiBot
{
	/// <summary>
	/// The APIManager manages all API calls to the Artifact Wiki and converts them into a usable format.
	/// </summary>
	public class APIManager
	{
		public static DiscordClient Client { get; set; }

		public static JToken[] Heroes { get; set; }
		public static JToken[] Creatures { get; set; }
		public static JToken[] Spells { get; set; }
		public static JToken[] Items { get; set; }
		public static JToken[] Improvements { get; set; }

		public static WikiChange[] RecentChanges { get; set; }

		public static double CardDelay { get; set; }
		public static double ChangesDelay { get; set; }

		private static string baseURL = @"https://artifactwiki.com";

		#region Card APIs
		private static string heroAPI = baseURL +
			@"/api.php?action=cargoquery&format=json&limit=max&tables=Heroes&fields=Title%2C%20Color%2C%20Attack%2C%20Armor%2C%20Health%2C%20Ability%2C%20Icon%2C%20Image";
		private static string creatureAPI = baseURL +
			@"/api.php?action=cargoquery&format=json&limit=max&tables=Creatures&fields=Title%2C%20Color%2C%20Hero%2C%20Attack%2C%20Armor%2C%20Health%2C%20Image";
		private static string spellAPI = baseURL +
			@"/api.php?action=cargoquery&format=json&limit=max&tables=Spells&fields=Title%2C%20Color%2C%20Mana%2C%20Hero%2C%20Description%2C%20Image";
		private static string itemAPI = baseURL +
			@"/api.php?action=cargoquery&format=json&limit=max&tables=Items&fields=Title%2C%20Category%2C%20Cost%2C%20Description%2C%20Active%2C%20Image";
		private static string improvementAPI = baseURL +
			@"/api.php?action=cargoquery&format=json&limit=max&tables=Improvements&fields=Title%2C%20Color%2C%20Hero%2C%20Mana%2C%20Reactive%2C%20Icon%2C%20Image";
		#endregion

		private static string changesAPI = baseURL +
			@"/api.php?action=query&list=recentchanges&rcprop=title|ids|sizes|flags|user|parsedcomment|loginfo&rclimit=10&format=json";


		// Initialize Json Files
		public static async Task Init(DiscordClient client, int cardDelay, int changesDelay)
		{
			Client = client;
			CardDelay = cardDelay;
			ChangesDelay = changesDelay;
			await UpdateAllCards();
			await UpdateRecentChanges();
		}

		public static async Task ChangesUpdater()
		{
			Int32 delay = (Int32)ChangesDelay * 1000 * 60;
			while (true)
			{
				Thread.Sleep(delay);
				await UpdateRecentChanges();
			}
		}

		public static async Task<TimeSpan> UpdateRecentChanges()
		{
			await BotStates.SetUpdating();
			DateTime start = DateTime.Now;

			using (var client = new System.Net.WebClient())
			{
				client.DownloadFile(changesAPI, "changes_download.json");
			}

			string json = "";
			using (var fs = File.OpenRead("changes_download.json"))
			using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
				json = await sr.ReadToEndAsync();

			JObject o = JObject.Parse(json);

			JToken[] _changes = o["query"]["recentchanges"].Children().ToArray();

			WikiChange[] changes = new WikiChange[_changes.Length];

			for (int i = 0; i < changes.Length; i++)
			{
				changes[i] = JsonConvert.DeserializeObject<WikiChange>(_changes[i].ToString());
			}

			RecentChanges = changes;
			Client.DebugLogger.LogMessage(LogLevel.Debug, "JsonManager", "'changes.json' updated.", DateTime.Now);
			DateTime end = DateTime.Now;
			await BotStates.SetReady();
			return end - start;
		}

		#region Card Updates
		// Update the Card Json Files
		public static async Task CardUpdater()
		{
			Int32 delay = (Int32)CardDelay * 1000 * 60;
			while(true)
			{
				Thread.Sleep(delay);
				await UpdateAllCards();
			}
		}

		public static async Task<TimeSpan> UpdateAllCards()
		{
			await BotStates.SetUpdating();
			DateTime start = DateTime.Now;

			await UpdateHeroes();
			await UpdateItems();
			await UpdateSpells();
			await UpdateCreatures();
			await UpdateImprovements();

			DateTime end = DateTime.Now;
			await BotStates.SetReady();
			return end - start;
		}

		public static async Task<JToken[]> UpdateHeroes()
		{
			JToken[] heroes = await LoadCardJson(heroAPI, "heroes");
			Heroes = heroes;
			Client.DebugLogger.LogMessage(LogLevel.Debug, "JsonManager", "'heroes.json' updated.", DateTime.Now);
			return heroes;
		}

		public static async Task<JToken[]> UpdateCreatures()
		{
			JToken[] creatures = await LoadCardJson(creatureAPI, "creatures");
			Creatures = creatures;
			Client.DebugLogger.LogMessage(LogLevel.Debug, "JsonManager", "'creatures.json' updated.", DateTime.Now);
			return creatures;
		}

		public static async Task<JToken[]> UpdateSpells()
		{
			JToken[] spells = await LoadCardJson(spellAPI, "spells");
			Spells = spells;
			Client.DebugLogger.LogMessage(LogLevel.Debug, "JsonManager", "'spells.json' updated.", DateTime.Now);
			return spells;
		}

		public static async Task<JToken[]> UpdateImprovements()
		{
			JToken[] improvements = await LoadCardJson(improvementAPI, "improvements");
			Improvements = improvements;
			Client.DebugLogger.LogMessage(LogLevel.Debug, "JsonManager", "'improvements.json' updated.", DateTime.Now);
			return improvements;
		}

		public static async Task<JToken[]> UpdateItems()
		{
			JToken[] items = await LoadCardJson(itemAPI, "items");
			Items = items;
			Client.DebugLogger.LogMessage(LogLevel.Debug, "JsonManager", "'items.json' updated.", DateTime.Now);
			return items;
		}

		public static async Task<JToken[]> LoadCardJson(string api, string name)
		{
			// Download json
			using (var client = new System.Net.WebClient())
			{
				client.DownloadFile(api, $"{name}_download.json");
			}

			string json = "";
			using (var fs = File.OpenRead($"{name}_download.json"))
			using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
				json = await sr.ReadToEndAsync();

			// Create Json object
			JObject o = JObject.Parse(json);

			// Create JToken array
			JToken[] array = o["cargoquery"].Children().ToArray();

			// Normalize Array
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = array[i]["title"];
			}

			return array;
		}
		#endregion
	}

	public class WikiChange
	{
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
			switch(Type)
			{
				case "log":
					switch(Logaction)
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
					if(!Comment.Equals(""))
					{
						comment = $"\n*{Comment}*";
					}
					return $"*E:* **User:{User}** edited the page **{Title}**.{comment}";
				case "new":
					return $"*N:* **User:{User}** created the page **{Title}**.";
				default:
					return $"Unknown type: {Type}";
			}
		}
	}
}
