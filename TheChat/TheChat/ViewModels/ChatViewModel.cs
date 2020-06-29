using Acr.UserDialogs;
using FreshMvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using TheChat.Core.Models;
using TheChat.Core.Services;

namespace TheChat.ViewModels
{
    public class ChatViewModel : FreshBasePageModel
    {
        private readonly IChatService _chateService;
        private readonly IUserDialogs _userDialogs;

        public ChatViewModel(IChatService chateService, IUserDialogs userDialogs)
        {
            _chateService = chateService;
            _userDialogs = userDialogs;
        }

        public ObservableCollection<User> Users { get; set; } = new ObservableCollection<User>();

        public string UserName { get; set; }

        public string GroupName { get; set; }

        public override async void Init(object initData)
        {
            base.Init(initData);
            var data = initData as Tuple<string, string>;
            UserName = data.Item1;
            GroupName = data.Item2;

            var user = await _chateService.GetUsersGroup(GroupName);
            Users = new ObservableCollection<User>(user);
        }

    }
}
