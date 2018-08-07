using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using ArtifactWikiBot.Wiki;


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

		public static WikiChange[] RecentChanges { get; set; }

		public static double CardDelay { get; set; }
		public static double ChangesDelay { get; set; }

		private static string baseURL = @"https://artifactwiki.com";

		#region Card APIs
		private static string abilityAPI = baseURL +
			@"/api.php?action=cargoquery&format=json&limit=max&tables=Abilities&fields=Title%2C%20Cooldown%2C%20Icon%2C%20Type%2C%20Description";
		private static string heroAPI = baseURL +
			@"/api.php?action=cargoquery&format=json&limit=max&tables=Heroes&fields=Title%2C%20Color%2C%20Attack%2C%20Armor%2C%20Health%2C%20Ability%2C%20Icon%2C%20Image";
		private static string creatureAPI = baseURL +
			@"/api.php?action=cargoquery&format=json&limit=max&tables=Creatures&fields=Title%2C%20Color%2C%20Hero%2C%20Attack%2C%20Armor%2C%20Health%2C%20Image%2C%20Ability";
		private static string spellAPI = baseURL +
			@"/api.php?action=cargoquery&format=json&limit=max&tables=Spells&fields=Title%2C%20Color%2C%20Mana%2C%20Hero%2C%20Description%2C%20Image%2C%20Lane";
		private static string itemAPI = baseURL +
			@"/api.php?action=cargoquery&format=json&limit=max&tables=Items&fields=Title%2C%20Category%2C%20Cost%2C%20Description%2C%20Active%2C%20Image";
		private static string improvementAPI = baseURL +
			@"/api.php?action=cargoquery&format=json&limit=max&tables=Improvements&fields=Title%2C%20Color%2C%20Hero%2C%20Mana%2C%20Reactive%2C%20Icon%2C%20Image%2C%20Lane";
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

			await UpdateAbilities();
			await UpdateHeroes();
			await UpdateItems();
			await UpdateSpells();
			await UpdateCreatures();
			await UpdateImprovements();

			DateTime end = DateTime.Now;
			await BotStates.SetReady();
			return end - start;
		}

		public static async Task<TimeSpan> UpdateAbilities()
		{
			DateTime start = DateTime.Now;
			JToken[] abilities = await LoadCardJson(abilityAPI, "abilities");
			AbilityCard[] abilityList = new AbilityCard[abilities.Length];
			for (int i = 0; i < abilities.Length; i++)
			{
				abilityList[i] = new AbilityCard(abilities[i]);
			}
			abilityList.Sort();
			AbilityCard.List = abilityList;
			DateTime end = DateTime.Now;
			TimeSpan time = end - start;
			Client.DebugLogger.LogMessage(LogLevel.Debug, "APIManager", $"Ability list updated in {time}.", DateTime.Now);
			return time;
		}

		public static async Task<TimeSpan> UpdateHeroes()
		{
			DateTime start = DateTime.Now;
			JToken[] heroes = await LoadCardJson(heroAPI, "heroes");
			HeroCard[] heroList = new HeroCard[heroes.Length];
			for (int i = 0; i < heroes.Length; i++)
			{
				heroList[i] = new HeroCard(heroes[i]);
			}
			heroList.Sort();
			HeroCard.List = heroList;
			DateTime end = DateTime.Now;
			TimeSpan time = end - start;
			Client.DebugLogger.LogMessage(LogLevel.Debug, "APIManager", $"Hero list updated in {time}.", DateTime.Now);
			return time;
		}

		public static async Task<TimeSpan> UpdateCreatures()
		{
			DateTime start = DateTime.Now;
			JToken[] creatures = await LoadCardJson(creatureAPI, "creatures");
			CreatureCard[] creatureList = new CreatureCard[creatures.Length];
			for (int i = 0; i < creatures.Length; i++)
			{
				creatureList[i] = new CreatureCard(creatures[i]);
			}
			creatureList.Sort();
			CreatureCard.List = creatureList;
			DateTime end = DateTime.Now;
			TimeSpan time = end - start;
			Client.DebugLogger.LogMessage(LogLevel.Debug, "APIManager", $"Creature list updated in {time}.", DateTime.Now);
			return time;
		}

		public static async Task<TimeSpan> UpdateSpells()
		{
			Console.WriteLine("Update Spells");
			DateTime start = DateTime.Now;
			JToken[] spells = await LoadCardJson(spellAPI, "spells");
			SpellCard[] spellList = new SpellCard[spells.Length];
			Console.WriteLine("Assign");
			for (int i = 0; i < spells.Length; i++)
			{
				spellList[i] = new SpellCard(spells[i]);
			}
			Console.WriteLine("Sort Spells");
			spellList.Sort();
			Console.WriteLine("Sorted");
			SpellCard.List = spellList;
			DateTime end = DateTime.Now;
			TimeSpan time = end - start;
			Client.DebugLogger.LogMessage(LogLevel.Debug, "APIManager", $"Spell list updated in {time}.", DateTime.Now);
			return time;
		}

		public static async Task<TimeSpan> UpdateImprovements()
		{
			DateTime start = DateTime.Now;
			JToken[] improvements = await LoadCardJson(improvementAPI, "improvements");
			ImprovementCard[] improvementList = new ImprovementCard[improvements.Length];
			for (int i = 0; i < improvements.Length; i++)
			{
				improvementList[i] = new ImprovementCard(improvements[i]);
			}
			improvementList.Sort();
			ImprovementCard.List = improvementList;
			DateTime end = DateTime.Now;
			TimeSpan time = end - start;
			Client.DebugLogger.LogMessage(LogLevel.Debug, "APIManager", $"Improvement list updated in {time}.", DateTime.Now);
			return time;
		}

		public static async Task<TimeSpan> UpdateItems()
		{
			DateTime start = DateTime.Now;
			JToken[] items = await LoadCardJson(itemAPI, "items");
			ItemCard[] itemList = new ItemCard[items.Length];
			for (int i = 0; i < items.Length; i++)
			{
				itemList[i] = new ItemCard(items[i]);
			}
			itemList.Sort();
			ItemCard.List = itemList;
			DateTime end = DateTime.Now;
			TimeSpan time = end - start;
			Client.DebugLogger.LogMessage(LogLevel.Debug, "APIManager", $"Item list updated in {time}.", DateTime.Now);
			return time;
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
