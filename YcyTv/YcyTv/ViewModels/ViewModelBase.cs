using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Prism.Mvvm;
using Prism.Navigation;

using Xamarin.Essentials;

using YcyTv.DataBase;

namespace YcyTv.ViewModels
{
    public abstract class ViewModelBase : BindableBase, IInitialize, INavigationAware, IDestructible
    {
        protected INavigationService NavigationService { get; private set; }

        private string _title;

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        protected ViewModelBase(INavigationService navigationService)
        {
            NavigationService = navigationService;
        }

        /// <summary>
        /// 页面加载时候可以获取到属性。
        /// </summary>
        /// <param name="parameters">传递的参数</param>
        public virtual async void Initialize(INavigationParameters parameters)
        {
            await CheckUpdateData();
        }

        protected async Task CheckUpdateData()
        {
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
        /// 页面弹出时
        /// </summary>
        /// <param name="parameters"></param>
        public virtual void OnNavigatedFrom(INavigationParameters parameters)
        {
        }

        /// <summary>
        /// 导航到
        /// </summary>
        /// <param name="parameters"></param>
        public virtual void OnNavigatedTo(INavigationParameters parameters)
        {
        }

        /// <summary>
        /// 销毁
        /// </summary>
        public virtual void Destroy()
        {
        }
    }
}
