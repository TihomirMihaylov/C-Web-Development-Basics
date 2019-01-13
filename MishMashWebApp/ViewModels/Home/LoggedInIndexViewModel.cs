using MishMashWebApp.ViewModels.Channels;
using System.Collections.Generic;

namespace MishMashWebApp.ViewModels.Home
{
    public class LoggedInIndexViewModel
    {
        public string UserRole { get; set; } ////Вече взимам информация дали е админ не от вю модел, а от MvcUserInfo класа, който добавихме. Може и без това пропърти, но го оставям защото на места се ползва

        public IEnumerable<BaseChannelViewModel> YourChannels { get; set; }

        public IEnumerable<BaseChannelViewModel> SuggestedChannels { get; set; }

        public IEnumerable<BaseChannelViewModel> SeeOtherChannels { get; set; }
    }
}