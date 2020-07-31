﻿using DasBlog.Services;
using DasBlog.Web.Models.BlogViewModels;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DasBlog.Web.TagHelpers.Post
{
	public class PostCategoriesListTagHelper : TagHelper
	{
		public IList<CategoryViewModel> Categories { get; set; }

		public PostViewModel Post { get; set; }

		private readonly IDasBlogSettings dasBlogSettings;
		private const string CATEGORY_ITEM_TEMPLATE = "<a href='{0}' class='dbc-a-category'>{1}</a>";

		public PostCategoriesListTagHelper(IDasBlogSettings dasBlogSettings)
		{
			this.dasBlogSettings = dasBlogSettings;
		}

		public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
		{
			if (Post != null)
			{
				Categories = Post.Categories;
			}

			var categorylist = string.Empty;
			output.TagName = "span";
			output.TagMode = TagMode.StartTagAndEndTag;
			output.Attributes.SetAttribute("class", "dbc-span-category");

			var content = await output.GetChildContentAsync();
			var format = content.GetContent();

			if (string.IsNullOrWhiteSpace(format))
			{
				format = " ";
			}

			foreach (var category in Categories)
			{
				categorylist = categorylist + string.Format(CATEGORY_ITEM_TEMPLATE, dasBlogSettings.GetCategoryViewUrl(category.CategoryUrl), category.Category) + format;
			}

			if (!string.IsNullOrWhiteSpace(categorylist))
			{
				categorylist = categorylist.Remove(categorylist.Length - format.Length);
			}

			output.Content.SetHtmlContent(categorylist);
		}
	}
}
