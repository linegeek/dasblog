﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DasBlog.Services;
using DasBlog.Web.Models.BlogViewModels;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace DasBlog.Web.TagHelpers.Comments
{
	public class CommentDeleteLinkTagHelper : TagHelper
	{
		public CommentViewModel Comment { get; set; }

		private readonly IDasBlogSettings dasBlogSettings;
		private const string COMMENTDELETE_URL = "{0}/comments/{1}";
		private const string COMMENTTEXT_MSG = "Are you sure you want to delete the comment from '{0}'?";

		public CommentDeleteLinkTagHelper(IDasBlogSettings dasBlogSettings) 
		{
			this.dasBlogSettings = dasBlogSettings;
		}

		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			var deleteurl = string.Format(COMMENTDELETE_URL, dasBlogSettings.GetPermaLinkUrl(Comment.BlogPostId), Comment.CommentId);
			var commenttxt = string.Format(COMMENTTEXT_MSG, Comment.Name);

			output.TagName = "a";
			output.TagMode = TagMode.StartTagAndEndTag;
			output.Attributes.SetAttribute("href", $"javascript:commentManagement(\"{deleteurl}\",\"{commenttxt}\",\"DELETE\")");
			output.Attributes.SetAttribute("class", "dbc-comment-delete-link");
			output.Content.SetHtmlContent("Delete this comment");
		}

		public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
		{
			return Task.Run(() => Process(context, output));
		}

	}
}
