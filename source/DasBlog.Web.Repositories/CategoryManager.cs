﻿using System.IO;
using DasBlog.Managers.Interfaces;
using DasBlog.Services;
using newtelligence.DasBlog.Runtime;

namespace DasBlog.Managers
{
	public class CategoryManager : ICategoryManager
    {
        private readonly IBlogDataService dataService;
        private readonly IDasBlogSettings dasBlogSettings;

        public CategoryManager(IDasBlogSettings settings)
        {
            dasBlogSettings = settings;

			var loggingDataService = LoggingDataServiceFactory.GetService(Path.Combine(dasBlogSettings.WebRootDirectory, dasBlogSettings.SiteConfiguration.LogDir)); ;
			dataService = BlogDataServiceFactory.GetService(Path.Combine(dasBlogSettings.WebRootDirectory, dasBlogSettings.SiteConfiguration.ContentDir), loggingDataService);
		}

        public EntryCollection GetEntries()
        {
            return dataService.GetEntries(false);
        }

        public EntryCollection GetEntries(string category, string acceptLanguages)
        {
			category = category.Replace(dasBlogSettings.SiteConfiguration.TitlePermalinkSpaceReplacement, "+");
            return dataService.GetEntriesForCategory(category, acceptLanguages);
        }

		public string GetCategoryTitle(string categoryurl)
		{
			categoryurl = categoryurl.Replace(dasBlogSettings.SiteConfiguration.TitlePermalinkSpaceReplacement, "+");
			return dataService.GetCategoryTitle(categoryurl);
		}
	}
}
