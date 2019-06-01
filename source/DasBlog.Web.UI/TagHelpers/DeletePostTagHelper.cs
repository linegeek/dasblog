﻿using DasBlog.Services;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Threading.Tasks;

namespace DasBlog.Web.TagHelpers
{
	public class DeletePostTagHelper : TagHelper
	{
		public string BlogPostId { get; set; }

		public string BlogTitle { get; set; }

		private readonly IDasBlogSettings dasBlogSettings;

		public DeletePostTagHelper(IDasBlogSettings dasBlogSettings)
		{
			this.dasBlogSettings = dasBlogSettings;
		}

		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			output.TagName = "a";
			output.TagMode = TagMode.StartTagAndEndTag;
			output.Attributes.SetAttribute("href", $"javascript:deleteEntry(\"{dasBlogSettings.GetPermaLinkUrl(BlogPostId + "/delete")}\",\"{BlogTitle}\")");
			output.Content.SetHtmlContent("Delete this post");
		}

		public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
		{
			return Task.Run(() => Process(context, output));
		}
	}
}
