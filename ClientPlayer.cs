using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace DiscordRP {
	public class ClientPlayer : ModPlayer {
		internal bool dead = false;
		internal int nearbyNPC = 0;

		public override void OnEnterWorld(Player pPlayer) {
			if (pPlayer.whoAmI == Main.myPlayer) {
				DiscordRPMod.Instance.inWorld = true;

				DiscordRPMod.Instance.UpdateWorldStaticInfo();
				DiscordRPMod.Instance.ClientUpdatePlayer();
			}

			DiscordRPMod.Instance.UpdateLobbyInfo();
			DiscordRPMod.Instance.ClientForceUpdate();
		}

		public override void PlayerConnect(Player pPlayer) {
			if (pPlayer.whoAmI == Main.myPlayer) {
				DiscordRPMod.Instance.inWorld = true;
			}

			DiscordRPMod.Instance.UpdateLobbyInfo();
			DiscordRPMod.Instance.ClientForceUpdate();
		}

		public override void PlayerDisconnect(Player pPlayer) {
			if (pPlayer.whoAmI == Main.myPlayer) {
				DiscordRPMod.Instance.inWorld = false;
			}

			DiscordRPMod.Instance.UpdateLobbyInfo();
			DiscordRPMod.Instance.ClientForceUpdate();
		}

		public override void PreUpdate() {
			nearbyNPC = Main.npc.Count(npc => npc.active && npc.townNPC && Vector2.DistanceSquared(npc.position, player.Center) <= 2250000f);
		}

		public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource) {
			if (player.whoAmI == Main.myPlayer) {
				dead = true;
			}
		}

		public override void OnRespawn(Player pPlayer) {
			if (pPlayer.whoAmI == Main.myPlayer) {
				dead = false;
			}
		}

		public Tile CurrentTile() {
			Player me = Main.LocalPlayer;

			int xpos = (int)(me.Center.X / 16f);
			int ypos = (int)(me.Center.Y / 16f);
			return Main.tile[xpos, ypos];
		}
	}
}
