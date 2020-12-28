using System;

namespace DiscordRP {
	/// <summary>
	///	Class for custom biomes
	/// </summary>
	public class BiomeStatus {
		/// <value>
		/// function to determine if the player is currently
		/// within the biome. You should probably use
		/// <see cref="active"/> when checking if the biome
		/// is active within normal code.
		/// </value>
		internal Func<bool> checker = null;

		public string largeKey = "biome_placeholder";
		public string largeText = "???";
		public string client = "default";
		public float priority = 0f;

		/// <value>
		/// Whether or not the player is currently in the biome.
		/// This should be used to be somewhat consistent with
		/// the rest of the Terraria API.
		/// </value>
		public bool active => checker();
	}
}
