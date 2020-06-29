using Acr.UserDialogs;
using FreshMvvm;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using TheChat.Core.Models;
using TheChat.Core.Services;
using Xamarin.Forms;

namespace TheChat.ViewModels
{
    public class RoomsViewModel : FreshBasePageModel
    {
        private readonly IChatService _chatService;
        private readonly IUserDialogs _userDialogs;
        bool IsBusy = false;
        string UserName;

        public RoomsViewModel(IChatService chatService, IUserDialogs userDialogs)
        {
            _chatService = chatService;
            _userDialogs = userDialogs;
        }

        public List<Room> Rooms { get; set; }
        public Room CurrentRoom { get; set; }

        public ICommand EnterRoomCommand => new Command(async () =>
        {
            if (!IsBusy)
            {
                IsBusy = true;
                if(CurrentRoom != null)
                {
                    Tuple<string, string> data = new Tuple<string, string>(UserName, CurrentRoom.Name);
                    await CoreMethods.PushPageModel<ChatViewModel>(data);
                    CurrentRoom = null;
                }
                IsBusy = false;
            }
        });

        public override void Init(object initData)
        {
            base.Init(initData);
            UserName = initData as string;
        }

        protected override async void ViewIsAppearing(object sender, EventArgs e)
        {
            base.ViewIsAppearing(sender, e);
            _userDialogs.ShowLoading("Cargando");

            Rooms = await _chatService.GetRooms();

            _userDialogs.HideLoading();
        }
    }
}
