using MishMashWebApp.Models;
using MishMashWebApp.ViewModels.Channels;
using SIS.HTTP.Responses;
using SIS.MvcFramework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MishMashWebApp.Controllers
{
    public class ChannelsController : BaseController
    {
        [Authorize]
        public IHttpResponse Details(int id)
        {
            ChannelViewModel channelViewModel = this.Db.Channels.Where(c => c.Id == id)
                        .Select(c => new ChannelViewModel
                        {
                            Type = c.Type,
                            Description = c.Description,
                            Name = c.Name,
                            FollowersCount = c.Followers.Count(),
                            Tags = c.Tags.Select(tag => tag.Tag.Name)
                        }).FirstOrDefault();

            if(channelViewModel == null)
            {
                return this.BadRequestError("Invalid channel id.");
            }

            return this.View(channelViewModel);
        }

        [Authorize]
        public IHttpResponse Followed()
        {
            User user = this.Db.Users.FirstOrDefault(u => u.Username == this.User.Username);
            //if (user == null)
            //{
            //    return this.View("Users/Login");
            //}

            List<BaseChannelViewModel> followedChannels = this.Db.Channels.Where(c =>
                    c.Followers.Any(uc => uc.User.Username == this.User.Username))
                        .Select(c => new BaseChannelViewModel
                        {
                             Id = c.Id,
                             Type = c.Type,
                             Name = c.Name,
                             FollowersCount = c.Followers.Count()
                        }).ToList();

            FollowedChannelsViewModel viewModel = new FollowedChannelsViewModel
            {
                FollowedChannels = followedChannels
            };

            if(user.Role == Role.Admin){
                string layoutName = "_LayoutAdmin";
                return this.View(viewModel, layoutName);
            }

            return this.View("Channels/Followed", viewModel);
        }

        [Authorize]
        public IHttpResponse Follow(int id)
        {
            User user = this.Db.Users.FirstOrDefault(u => u.Username == this.User.Username);
            if(!this.Db.UserInChannel.Any(uc => 
                uc.UserId == user.Id && uc.ChannelId == id))
            {
                this.Db.UserInChannel.Add(new UserInChannel
                {
                    ChannelId = id,
                    UserId = user.Id
                });

                this.Db.SaveChanges();
            }

            return this.Redirect("/Channels/Followed");
        }

        [Authorize]
        public IHttpResponse Unfollow(int id)
        {
            User user = this.Db.Users.FirstOrDefault(u => u.Username == this.User.Username);

            UserInChannel userInChannel = this.Db.UserInChannel.FirstOrDefault(
                uc => uc.UserId == user.Id && uc.ChannelId == id);

            if(userInChannel != null)
            {
                this.Db.UserInChannel.Remove(userInChannel);
                this.Db.SaveChanges();
            }

            return this.Redirect("/Channels/Followed");
        }

        [Authorize("Admin")]
        public IHttpResponse Create()
        {
            //User user = this.Db.Users.FirstOrDefault(u => u.Username == this.User.Username);
            //if (user.Role != Role.Admin)
            //{
            //    return this.View("Users/Login");
            //}

            //string layoutName = "_LayoutAdmin";
            //return this.View(null, layoutName); //!!!

            return this.View();
        }
        
        [Authorize(nameof(Role.Admin))] //може и така да се напише. Така дори е по-правилно
        [HttpPost]
        public IHttpResponse Create(CreateChannelsInputModel model)
        {
            if(!Enum.TryParse(model.Type, true, out ChannelType type))
            {
                return this.BadRequestErrorWithView("Invalid channel type.");
            }

            Channel channel = new Channel
            {
                Name = model.Name,
                Description = model.Description,
                Type = type
            };

            if(!string.IsNullOrWhiteSpace(model.Tags))
            {
                string[] tags = model.Tags.Split(',', ';', StringSplitOptions.RemoveEmptyEntries);
                foreach (string tag in tags)
                {
                    Tag dbTag = this.Db.Tags.FirstOrDefault(t => t.Name == tag.Trim());
                    if (dbTag == null)
                    {
                        dbTag = new Tag { Name = tag.Trim() };
                        this.Db.Tags.Add(dbTag);
                        this.Db.SaveChanges();
                    }

                    channel.Tags.Add(new ChannelTag
                    {
                        TagId = dbTag.Id
                    });
                }
            }

            this.Db.Channels.Add(channel);
            this.Db.SaveChanges();

            return this.Redirect("/Channels/Details?id=" + channel.Id);
        }
    }
}