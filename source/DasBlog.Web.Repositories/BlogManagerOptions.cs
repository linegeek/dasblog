namespace DasBlog.Managers
{
	public class PingServices
	{
		
	}
	/// <summary>
	/// loaded from site.config file
	/// </summary>
	public class BlogManagerOptions
	{
		public string ContentDir { get; set; }
		public bool EnableAutoPingback { get; set; }
		public bool EnableTitlePermaLinkUnique { get; set; }
		public string LogDir { get; set; }
		public string Root { get; set; }
		public string Title { get; set; }
		public string TitlePermalinkSpaceReplacement { get; set; }
	}
	public class BlogManagerModifiableOptions
	{
		public bool AdjustDisplayTimeZone { get; set; }
		public int ContentLookaheadDays { get; set; }
		public string CrossPostFooter { get; set; }
		public int DaysCommentsAllowed { get; set; }
		public decimal DisplayTimeZoneIndex { get; set; }
		public bool EnableCommentDays { get; set; }
		public bool EnableComments { get; set; }
		public bool EnableCrossPostFooter { get; set; }
		public int EntriesPerPage { get; set; }
		public int FrontPageEntryCount { get; set; }

		//public object PingServices { get; set; }
				// currently hardcoded in BlogManager - will be sorted when strategy is clear

		public int RssDayCount { get; set; }
		public int RssEntryCount { get; set; }
	}
	/// <summary>
	/// options not loaded from site.config
	/// </summary>
	public class BlogManagerExtraOptions
	{
		/// <summary>
		/// avoiding passing IHostingEnvironment (whose ContentRootPath is operative)
		/// so that we don't create an unnecessary dependency between DasBlog.Managers and ASP.NET
		/// </summary>
		public string ContentRootPath { get; set; }
	}
}
