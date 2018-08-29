﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using DasBlog.Managers.Interfaces;
using newtelligence.DasBlog.Web.Services.Rss20;
using Microsoft.AspNetCore.Http;
using newtelligence.DasBlog.Web.Services.Rsd;

namespace DasBlog.Web.Controllers
{
    [Produces("text/xml")]
    [Route("feed")]
    public class FeedController : DasBlogController
    {
        private IMemoryCache memoryCache;
        private ISubscriptionManager subscriptionManager;
        private const string RSS_CACHE_KEY = "RSS_CACHE_KEY";

        public FeedController(ISubscriptionManager subscriptionManager, IHttpContextAccessor httpContextAccessor, IMemoryCache memoryCache)
        {  
            this.subscriptionManager = subscriptionManager;
            this.memoryCache = memoryCache;
        }

        [Route("")]
        [HttpGet("rss")]
        public IActionResult Rss()
        {
            RssRoot rss = null; 

            if (!memoryCache.TryGetValue(RSS_CACHE_KEY, out rss))
            {
                rss = subscriptionManager.GetRss();

                var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(5));

                memoryCache.Set(RSS_CACHE_KEY, rss, cacheEntryOptions);
            }

            return Ok(rss);
        }

        [HttpGet("rss/{category}")]
        public IActionResult RssByCategory(string category)
        {
            RssRoot rss = null;

            if (!memoryCache.TryGetValue(RSS_CACHE_KEY + "_" + category, out rss))
            {
                rss = subscriptionManager.GetRssCategory(category);

                var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(5));

                memoryCache.Set(RSS_CACHE_KEY + "_" + category, rss, cacheEntryOptions);
            }

            return Ok(rss);
        }

        [HttpGet("rsd")]
        public ActionResult Rsd()
        {
            RsdRoot rsd = null;

            rsd = subscriptionManager.GetRsd();

            return Ok(rsd);
        }

        public IActionResult Atom()
        {
            return NoContent();
        }

        public IActionResult Atom(string category)
        {
            return NoContent();
        }
    }
}
