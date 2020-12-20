using System.Collections.Generic;
using System.Threading;

using Prism.Events;
using Prism.Ioc;

using SQLite;

using Xamarin.Essentials.Implementation;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.WindowsSpecific;

using YcyTv.DataBase;
using YcyTv.ViewModels;
using YcyTv.Views;
using YiciTV.Api;

namespace YcyTv
{
    public partial class App
    {
        private IEventAggregator _eventAggregator;

        private static DatabaseHelper _databaseHelper;

        public static Thread Thread { get; set; }

        public static SQLiteAsyncConnection Database
        {
            get
            {
                if (_databaseHelper == null)
                {
                    _databaseHelper = new DatabaseHelper();
                }

                return DatabaseHelper.Database;
            }
        }

        protected override async void OnInitialized()
        {
            Xamarin.Forms.Device.SetFlags(new List<string>
            {
                "StateTriggers_Experimental", "IndicatorView_Experimental", "CarouselView_Experimental",
                "MediaElement_Experimental"
            });

            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(
                "MzY5MjEwQDMxMzgyZTM0MmUzMFJsejZaRWN2d041R2tPMHhzNklldXh4RzY4UCtYbWsxU1N4YWpaUFYrdGM9");

            InitializeComponent();

            On<Windows>().SetImageDirectory("Assets");

            await NavigationService.NavigateAsync("NavigationPage/MainPage");
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IAppInfo, AppInfoImplementation>();
            containerRegistry.RegisterForNavigation<NavigationPage>();
            containerRegistry.RegisterForNavigation<MainPage>();
            containerRegistry.RegisterForNavigation<TVListPage, TVListPageViewModel>();
            containerRegistry.RegisterForNavigation<HistoryItemsPage, HistoryItemsPageViewModel>();
            containerRegistry.RegisterForNavigation<SearchPage, SearchPageViewModel>();
            containerRegistry.RegisterForNavigation<VodDetailPage, VodDetailPageViewModel>();
            containerRegistry.RegisterForNavigation<VodPlayPage, VodPlayPageViewModel>();
        }

        public static IEventAggregator GetEventAggregator()
        {
            if (Current is App app)
            {
                return app._eventAggregator ?? (app._eventAggregator = app.Container.Resolve<IEventAggregator>());
            }

            return null;
        }

        public static async void UpdateVods()
        {
            try
            {
                await OkzywApi.StartVodPaChong();
            }
            catch (System.Exception)
            {
                // ignored
            }
        }
    }
}
