﻿using AutoMapper;
using DasBlog.Managers.Interfaces;
using DasBlog.Services;
using System.Linq;
using DasBlog.Web.Models.BlogViewModels;
using DasBlog.Web.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using newtelligence.DasBlog.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using EventCodes = DasBlog.Services.ActivityLogs.EventCodes;
using DasBlog.Services.ActivityLogs;

namespace DasBlog.Web.Controllers
{
	[Route("archive")]
	[ResponseCache(Duration = 14400, Location = ResponseCacheLocation.Any)]
	public class ArchiveController : DasBlogBaseController
	{
		private readonly IArchiveManager archiveManager;
		private readonly IHttpContextAccessor httpContextAccessor;
		private readonly IMapper mapper;
		private readonly ILogger<ArchiveController> logger;
		private const string ARCHIVE = "Archive";

		public ArchiveController(IArchiveManager archiveManager, IHttpContextAccessor httpContextAccessor, IMapper mapper,
									ILogger<ArchiveController> logger, IDasBlogSettings settings) : base(settings)
		{
			this.archiveManager = archiveManager;
			this.httpContextAccessor = httpContextAccessor;
			this.mapper = mapper;
			this.logger = logger;
		}

		[HttpGet("")]
		public IActionResult Archive()
		{
			return Archive(DateTime.Now.Year, DateTime.Now.Month);
		}

		[HttpGet("{year}")]
		public IActionResult Archive(int year)
		{
			var dateTime = new DateTime(year, 1, 1);
			var months = GetMonthsViewModel(dateTime, true);
			return View(months);
		}

		[HttpGet("{year}/{month}")]
		public IActionResult Archive(int year, int month)
		{
			var dateTime = new DateTime(year, month, 1);
			var months = GetMonthsViewModel(dateTime);
			return View(months);
		}

		[HttpGet("{year}/{month}/{day}")]
		public IActionResult Archive(int year, int month, int day)
		{
			var dateTime = new DateTime(year, month, day);
			var months = GetMonthsViewModel(dateTime);
			return View(months);
		}

		private List<MonthViewViewModel> GetMonthsViewModel(DateTime dateTime, bool wholeYear = false)
		{
			string languageFilter = httpContextAccessor.HttpContext.Request.Headers["Accept-Language"];

			ViewBag.PreviousMonth = dateTime.AddMonths(-1).Date;
			ViewBag.NextMonth = dateTime.AddMonths(1).Date;
			ViewBag.CurrentMonth = dateTime.Date;

			var stopWatch = new Stopwatch();
			stopWatch.Start();

			//unique list of years for the top of archives
			var daysWithEntries = archiveManager.GetDaysWithEntries();
			ViewBag.Years = daysWithEntries.Select(i => i.Year).Distinct();

			EntryCollection entries;
			if (wholeYear)
				entries = archiveManager.GetEntriesForYear(dateTime, languageFilter);
			else
				entries = archiveManager.GetEntriesForMonth(dateTime, languageFilter);


			stopWatch.Stop();
			logger.LogInformation(new DasBlog.Services.ActivityLogs.EventDataItem(EventCodes.Site, null, $"ArchiveController (Date: {dateTime.ToLongDateString()}; Year: {wholeYear}) Time elapsed: {stopWatch.Elapsed.TotalMilliseconds}ms"));

			//TODO: Do I need this?
			//entries = new EntryCollection(entries.OrderBy(e => e.CreatedUtc));

			DefaultPage(ARCHIVE);
			return MonthViewViewModel.Create(dateTime, entries, mapper);
		}
	}
}
