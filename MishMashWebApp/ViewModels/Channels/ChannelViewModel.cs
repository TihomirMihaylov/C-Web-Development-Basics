using MishMashWebApp.Models;
using System.Collections.Generic;

namespace MishMashWebApp.ViewModels.Channels
{
    public class ChannelViewModel
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public ChannelType Type { get; set; }

        public virtual IEnumerable<string> Tags { get; set; }

        public string TagsAsString => string.Join(", ", this.Tags);

        public int FollowersCount { get; set; }
    }
}