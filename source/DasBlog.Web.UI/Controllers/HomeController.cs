﻿using System;
using System.Diagnostics;
using System.Linq;
using AutoMapper;
using DasBlog.Core;
using DasBlog.Managers.Interfaces;
using DasBlog.Web.Models;
using DasBlog.Web.Models.BlogViewModels;
using DasBlog.Web.Settings;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DasBlog.Web.Controllers
{
	public class HomeController : DasBlogBaseController
	{
		private readonly IBlogManager blogManager;
		private readonly IDasBlogSettings dasBlogSettings;
		private readonly IMapper mapper;
		private readonly ILogger<HomeController> logger;
		
		public HomeController(IBlogManager blogManager, IDasBlogSettings settings, IXmlRpcManager rpcManager, 
							IMapper mapper, ILogger<HomeController> logger) : base(settings)
		{
			this.blogManager = blogManager;
			dasBlogSettings = settings;
			this.mapper = mapper;
			this.logger = logger;
		}

		public IActionResult Index()
		{
			var lpvm = new ListPostsViewModel
			{
				Posts = blogManager.GetFrontPagePosts(Request.Headers["Accept-Language"])
							.Select(entry => mapper.Map<PostViewModel>(entry)).
							Select(editentry => editentry).ToList()
			};

			logger.LogDebug($"In Index - {lpvm.Posts.Count} post found");

			return AggregatePostView(lpvm);
		}

		[HttpGet("page")]
		public IActionResult Page()
		{
			return Index();
		}

		[HttpGet("page/{index:int}")]
		public IActionResult Page(int index)
		{
			if (index == 0)
			{
				return Index();
			}

			ViewData["Message"] = string.Format("Page...{0}", index);

			var lpvm = new ListPostsViewModel
			{
				Posts = blogManager.GetEntriesForPage(index, Request.Headers["Accept-Language"])
								.Select(entry => mapper.Map<PostViewModel>(entry)).ToList()
			};

			return AggregatePostView(lpvm);
		}

		public IActionResult About()
		{
			DefaultPage();

			ViewData["Message"] = "Your application description page.";

			return NoContent();
		}

		public IActionResult Contact()
		{
			DefaultPage();

			ViewData["Message"] = "Your contact page.";

			return NoContent();
		}

		public IActionResult Error()
		{
			try
			{
				var feature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
				if (feature != null)
				{
					var path = feature.Path;
					var ex = feature.Error;
				}
				return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

			}
			catch (Exception ex)
			{
				logger.LogError(ex, ex.Message, null);
				return Content("DasBlog - an error occurred (and reporting gailed) - Click the browser 'Back' button to try using the application");
			}
		}
	}
}

