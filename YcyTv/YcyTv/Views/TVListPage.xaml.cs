using System.Linq;
using Syncfusion.ListView.XForms;
using Xamarin.Forms;
using YcyTv.ViewModels;

namespace YcyTv.Views
{
    public partial class TVListPage : ContentPage
    {
        public TVListPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            if (BindingContext is TVListPageViewModel vm)
            {
                if (vm.VodTypes.Any() && VodTypeList.CurrentItem == null)
                {
                    VodTypeList.CurrentItem = vm.VodTypes.First();
                }
                if (vm.VodAreas.Any() && VodAreaList.CurrentItem == null)
                {
                    VodAreaList.CurrentItem = vm.VodAreas.First();
                }
                if (vm.VodYears.Any() && VodYearList.CurrentItem == null)
                {
                    VodYearList.CurrentItem = vm.VodYears.First();
                }
            }
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            TvVodPlayList.LayoutManager = height > width ? new GridLayout { SpanCount = 3 } : new GridLayout { SpanCount = 5 };

            base.OnSizeAllocated(width, height);
        }
    }
}
