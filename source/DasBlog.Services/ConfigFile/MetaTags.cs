﻿using System;
using System.Collections.Generic;
using System.Text;
using DasBlog.Services.ConfigFile.Interfaces;

namespace DasBlog.Services.ConfigFile
{
    public class MetaTags : IMetaTags
    {
        public string MetaDescription { get; set; }
        public string MetaKeywords  { get; set; }
        public string TwitterCard  { get; set; }
        public string TwitterSite  { get; set; }
        public string TwitterCreator  { get; set; }
        public string TwitterImage  { get; set; }
        public string FaceBookAdmins  { get; set; }
        public string FaceBookAppID  { get; set; }
		public string GoogleAnalyticsID { get; set; }
	}
}
