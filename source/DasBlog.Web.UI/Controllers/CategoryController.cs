﻿using AutoMapper;
using DasBlog.Core;
using DasBlog.Managers.Interfaces;
using DasBlog.Web.Models.BlogViewModels;
using DasBlog.Web.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DasBlog.Web.Controllers
{
	public class CategoryController : DasBlogBaseController
	{
		private readonly ICategoryManager categoryManager;
		private readonly IHttpContextAccessor httpContextAccessor;
		private readonly IDasBlogSettings dasBlogSettings;
		private readonly IMapper mapper;

		public CategoryController(ICategoryManager categoryManager, IDasBlogSettings settings, IHttpContextAccessor httpContextAccessor, IMapper mapper)
			: base(settings)
		{
			this.categoryManager = categoryManager;
			dasBlogSettings = settings;
			this.httpContextAccessor = httpContextAccessor;
			this.mapper = mapper;
		}

		[HttpGet("category")]
		public IActionResult Category()
		{
			var viewModel = GetCategoryListFromCategoryManager(string.Empty);
			return View(viewModel);
		}

		[HttpGet("category/{cat}")]
		public IActionResult Category(string cat)
		{
			var viewModel = GetCategoryListFromCategoryManager(cat);
			return View(viewModel);
		}

		private CategoryListViewModel GetCategoryListFromCategoryManager(string category)
		{
			var entries = !string.IsNullOrEmpty(category)
				? categoryManager.GetEntries(category, httpContextAccessor.HttpContext.Request.Headers["Accept-Language"])
				: categoryManager.GetEntries();

			var viewModel = CategoryListViewModel.Create(entries);
			return viewModel;
		}
	}
}
