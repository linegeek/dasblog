﻿using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using DasBlog.Core;
using DasBlog.Managers.Interfaces;
using DasBlog.Web.Models.BlogViewModels;
using DasBlog.Web.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using newtelligence.DasBlog.Runtime;

namespace DasBlog.Web.Controllers
{
	[Authorize]
	public class BlogPostController : DasBlogBaseController
	{
		private IBlogManager _blogManager;
		private IHttpContextAccessor _httpContextAccessor;
		private readonly IDasBlogSettings _dasBlogSettings;
		private readonly IMapper _mapper;

		public BlogPostController(IBlogManager blogManager, IHttpContextAccessor httpContextAccessor,
									IDasBlogSettings settings, IMapper mapper) : base(settings)
		{
			_blogManager = blogManager;
			_httpContextAccessor = httpContextAccessor;
			_dasBlogSettings = settings;
			_mapper = mapper;
		}

		[AllowAnonymous]
		public IActionResult Post(string posttitle)
		{
			ListPostsViewModel lpvm = new ListPostsViewModel();

			if (!string.IsNullOrEmpty(posttitle))
			{
				var entry = _blogManager.GetBlogPost(posttitle.Replace(_dasBlogSettings.SiteConfiguration.TitlePermalinkSpaceReplacement, string.Empty));
				if (entry != null)
				{
					lpvm.Posts = new List<PostViewModel>() { _mapper.Map<PostViewModel>(entry) };

					SinglePost(lpvm.Posts.First());

					return View("Page", lpvm);
				}
				else
				{
					return NotFound();
				}
			}
			else
			{
				return RedirectToAction("Index", "Home");
			}
		}

		[HttpGet("post/{postid:guid}/edit")]
		public IActionResult EditPost(Guid postid)
		{
			PostViewModel pvm = new PostViewModel();

			if (!string.IsNullOrEmpty(postid.ToString()))
			{
				var entry = _blogManager.GetEntryForEdit(postid.ToString());
				if (entry != null)
				{
					pvm = _mapper.Map<PostViewModel>(entry);
					List<CategoryViewModel> allcategories = _mapper.Map<List<CategoryViewModel>>(_blogManager.GetCategories());

					foreach (var cat in allcategories)
					{
						if (pvm.Categories.Count(x => x.Category == cat.Category) > 0)
						{
							cat.Checked = true;
						}
					}

					pvm.AllCategories = allcategories;

					return View(pvm);
				}
			}

			return NotFound();
		}

		[HttpPost("post/edit")]
		public IActionResult EditPost(PostViewModel post)
		{
			if (!ModelState.IsValid)
			{
				return View(post);
			}

			try
			{
				Entry entry = _mapper.Map<Entry>(post);

				entry.Author = _httpContextAccessor.HttpContext.User.Identity.Name;
				entry.Language = "en-us"; //TODO: We inject this fron http context?
				entry.Latitude = null;
				entry.Longitude = null;

				EntrySaveState sts = _blogManager.UpdateEntry(entry);
				if (sts != EntrySaveState.Updated)
				{
					ModelState.AddModelError("", "Failed to edit blog post");
					return View(post);
				}
			}
			catch (Exception e)
			{
				RedirectToAction("Error");
			}

			return View(post);
		}

		[HttpGet("post/create")]
		public IActionResult CreatePost()
		{
			PostViewModel post = new PostViewModel();
			post.CreatedDateTime = DateTime.UtcNow;  //TODO: Set to the timezone configured???
			post.AllCategories = _mapper.Map<List<CategoryViewModel>>(_blogManager.GetCategories());

			return View(post);
		}

