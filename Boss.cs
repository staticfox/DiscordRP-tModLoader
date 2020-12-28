namespace DiscordRP {
	/// <summary>
	/// Represents an NPC that is presentable through
	/// Discord Rich Presence.
	/// </summary>
	public class Boss {
		/// <value>
		/// name of the uploaded image for the large profile artwork
		/// </value>
		public string imageKey;

		/// <value>
		/// tooltip for the imageKey
		/// </value>
		public string imageName;

		/// <value>
		/// floating point value to allow for boss precedence.
		/// the higher the value, the higher the precedence.
		/// </value>
		public float priority;

		/// <value>
		/// DiscordRP instance linked to this boss
		/// </value>
		public string clientId;
	}
}
