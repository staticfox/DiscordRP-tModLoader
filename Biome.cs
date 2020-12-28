using System;

namespace DiscordRP {
	/// <summary>
	/// Class for custom biomes
	/// </summary>
	public class Biome {
		/// <value>
		/// function to determine if the player is currently
		/// within the biome. You should probably use
		/// <see cref="active"/> when checking if the biome
		/// is active within normal code.
		/// </value>
		internal Func<bool> checker = null;

		/// <value>
		/// name of the uploaded image for the large profile artwork
		/// </value>
		public string imageKey = "biome_placeholder";

		/// <value>
		/// tooltip for the imageKey
		/// </value>
		public string imageName = "???";

		/// <value>
		/// floating point value to allow for boss precedence.
		/// the higher the value, the higher the precedence.
		/// </value>
		public float priority = 0f;

		/// <value>
		/// DiscordRP instance linked to this boss
		/// </value>
		public string clientId = "default";

		/// <value>
		/// Whether or not the player is currently in the biome.
		/// This should be used to be somewhat consistent with
		/// the rest of the Terraria API.
		/// </value>
		public bool active => checker();
	}
}