		[HttpPost("post/create")]
		public IActionResult CreatePost(PostViewModel post)
		{
			if (!ModelState.IsValid)
			{
				return View(post);
			}

			try
			{
				Entry entry = _mapper.Map<Entry>(post);

				entry.Initialize();
				entry.Author = _httpContextAccessor.HttpContext.User.Identity.Name;
				entry.Language = "en-us"; //TODO: We inject this fron http context?
				entry.Latitude = null;
				entry.Longitude = null;

				EntrySaveState sts = _blogManager.CreateEntry(entry);
				if (sts != EntrySaveState.Added)
				{
					ModelState.AddModelError("", "Failed to create blog post");
					return View(post);
				}
			}
			catch (Exception e)
			{
				RedirectToAction("Error");
			}

			return View("Views/BlogPost/EditPost.cshtml", post);
		}

		[HttpGet("post/{postid:guid}/delete")]
		public IActionResult DeletePost(Guid postid)
		{
			try
			{
				_blogManager.DeleteEntry(postid.ToString());
			}
			catch (Exception ex)
			{
				RedirectToAction("Error");
			}

			return RedirectToAction("Index", "Home");
		}

		[AllowAnonymous]
		[HttpGet("post/{postid:guid}/comment")]
		public IActionResult Comment(Guid postid)
		{
			// TODO are comments enabled?

			Entry entry = _blogManager.GetBlogPost(postid.ToString());

			ListPostsViewModel lpvm = new ListPostsViewModel();
			lpvm.Posts = new List<PostViewModel> { _mapper.Map<PostViewModel>(entry) };

			ListCommentsViewModel lcvm = new ListCommentsViewModel
			{
				Comments = _blogManager.GetComments(postid.ToString(), false)
					.Select(comment => _mapper.Map<CommentViewModel>(comment)).ToList(),
				PostId = postid.ToString()
			};

			lpvm.Posts.First().Comments = lcvm;

			SinglePost(lpvm.Posts.First());

			return View("Page", lpvm);
		}

		[AllowAnonymous]
		[HttpPost("post/comment")]
		public IActionResult AddComment(AddCommentViewModel addcomment)
		{
			if (!_dasBlogSettings.SiteConfiguration.EnableComments)
			{
				return BadRequest();
			}

			if (!ModelState.IsValid)
			{
				Comment(new Guid(addcomment.TargetEntryId));
			}

			Comment commt = _mapper.Map<Comment>(addcomment);
			commt.AuthorIPAddress = HttpContext.Connection.RemoteIpAddress.ToString();
			commt.AuthorUserAgent = HttpContext.Request.Headers["User-Agent"].ToString();
			commt.CreatedUtc = commt.ModifiedUtc = DateTime.UtcNow;
			commt.EntryId = Guid.NewGuid().ToString();
			commt.IsPublic = !_dasBlogSettings.SiteConfiguration.CommentsRequireApproval;

			CommentSaveState state = _blogManager.AddComment(addcomment.TargetEntryId, commt);

			if (state == CommentSaveState.Failed)
			{
				ModelState.AddModelError("", "Comment failed");
				return StatusCode(500);
			}

			if (state == CommentSaveState.NotFound)
			{
				ModelState.AddModelError("", "Invalid comment attempt");
				return NotFound();
			}

			return Comment(new Guid(addcomment.TargetEntryId));
		}

		[HttpDelete("post/{postid:guid}/comment/{commentid:guid}")]
		public IActionResult DeleteComment(Guid postid, Guid commentid)
		{
			CommentSaveState state = _blogManager.DeleteComment(postid.ToString(), commentid.ToString());

			if (state == CommentSaveState.Failed)
			{
				return StatusCode(500);
			}

			if (state == CommentSaveState.NotFound)
			{
				return NotFound();
			}

			return Ok();
		}

		[HttpPatch("post/{postid:guid}/comment/{commentid:guid}")]
		public IActionResult ApproveComment(Guid postid, Guid commentid)
		{
			CommentSaveState state = _blogManager.ApproveComment(postid.ToString(), commentid.ToString());

			if (state == CommentSaveState.Failed)
			{
				return StatusCode(500);
			}

			if (state == CommentSaveState.NotFound)
			{
				return NotFound();
			}

			return Ok();
		}
	}
}
