using System;

namespace DiscordRP {
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
}
