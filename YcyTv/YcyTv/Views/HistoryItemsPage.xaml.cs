using System;
using System.Linq;
using System.Threading.Tasks;
using Syncfusion.ListView.XForms;

using Xamarin.Forms;

using YcyTv.DataBase;
using YcyTv.ViewModels;

namespace YcyTv.Views
{
    public partial class HistoryItemsPage : ContentPage
    {
        public HistoryItemsPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            if (BindingContext is HistoryItemsPageViewModel vm)
            {
                try
                {
                    var count = await App.Database.Table<VodPlayHistoryDb>().CountAsync();
                    if (count > 0)
                    {
                        await vm.InitData();
                    }
                }
                catch (Exception)
                {
                    vm.IsHideData = true;
                    vm.IsHideNoData = false;
                }

                VodPlayHistoryList.IsVisible = vm.HistoryVodPlays.Count > 0;
                NoData.IsVisible = vm.HistoryVodPlays.Count <= 0;
            }

            base.OnAppearing();
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            VodPlayHistoryList.LayoutManager = height > width ? new GridLayout { SpanCount = 3 } : new GridLayout { SpanCount = 5 };
            base.OnSizeAllocated(width, height);
        }
    }
}
