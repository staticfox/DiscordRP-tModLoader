using System;
using System.Collections.Generic;
using DiscordRPC;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DiscordRP {

	public class DiscordRPMod : Mod {

		//Mod Helper Issues report
		public static string GithubUserName => "PurplefinNeptuna";
		public static string GithubProjectName => "DiscordRP-tModLoader";

		public static DiscordRPMod Instance = null;

		public string currentClient = "default";

		internal DiscordRpcClient Client {
			get; set;
		}

		internal RichPresence RichPresenceInstance {
			get; private set;
		}

		internal int prevSend = 0;
		internal bool inWorld = false;
		internal bool canCreateClient;

		internal ClientConfig config = null;

		internal Timestamps timestamp = null;

		internal Dictionary<int, Boss> exBossIDtoDetails = new Dictionary<int, Boss>();

		internal DRPStatus customStatus = null;

		internal List<BiomeStatus> exBiomeStatus = new List<BiomeStatus>();

		internal string worldStaticInfo = null;

		//internal static Dictionary<string, DiscordRpcClient> discordRPCs;
		internal Dictionary<string, string> savedDiscordAppId;

		internal int nowSeconds => (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;

		internal ClientPlayer modPlayer => Main.LocalPlayer.GetModPlayer<ClientPlayer>();

		public void addBoss(int bossId, Boss boss) {
			exBossIDtoDetails.Add(bossId, boss);
		}

		private bool bossExists(int bossId) {
			return exBossIDtoDetails.ContainsKey(bossId);
		}

		private Boss getBossById(int bossId) {
			return exBossIDtoDetails[bossId];
		}

		private Boss getCurrentBoss() {
			Boss currentBoss = null;

			foreach (Terraria.NPC npc in Main.npc) {
				if (!npc.active || !bossExists(npc.type))
					continue;

				Boss boss = getBossById(npc.type);
				if (currentBoss != null && currentBoss.priority > boss.priority)
					continue;

				currentBoss = boss;
			}

			return currentBoss;
		}

		public void addBiome(BiomeStatus biome) {
			exBiomeStatus.Add(biome);
		}

		private BiomeStatus getCurrentBiome() {
			BiomeStatus currentBiome = null;

			foreach (BiomeStatus biome in exBiomeStatus) {
				if (!biome.active)
					continue;

				if (currentBiome != null && currentBiome.priority > biome.priority)
					continue;

				currentBiome = biome;
			}

			return currentBiome;
		}

		private string getTimeOfDay() {
			if (!config.showTimeCycle)
				return null;

			// Notes on the time system in Terraria - 'time'
			// is referred to as the amount of seconds that
			// have passed since the day transitioned to night
			// or vice-versa. The key is to check Main.dayTime
			// to determine whether 0.0 is 4:30 AM (day) or
			// 7:30 PM (night).
			//
			// 1 hour   = 3600
			// 1 minute = 60
			// 1 second = 1
			//
			// 15 hours of day
			//
			// Day start = 4:30 AM
			// dayLength = 54000.0
			//
			// 9 hours of night
			//
			// Night start = 7:30 PM
			// nightLength = 32400.0
			if (Main.dayTime) {
				// We'll consider 6:00 AM as the time when
				// day "officially" starts and 6:00 PM as
				// the time when day winds down. Partially
				// going off of IRL parallels and partially
				// because peak fishing ends at
				// 6:00 AM and the merchant will always
				// leave at 6pm.
				if (Main.time < 7200.0) {
					return "Dawn";
				} else if (Main.time >= 46800.0) {
					return "Dusk";
				} else {
					return "Day";
				}
			} else {
				// The vast majority of checks are in the
				// day time since it's completely dark in
				// Terraria for the entire duration of the
				// night.
				return "Night";
			}
		}

		private string getPlayerState() {
			if (!config.ShowPlayerStats())
				return null;

			if (modPlayer.dead)
				return "Dead";

			string state = "";

			if (config.showHealth)
				state += $"HP: {Main.LocalPlayer.statLife} ";

			if (config.showDPS)
				state += $"DPS: {Main.LocalPlayer.getDPS()} ";

			if (config.showMana)
				state += $"MP: {Main.LocalPlayer.statMana} ";

			if (config.showDefense)
				state += $"DEF: {Main.LocalPlayer.statDefense} ";

			return state.Trim();
		}

		public DiscordRPMod() {
			Properties = new ModProperties() {
				Autoload = true,
				AutoloadBackgrounds = true,
				AutoloadGores = true,
				AutoloadSounds = true
			};
			Instance = this;
		}

		public override void Load() {
			if (!Main.dedServ) {
				currentClient = "default";
				canCreateClient = true;
				exBiomeStatus = new List<BiomeStatus>();
				exBossIDtoDetails = new Dictionary<int, Boss>();

				savedDiscordAppId = new Dictionary<string, string>();

				RichPresenceInstance = new RichPresence {
					Secrets = new Secrets()
				};

				timestamp = Timestamps.Now;

				CreateNewDiscordRPCRichPresenceInstance();
				//CreateNewDiscordRPCRichPresenceInstance("716207249902796810", "angryslimey");
				//AddDiscordAppID("angryslimey", "716207249902796810");
			}
		}

		public override void AddRecipes() {
			if (!Main.dedServ) {
				DRPX.AddVanillaBosses();
				DRPX.AddVanillaBiomes();
				DRPX.AddVanillaEvents();

				Main.OnTickForThirdPartySoftwareOnly += ClientUpdate;
				//finished
				canCreateClient = false;
				ClientOnMainMenu();
			}
		}

		/// <summary>
		/// Change the Discord App ID, currently takes 3s to change
		/// </summary>
		/// <param name="newClient">New Discord App ID key</param>
		public void ChangeDiscordClient(string newClient) {
			if (newClient == currentClient) {
				return;
			}
			if (!savedDiscordAppId.ContainsKey(newClient)) {
				return;
			}
			currentClient = newClient;
			// if (Client.ApplicationID != savedDiscordAppId[newClient]) {
			// 	Client.ApplicationID = savedDiscordAppId[newClient];
			// }
		}

		/// <summary>
		/// Create new DiscordRP client, currently only used once
		/// </summary>
		/// <param name="appId">Discord App ID</param>
		/// <param name="key">key for App ID</param>
		internal void CreateNewDiscordRPCRichPresenceInstance(string key = "default") {
			// https://github.com/PurplefinNeptuna
			// string discordAppId = "404654478072086529";

			// The images tied to this app were copied from the
			// original author's Discord app - I made a new app
			// purely so I could continue to add images to the
			// project based off the content added in 1.4.
			// Social anxiety really sucks.
			// https://github.com/staticfox
			string discordAppId = "792583749040209960";

			// This should never change
			const string steamAppID = "1281930";

			if (!savedDiscordAppId.ContainsKey(key)) {
				savedDiscordAppId.Add(key, discordAppId);
			}

			Client = new DiscordRpcClient(applicationID: discordAppId, autoEvents: false);

			bool failedToRegisterScheme = false;

			try {
				Client.RegisterUriScheme(steamAppID);
			}
			catch (Exception) {
				failedToRegisterScheme = true;
			}

			if (!failedToRegisterScheme) {
				Client.OnJoinRequested += ClientOnJoinRequested;
				Client.OnJoin += ClientOnJoin;
			}

			if (config.enable) {
				Client.Initialize();
			}
		}

		/// <summary>
		/// Add other Discord App ID
		/// </summary>
		/// <param name="key">the key</param>
		/// <param name="appID">Discord App ID</param>
		public void AddDiscordAppID(string key, string appID) {
			if (!savedDiscordAppId.ContainsKey(key)) {
				savedDiscordAppId.Add(key, appID);
			}
		}

		/// <summary>
		/// Discord OnJoin event, called on the joiner
		/// </summary>
		private void ClientOnJoin(object sender, DiscordRPC.Message.JoinMessage args) {
			//this is empty lol
			//SocialAPI.Network.Connect(new SteamAddress(new CSteamID(Convert.ToUInt64(args.Secret))));
		}

		/// <summary>
		/// Discord OnJoinRequested event, called on the host, currently deny everything lol
		/// </summary>
		private void ClientOnJoinRequested(object sender, DiscordRPC.Message.JoinRequestMessage args) {
			Client.Respond(args, false);
		}

		/// <summary>
		/// Change the status to main menu
		/// </summary>
		private void ClientOnMainMenu() {
			ChangeDiscordClient("default");
			if (customStatus == null) {
				ClientSetStatus("", "In Main Menu", "payload_test", "tModLoader");
			}
			else {
				ClientSetStatus(customStatus.GetState(), customStatus.GetDetails(),
				customStatus.largeKey, customStatus.largeImage,
				customStatus.smallKey, customStatus.smallImage);
			}

			ClientSetParty();
			ClientForceUpdate();
		}

		/// <summary>
		///	override this because i can only find this method that called when going to main menu
		/// </summary>
		public override void PreSaveAndQuit() {
			if (!Main.dedServ) {
				ClientOnMainMenu();
			}
		}

		/// <summary>
		/// Change the status
		/// </summary>
		/// <param name="state">lower status string</param>
		/// <param name="details">upper status string</param>
		/// <param name="largeImageKey">key for large image</param>
		/// <param name="largeImageText">text for large image</param>
		/// <param name="smallImageKey">key for small image</param>
		/// <param name="smallImageText">text for small image</param>
		public void ClientSetStatus(string state = "", string details = "", string largeImageKey = null, string largeImageText = null, string smallImageKey = null, string smallImageText = null) {
			RichPresenceInstance.Assets = RichPresenceInstance.Assets ?? new Assets();
			RichPresenceInstance.State = state;
			RichPresenceInstance.Details = details;
			if (largeImageKey == null) {
				RichPresenceInstance.Assets.LargeImageKey = null;
				RichPresenceInstance.Assets.LargeImageText = null;
			}
			else {
				RichPresenceInstance.Assets.LargeImageKey = largeImageKey;
				RichPresenceInstance.Assets.LargeImageText = largeImageText;
			}

			if (smallImageKey == null) {
				RichPresenceInstance.Assets.SmallImageKey = null;
				RichPresenceInstance.Assets.SmallImageText = null;
			}
			else {
				RichPresenceInstance.Assets.SmallImageKey = smallImageKey;
				RichPresenceInstance.Assets.SmallImageText = smallImageText;
			}
		}

		/// <summary>
		/// set the party settings
		/// </summary>
		/// <param name="secret">party secret</param>
		/// <param name="id">party id</param>
		/// <param name="partysize">party current size</param>
		public void ClientSetParty(string secret = null, string id = null, int partysize = 0) {
			if (partysize == 0 || id == null) {
				RichPresenceInstance.Secrets.JoinSecret = null;
				RichPresenceInstance.Party = null;
			}
			else {
				//RichPresenceInstance.Secrets.JoinSecret = secret;
				//RichPresenceInstance.Party = RichPresenceInstance.Party ?? new Party();
				//RichPresenceInstance.Party.Size = partysize;
				//RichPresenceInstance.Party.Max = 256;
				//RichPresenceInstance.Party.ID = id;
				RichPresenceInstance.Secrets.JoinSecret = null;
				RichPresenceInstance.Party = null;
			}
		}

		/// <summary>
		/// Forcing update rich presence
		/// </summary>
		public void ClientForceUpdate() {
			if (Client != null && !Client.IsDisposed) {
				if (!Client.IsInitialized && config.enable) {
					Client.Initialize();
				}
				RichPresenceInstance.Timestamps = config.showTime ? timestamp : null;
				Client.SetPresence(RichPresenceInstance);
				Client.Invoke();
			}
		}

		/// <summary>
		///	run this everytick to update
		/// </summary>
		public void ClientUpdate() {
			if (!Main.gameMenu && !Main.dedServ) {
				if (Main.gamePaused || Main.gameInactive || !inWorld) {
					return;
				}

				int now = nowSeconds;
				if (now != prevSend && ((now - prevSend) % config.timer) == 0) {
					ClientUpdatePlayer();
					ClientForceUpdate();

					prevSend = now;
				}
			}
		}

		public override void Unload() {
			Main.OnTickForThirdPartySoftwareOnly -= ClientUpdate;
			Client?.Dispose();

			Instance = null;
			config = null;
		}

		public override object Call(params object[] args) {
			if (!Main.dedServ) {
				return DRPX.Call(args);
			}
			return "Can't call on server";
		}

		/// <summary>
		/// update the party info
		/// </summary>
		internal void UpdateLobbyInfo() {
			if (Main.LobbyId != 0UL) {
				//string sId = SteamUser.GetSteamID().ToString();
				ClientSetParty(null, Main.LocalPlayer.name, Main.CurrentFrameFlags.ActivePlayersCount);
			}
		}

		/// <summary>
		/// method for update the status, checking from item to biome/boss/events
		/// </summary>
		internal void ClientUpdatePlayer() {
			if (Main.LocalPlayer == null)
				return;

			string largeImageKey = null;
			string largeImageText = null;
			string selectedClient = "default";

			(string itemKey, string itemText) = GetItemStat();

			Boss boss = getCurrentBoss();
			BiomeStatus biome = getCurrentBiome();

			if (boss != null) {
				largeImageKey = boss.imageKey;
				largeImageText = "Fighting " + boss.imageName;
				selectedClient = boss.clientId;
			} else if (biome != null) {
				largeImageKey = biome.largeKey;
				largeImageText = "In " + biome.largeText;
				selectedClient = biome.client;

				string timeOfDay = getTimeOfDay();
				if (timeOfDay != null)
					largeImageText += $" ({timeOfDay})";
			}

			string state = getPlayerState();
			ClientSetStatus(state, largeImageText, largeImageKey, worldStaticInfo, itemKey, itemText);
			UpdateLobbyInfo();
			ChangeDiscordClient(selectedClient);

			if (modPlayer.dead)
				ClientForceUpdate();
		}

		/// <summary>
		/// Get the player's item stat
		/// </summary>
		/// <returns>key and text for small images</returns>
		internal (string, string) GetItemStat() {
			int atk = -1;
			string key = null;
			string text = null;
			string atkType = "";
			Item item = Main.LocalPlayer?.HeldItem;

			List<(DamageClass, string)> DamageClasses = new List<(DamageClass, string)>() {
				(DamageClass.Melee, "Melee"),
				(DamageClass.Ranged, "Range"),
				(DamageClass.Magic, "Magic"),
				(DamageClass.Throwing, "Throw"),
				(DamageClass.Summon, "Summon"),
			};

			if (item != null) {
				text = "";

				if (config.showPrefix && item.prefix != 0 && item.prefix < PrefixID.Count) {
					string prefix = PrefixID.Search.GetName(item.prefix);
					text += prefix + " ";
				}

				text += item.Name;

				foreach (var tuple in DamageClasses) {
					DamageClass damageClass = tuple.Item1;
					string damageName = tuple.Item2;

					if (item.DamageType == damageClass) {
						atk = (int) Math.Ceiling((float)item.damage * (float)Main.LocalPlayer.GetDamage(damageClass));
						atkType = damageName;
						break;
					}
				}
			}

			if (atk >= 0) {
				key = "atk_" + atkType.ToLower();

				if (config.showDamage)
					text += $" ({atk} Damage)";
			}
			return (key, text);
		}

		public void UpdateWorldStaticInfo() {
			string wName = Main.worldName;
			string wDiff = Main.masterMode ? "(Master)" : Main.expertMode ? "(Expert)" : "(Normal)";

			if (!config.showWorldName) {
				if (Main.netMode == NetmodeID.SinglePlayer) {
					wName = "Single Player";
				} else {
					wName = "Multiplayer";
				}
			}

			worldStaticInfo = string.Format("Playing {0} {1}", wName, wDiff);
		}
	}
}
