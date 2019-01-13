using MishMashWebApp.Models;
using MishMashWebApp.ViewModels.Channels;
using MishMashWebApp.ViewModels.Home;
using SIS.HTTP.Responses;
using SIS.MvcFramework;
using System.Collections.Generic;
using System.Linq;

namespace MishMashWebApp.Controllers
{
    public class HomeController : BaseController
    {
        public IHttpResponse Index()
        {
            User user = this.Db.Users.FirstOrDefault(u => u.Username == this.User.Username);
            if (user != null)
            {
                LoggedInIndexViewModel viewModel = new LoggedInIndexViewModel();
                //viewModel.UserRole = user.Role.ToString(); //Вече взимам информация дали е админ не от вю модел, а от MvcUserInfo класа, който добавихме

                viewModel.YourChannels = this.Db.Channels.Where(c =>
                     c.Followers.Any(uc => uc.User.Username == this.User.Username))
                        .Select(c => new BaseChannelViewModel
                        {
                            Id = c.Id,
                            Type = c.Type,
                            Name = c.Name,
                            FollowersCount = c.Followers.Count()
                        }).ToList();

                List<int> followedChannelsTags = this.Db.Channels.Where(c =>
                     c.Followers.Any(uc => uc.User.Username == this.User.Username))//за всички канали, който е последвал юзъра
                     .SelectMany(x => x.Tags.Select(t => t.TagId)).ToList(); //искам да взема Ид-тата на теговете и да ги образувам в една обща колекция. (с повторенията)

                var unfollowedChannels = this.Db.Channels.Where(c =>
                     !c.Followers.Any(uc => uc.User.Username == this.User.Username) && //каналите, които не сме последвали
                     c.Tags.Any(t => followedChannelsTags.Contains(t.TagId)));   //и съдържат поне един общ таг от списъка с таговете на последваните канали.

                viewModel.SuggestedChannels = unfollowedChannels
                      .Select(c => new BaseChannelViewModel
                      {
                          Id = c.Id,
                          Type = c.Type,
                          Name = c.Name,
                          FollowersCount = c.Followers.Count()
                      }).ToList();

                List<int> otherChannelsIds = viewModel.YourChannels.Select(x => x.Id).ToList();
                otherChannelsIds = otherChannelsIds.Concat(viewModel.SuggestedChannels.Select(x => x.Id)).ToList();
                otherChannelsIds = otherChannelsIds.Distinct().ToList();

                viewModel.SeeOtherChannels = this.Db.Channels.Where(c => !otherChannelsIds.Contains(c.Id))
                     .Select(c => new BaseChannelViewModel
                     {
                         Id = c.Id,
                         Type = c.Type,
                         Name = c.Name,
                         FollowersCount = c.Followers.Count()
                     }).ToList();

                //if(user.Role == Role.Admin)
                //{
                //    string layoutName = "_LayoutAdmin"; //този лейаут вече не е нужeн, защото в основния добавихме проверка ако юзъра е админ да слага още едно линкче в нав-бар-а
                //    return this.View("Home/LoggedInIndex",viewModel, layoutName);
                //}

                return this.View("Home/LoggedIn-Index", viewModel);
            }
            else
            {
                return this.View();
            }
        }
    }
}