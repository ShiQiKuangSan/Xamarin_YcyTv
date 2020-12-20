using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Prism.Navigation;

using Syncfusion.XForms.PopupLayout;

using Xamarin.Essentials;
using Xamarin.Forms;

using YcyTv.Api;
using YcyTv.DataBase;
using YcyTv.Views;

namespace YcyTv
{
    public partial class MainPage : ContentPage
    {
        private readonly INavigationService _navigationService;
        private readonly DownloadHelper _downloadHelper;

        public MainPage(INavigationService navigationService)
        {
            InitializeComponent();
            _navigationService = navigationService;
            _downloadHelper = new DownloadHelper();

            NavigationPage.SetHasNavigationBar(this, false);

            LoadPopupLayout.PopupView.AutoSizeMode = AutoSizeMode.Height;
            LoadPopupLayout.PopupView.AnimationMode = AnimationMode.Zoom;
            LoadPopupLayout.PopupView.ShowFooter = false;
            LoadPopupLayout.PopupView.ShowHeader = false;

            Application.Current.Resources.TryGetValue("ItemColor", out var color);

            LoadPopupLayout.PopupView.ContentTemplate = new DataTemplate(() => new StackLayout
            {
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                VerticalOptions = LayoutOptions.CenterAndExpand,
                BackgroundColor = (Color)color,
                Children =
                {
                    new Label
                    {
                        Text = "初始化中",
                        TextColor = Color.White,
                        FontSize = 20,
                        HorizontalOptions = LayoutOptions.CenterAndExpand,
                        VerticalOptions = LayoutOptions.CenterAndExpand
                    }
                }
            });

        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (Device.RuntimePlatform == Device.Android)
            {
                AndroidLayout.IsVisible = true;
                AndroidLayout.IsEnabled = true;

                UWPLayout.IsVisible = false;
                UWPLayout.IsEnabled = false;
            }
            else
            {
                AndroidLayout.IsVisible = false;
                AndroidLayout.IsEnabled = false;

                UWPLayout.IsVisible = true;
                UWPLayout.IsEnabled = true;
            }

            var status = await Permissions.CheckStatusAsync<Permissions.StorageRead>();

            if (status != PermissionStatus.Granted)
            {
                await Permissions.RequestAsync<Permissions.StorageRead>();
            }

            status = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();

            if (status != PermissionStatus.Granted)
            {
                await Permissions.RequestAsync<Permissions.StorageWrite>();
            }

            status = await Permissions.CheckStatusAsync<Permissions.NetworkState>();

            if (status != PermissionStatus.Granted)
            {
                await Permissions.RequestAsync<Permissions.NetworkState>();
            }

            var status1 = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
            var status2 = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();
            var status3 = await Permissions.CheckStatusAsync<Permissions.NetworkState>();

            if (status1 == PermissionStatus.Granted && status2 == PermissionStatus.Granted && status3 == PermissionStatus.Granted && File.Exists(Constants.DatabasePath) && App.Thread == null)
            {
                App.Thread = new Thread(App.UpdateVods) { IsBackground = true };
                App.Thread.Start();

                new Thread(async () =>
                {
                    try
                    {
                        var d = DateTime.Now.AddMonths(-6);

                        await App.Database.Table<VodPlayHistoryDb>()
                             .Where(x => DateTime.Parse(x.PlayTime) <= d)
                             .DeleteAsync();
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                })
                { IsBackground = true }.Start();
            }
        }

        /// <summary>
        /// 搜索按钮被点击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Search_Btn_Clicked(object sender, EventArgs e)
        {
            await _navigationService.NavigateAsync($"{nameof(SearchPage)}");
        }

        /// <summary>
        /// 电视按钮被点击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void TV_Btn_Clicked(object sender, EventArgs e)
        {
            await _navigationService.NavigateAsync($"{nameof(TVListPage)}");
        }

        /// <summary>
        /// 历史被点击
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void History_Btn_Clicked(object sender, EventArgs e)
        {
            await _navigationService.NavigateAsync($"{nameof(HistoryItemsPage)}");
        }

        /// <summary>
        /// 重新初始化数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Reinitialize_Btn_Clicked(object sender, EventArgs e)
        {
            var status = await Permissions.CheckStatusAsync<Permissions.StorageRead>();

            if (status != PermissionStatus.Granted)
            {
                await DisplayAlert("警告", "文件读权限没有开启", "确定");
                await Permissions.RequestAsync<Permissions.StorageRead>();
            }

            status = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();

            if (status != PermissionStatus.Granted)
            {
                await DisplayAlert("警告", "文件写权限没有开启", "确定");
                await Permissions.RequestAsync<Permissions.StorageWrite>();
            }

            status = await Permissions.CheckStatusAsync<Permissions.NetworkState>();

            if (status != PermissionStatus.Granted)
            {
                await DisplayAlert("警告", "网络状态权限没有开启", "确定");
                await Permissions.RequestAsync<Permissions.NetworkState>();
            }

            var status1 = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
            var status2 = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();
            var status3 = await Permissions.CheckStatusAsync<Permissions.NetworkState>();

            if (status1 == PermissionStatus.Granted && status2 == PermissionStatus.Granted && status3 == PermissionStatus.Granted)
            {
                try
                {
                    LoadPopupLayout.Show();
                    App.Thread?.Interrupt();
                    App.Thread = null;

                    //初始化数据
                    await Task.Run(async () =>
                    {
                        await _downloadHelper.DownloadFile(Constants.DowloadDbUrl, Constants.DatabasePath, true);
                    }).ContinueWith((obj) =>
                       {
                           Device.BeginInvokeOnMainThread(() => { LoadPopupLayout.Dismiss(); });
                       })
                        .ContinueWith(async (t) =>
                       {
                           await Task.Delay(5000);
                           App.Thread = new Thread(App.UpdateVods) { IsBackground = true };
                           App.Thread.Start();
                       });
                }
                catch (Exception ex)
                {
                    await DisplayAlert("异常", ex.ToString(), "确定");
                }
            }
        }
    }
}
