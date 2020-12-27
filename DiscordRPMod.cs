using System;
using System.Collections.Generic;
using DiscordRPC;
using static DiscordRP.Boss;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DiscordRP {

	/// <summary>
	/// Rich Presence Status class, currently only used for custom main menu
	/// </summary>
	internal class DRPStatus {
		public string details = "", additionalDetails = "";
		public string largeKey = "", largeImage = "";
		public string smallKey = "", smallImage = "";
		public string GetState() => additionalDetails;
		public string GetDetails() => details;
	}

	/// <summary>
	///	Class for custom biomes
	/// </summary>
	internal class BiomeStatus {
		public Func<bool> checker = null;
		public string largeKey = "biome_placeholder";
		public string largeText = "???";
		public string client = "default";
		public float priority = 0f;
	}

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

		internal Dictionary<int, DiscordRP.Boss> exBossIDtoDetails = new Dictionary<int, DiscordRP.Boss>();

		internal DRPStatus customStatus = null;

		internal List<BiomeStatus> exBiomeStatus = new List<BiomeStatus>();

		internal string worldStaticInfo = null;

		//internal static Dictionary<string, DiscordRpcClient> discordRPCs;
		internal Dictionary<string, string> savedDiscordAppId;

		internal int nowSeconds => (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;

		internal ClientPlayer modPlayer => Main.LocalPlayer.GetModPlayer<ClientPlayer>();

		public void addBoss(int bossId, DiscordRP.Boss boss) {
			Instance.exBossIDtoDetails.Add(bossId, boss);
		}

		public bool bossExists(int bossId) {
			return Instance.exBossIDtoDetails.ContainsKey(bossId);
		}

		public DiscordRP.Boss getBossById(int bossId) {
			return Instance.exBossIDtoDetails[bossId];
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
				exBossIDtoDetails = new Dictionary<int, DiscordRP.Boss>();

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

			(string itemKey, string itemText) = GetItemStat();
			(string bigKey, string bigText, string selectedClient) = DRPX.GetBoss();

			string state = null;
			if (!modPlayer.dead && config.ShowPlayerStats()) {
				state = "";

				if (config.showHealth)
					state += $"HP: {Main.LocalPlayer.statLife} ";

				if (config.showDPS)
					state += $"DPS: {Main.LocalPlayer.getDPS()} ";

				if (config.showMana)
					state += $"MP: {Main.LocalPlayer.statMana} ";

				if (config.showDefense)
					state += $"DEF: {Main.LocalPlayer.statDefense} ";

				state = state.Trim();
			} else if (modPlayer.dead && config.ShowPlayerStats()) {
				state = "Dead";
			}

			ClientSetStatus(state, bigText, bigKey, worldStaticInfo, itemKey, itemText);
			UpdateLobbyInfo();
			Instance.ChangeDiscordClient(selectedClient);

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
