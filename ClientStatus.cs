namespace DiscordRP {
	/// <summary>
	/// Defines the required fields when updating
	/// the client's Rich Presence status.
	/// </summary>
	public class ClientStatus {
		/// <value>
		/// the user's current party status
		/// </value>
		public string state = "";

		/// <value>
		/// what the player is currently doing
		/// </value>
		public string details = "";

		/// <value>
		/// name of the uploaded image for the large profile artwork
		/// </value>
		public string largeImageKey = null;

		/// <value>
		/// tooltip for the largeImageKey
		/// </value>
		public string largeImageText = null;

		/// <value>
		/// name of the uploaded image for the small profile artwork
		/// </value>
		public string smallImageKey = null;

		/// <value>
		/// tooltip for the smallImageKey
		/// </value>
		public string smallImageText = null;
	}
}
