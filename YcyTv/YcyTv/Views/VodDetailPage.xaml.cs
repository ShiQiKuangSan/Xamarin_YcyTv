using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

using Syncfusion.ListView.XForms;

using Xamarin.Forms;

using YcyTv.DataBase;
using YcyTv.Models;
using YcyTv.ViewModels;

using YiciTV.Api;

namespace YcyTv.Views
{
    public partial class VodDetailPage : ContentPage
    {
        public VodDetailPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
        }

        protected override async void OnAppearing()
        {
            await SelectHistoryItem();
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            if (width > height)
            {
                VodPlayUrlList.LayoutManager = new GridLayout
                {
                    SpanCount = 8
                };
            }
            else
            {
                VodPlayUrlList.LayoutManager = new GridLayout
                {
                    SpanCount = 4
                };
            }

            base.OnSizeAllocated(width, height);
        }

        private async void UpdateUrlBtn_OnClicked(object sender, EventArgs e)
        {
            if (!(BindingContext is VodDetailPageViewModel vm))
                return;

            UpdateUrlBtn.IsEnabled = false;

            if (vm.Vod == null)
            {
                UpdateUrlBtn.IsEnabled = true;
                return;
            }

            var items = await OkzywApi.UpdateVodDetailUrl(vm.Vod.VodId);

            var list = new ObservableCollection<VodPlayUrlModel>();

            foreach (var playUrlDb in items)
            {
                list.Add(new VodPlayUrlModel
                {
                    Play = playUrlDb.Play,
                    PlayName = playUrlDb.PlayName
                });
            }

            vm.PlayUrls = list;

            await Task.Delay(1000);
            
            await SelectHistoryItem();

            UpdateUrlBtn.IsEnabled = true;

            UpdateChildrenLayout();
        }

        private async Task SelectHistoryItem()
        {
            if (BindingContext is VodDetailPageViewModel vm && vm.Vod != null)
            {
                var history = await App.Database.Table<VodPlayHistoryDb>()
                      .Where(x => x.VodId == vm.Vod.VodId)
                      .FirstOrDefaultAsync();

                if (history != null)
                {
                    if (history.PlayIndex <= vm.PlayUrls.Count - 1)
                    {
                        var playUrl = vm.PlayUrls[history.PlayIndex];

                        VodPlayUrlList.CurrentItem = playUrl;
                        VodPlayUrlList.SelectedItem = playUrl;
                    }
                }
            }
        }
    }
}
