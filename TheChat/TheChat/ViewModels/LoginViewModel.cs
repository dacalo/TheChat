using Acr.UserDialogs;
using FreshMvvm;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using TheChat.Core.Services;
using Xamarin.Forms;

namespace TheChat.ViewModels
{
    public class LoginViewModel : FreshBasePageModel
    {
        private readonly IChatService _chatService;
        private readonly IUserDialogs _userDialogs;

        public string UserName { get; set; }
        public bool IsBusy { get; set; }
        

        public LoginViewModel(IChatService chatService, IUserDialogs userDialogs)
        {
            _chatService = chatService;
            _userDialogs = userDialogs;
        }

        public ICommand ConnectCommand => new Command(async () =>
        {
            if (!IsBusy)
            {
                IsBusy = true;
                _userDialogs.ShowLoading("Conectando");

                await _chatService.InitAsync(UserName);
                await CoreMethods.PushPageModel<RoomsViewModel>(UserName);

                _userDialogs.HideLoading();
                IsBusy = false;
            }
        });
    }
}
