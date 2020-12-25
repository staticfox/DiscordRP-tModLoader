using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace DiscordRP {
	public class ClientPlayer : ModPlayer {
		internal bool dead = false;
		internal string worldStaticInfo = "";
		internal int nearbyNPC = 0;
		internal bool isMe => player.whoAmI == Main.myPlayer;

		public override void OnEnterWorld(Player player) {
			if (isMe) {
				DiscordRPMod.Instance.inWorld = false;
				string wName = Main.worldName;
				bool expert = Main.expertMode;
				string wDiff = (expert) ? "(Expert)" : "(Normal)";
				DiscordRPMod.Instance.worldStaticInfo = string.Format("Playing {0} {1}", wName, wDiff);
				DiscordRPMod.Instance.ClientUpdatePlayer();
			}
			DiscordRPMod.Instance.UpdateLobbyInfo();
			DiscordRPMod.Instance.ClientForceUpdate();
		}

		public override void PlayerConnect(Player player) {
			if (isMe) {
				DiscordRPMod.Instance.inWorld = true;
			}

			DiscordRPMod.Instance.UpdateLobbyInfo();
			DiscordRPMod.Instance.ClientForceUpdate();
		}

		public override void PlayerDisconnect(Player player) {
			if (isMe) {
				DiscordRPMod.Instance.inWorld = false;
			}

			DiscordRPMod.Instance.UpdateLobbyInfo();
			DiscordRPMod.Instance.ClientForceUpdate();
		}

		public override void PreUpdate() {
			nearbyNPC = Main.npc.Count(npc => npc.active && npc.townNPC && Vector2.DistanceSquared(npc.position, player.Center) <= 2250000f);
		}

		public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource) {
			if (isMe) {
				dead = true;
			}
		}

		public override void OnRespawn(Player player) {
			if (isMe) {
				dead = false;
			}
		}
	}
}
