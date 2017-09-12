﻿using Microsoft.AspNetCore.Mvc;
using Forum.Data;
using Forum.Web.Models.Reply;
using Forum.Data.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;

namespace Forum.Web.Controllers
{
    public class ReplyController : Controller
    {
        private IForum _forumService;
        private IPost _postService;
        private IApplicationUser _userService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReplyController(IForum forumService, IPost postService, IApplicationUser userService, UserManager<ApplicationUser> userManager)
        {
            _forumService = forumService;
            _postService = postService;
            _userService = userService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Create(int id)
        {
            var post = _postService.GetById(id);
            var forum = _forumService.GetById(post.Forum.Id);
            var replyingUser = GetReplyingUser();
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
                 
            var model = new PostReplyModel
            {
                PostContent = post.Content,
                PostTitle = post.Title,
                PostId = post.Id,

                ForumName = forum.Title,
                ForumId = forum.Id,
                ForumImageUrl = forum.ImageUrl,

                AuthorName = User.Identity.Name,
                AuthorImageUrl = user.ProfileImageUrl,
                AuthorId = user.Id,
                AuthorRating = user.Rating,

                Date = DateTime.Now
            };

            return View(model);
        }

        private async Task<ApplicationUser> GetReplyingUser()
        {
            return await _userManager.GetUserAsync(User);
        }

        [HttpPost]
        public async Task<IActionResult> AddReply(PostReplyModel model)
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId);

            var reply = BuildReply(model, user);
            await _postService.AddReply(reply);
            return RedirectToAction("Index", "Post", new { id = model.PostId });
        }

        private PostReply BuildReply(PostReplyModel reply, ApplicationUser user)
        {
            var now = DateTime.Now;
            var post = _postService.GetById(reply.PostId);

            return new PostReply 
            {
                Post = post,
                Content = reply.ReplyContent,
                Created = now,
                User = user
            };
        }
    }
}