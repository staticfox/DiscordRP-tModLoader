using Terraria.ID;
using Terraria.ModLoader;

namespace DiscordRP {
	public class ClientWorld : ModWorld {

		public static bool cloud;
		public static bool dirt;

		public override void TileCountsAvailable(int[] tileCounts) {
			cloud = (tileCounts[TileID.Cloud] + tileCounts[TileID.RainCloud]) > 40;
			dirt = tileCounts[TileID.Dirt] > 20;
		}
	}
}
