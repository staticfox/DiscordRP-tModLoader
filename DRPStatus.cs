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
}
