using Acr.UserDialogs;
using FreshMvvm;
using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using TheChat.Core.Helpers;
using TheChat.Core.Models;
using TheChat.Core.Services;
using TheChat.Messages;
using Xamarin.Forms;

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

        public string Text { get; set; }

        public User SelectedUser { get; set; }

        public ObservableCollection<ChatMessage> Messages { get; set; } = new ObservableCollection<ChatMessage>();

        public ICommand SendCommand => new Command(async () =>
        {
            if (string.IsNullOrEmpty(Text))
                return;

            var message = new SimpleTextMessage(UserName)
            {
                Text = this.Text,
                GroupName = this.GroupName
            };

            Messages.Add(new LocalSimpleTextMessage(message));

            await _chateService.SendMessageAsync(message);
            Text = string.Empty;
        });

        public ICommand PhotoCommand => new Command(async () =>
        {
            await CrossMedia.Current.Initialize();
            var options = new PickMediaOptions();
            options.CompressionQuality = 50;

            var photo = await CrossMedia.Current.PickPhotoAsync(options);
            
            if (photo == null)
                return;

            var stream = photo.GetStream();
            var bytes = ImageHelper.ReadFully(stream);
            var base64Photo = Convert.ToBase64String(bytes);
            var message = new PhotoMessage(UserName)
            {
                Base64Photo = base64Photo,
                FileEnding = photo.Path.Split('.').Last(),
                GroupName = GroupName
            };

            Messages.Add(message);
            _userDialogs.ShowLoading("Suviendo");
            await _chateService.SendMessageAsync(message);
            _userDialogs.HideLoading();
        });

        public ICommand ItemSelectedCommand => new Command(async () =>
        {
            if(SelectedUser != null)
            {
                var privateMessage = await _userDialogs.PromptAsync($"Private message for: {SelectedUser.UserId}");

                if (string.IsNullOrEmpty(privateMessage.Text))
                {
                    return;
                }

                var message = new SimpleTextMessage(UserName)
                {
                    Text = privateMessage.Text,
                    Recipient = SelectedUser.UserId
                };

                await _chateService.SendMessageAsync(message);
                SelectedUser = null;
            }
        });

        public ICommand LeaveCommand => new Command(async () =>
        {
            var message = new UserConnectedMessage(UserName, GroupName);
            await _chateService.LeaveChannelAsync(message);
            await CoreMethods.PopPageModel();
        });

        public override async void Init(object initData)
        {
            base.Init(initData);
            var data = initData as Tuple<string, string>;
            UserName = data.Item1;
            GroupName = data.Item2;

            var message = new UserConnectedMessage(UserName, GroupName);
            await _chateService.JoinChannelAsync(message);

            var user = await _chateService.GetUsersGroup(GroupName);
            Users = new ObservableCollection<User>(user);
        }

        protected override void ViewIsAppearing(object sender, EventArgs e)
        {
            base.ViewIsAppearing(sender, e);
            _chateService.OnReceivedMessage += _chateService_OnReceivedMessage;
        }

        private void _chateService_OnReceivedMessage(object sender, Core.EventHandlers.MessageEventArgs e)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                if(Messages.All(x => x.Id != e.Message.Id))
                {
                    if (e.Message.TypeInfo.Name == nameof(UserConnectedMessage))
                    {
                        var user = await _chateService.GetUsersGroup(GroupName);
                        Users = new ObservableCollection<User>(user);
                    }
                    if(e.Message.TypeInfo.Name != nameof(UserConnectedMessage))
                    {
                        var user = Users.FirstOrDefault(x => x.UserId == e.Message.Sender);
                        e.Message.Color = user.Color;
                        e.Message.Avatar = user.Avatar;
                    }
                    Messages.Add(e.Message);
                }
            });
        }

        protected override async void ViewIsDisappearing(object sender, EventArgs e)
        {
            base.ViewIsDisappearing(sender, e);
            _chateService.OnReceivedMessage -= _chateService_OnReceivedMessage;
            var message = new UserConnectedMessage(UserName, GroupName);
            await _chateService.LeaveChannelAsync(message);
        }

    }
}
