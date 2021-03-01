using Prism.Commands;
using Prism.Mvvm;

using System;
using System.Collections.Generic;
using System.Linq;
using Prism.Navigation;
using System.Threading.Tasks;
using YcyTv.Models;
using System.Collections.ObjectModel;
using Syncfusion.ListView.XForms;
using YcyTv.Views;
using YcyTv.DataBase;
using Xamarin.Forms;

namespace YcyTv.ViewModels
{
    public class SearchPageViewModel : ViewModelBase
    {
        /// <summary>
        /// 当前页
        /// </summary>
        private int _pageIndex = 0;

        private string _name = string.Empty;

        private ObservableCollection<VodPlayModel> _vodPlays;

        /// <summary>
        /// 视频列表
        /// </summary>
        public ObservableCollection<VodPlayModel> VodPlays
        {
            get => _vodPlays;
            set => SetProperty(ref _vodPlays, value);
        }

        /// <summary>
        /// 点击视频查看详情事件
        /// </summary>
        public DelegateCommand<SfListView> ClickVodDetailCommand { get; set; }

        /// <summary>
        /// 加载更多视频事件
        /// </summary>
        public DelegateCommand<SfListView> VodListLoadMoreCommand { get; set; }

        public DelegateCommand<string> PerformSearch { get; set; }

        public SearchPageViewModel(INavigationService navigationService) : base(navigationService)
        {
            VodListLoadMoreCommand = new DelegateCommand<SfListView>(OnVodListLoadMore);
            ClickVodDetailCommand = new DelegateCommand<SfListView>(OnClickVodDetail);
            PerformSearch = new DelegateCommand<string>(OnPerformSearch);
        }

        private async void OnPerformSearch(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                _name = string.Empty;
                VodPlays = new ObservableCollection<VodPlayModel>();
                return;
            }
            _pageIndex = 0;
            _name = query.ToUpper();
            VodPlays = await GetVodPlays(_name);
        }

        public override void Initialize(INavigationParameters parameters)
        {
            base.Initialize(parameters);
        }

        /// <summary>
        /// 加载更多的时，上次加载的数据是否存在，如果不存在就不继续查找下一页了
        /// </summary>
        private static bool _oldvodlist = true;

        /// <summary>
        /// 加载更多
        /// </summary>
        /// <param name="sfListView"></param>
        private async void OnVodListLoadMore(SfListView sfListView)
        {
            if (!_oldvodlist)
            {
                sfListView.IsBusy = false;
                return;
            }

            sfListView.IsBusy = true;

            _pageIndex++;

            var list = await GetVodPlays(_name, _pageIndex, 25);

            var items = list.ToList();

            _oldvodlist = items.Any();

            foreach (var playModel in list)
            {
                VodPlays.Add(playModel);
            }

            sfListView.IsBusy = false;
        }

        private async void OnClickVodDetail(SfListView sfListView)
        {
            if (sfListView.CurrentItem is VodPlayModel cuItem)
            {
                await NavigationService.NavigateAsync(nameof(VodDetailPage), new NavigationParameters { { "Vod", cuItem } }, false, true);
            }
        }

        /// <summary>
        /// 获得视频列表
        /// </summary>
        /// <param name="vodTypeId"></param>
        /// <param name="vodYearId"></param>
        /// <param name="vodAreaId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        private static async Task<ObservableCollection<VodPlayModel>> GetVodPlays(string name, int pageIndex = 0, int pageSize = 25)
        {
            var vodListCondition = App.Database.Table<VodPlayDb>()
                .Where(x => x.VodName.ToUpper().Contains(name) ||
                            x.VodActor.ToUpper().Contains(name));

            var items = new ObservableCollection<VodPlayModel>();

            var list = await vodListCondition
                .OrderByDescending(x => x.CreateTime)
                .Skip(pageIndex * pageSize).Take(pageSize)
                .ToListAsync();

            list.ForEach(x =>
            {
                items.Add(new VodPlayModel
                {
                    Id = x.Id,
                    VodId = x.VodId,
                    VodName = x.VodName,
                    VodNamePinYin = x.VodNamePinYin,
                    VodPic = x.VodPic,
                    VodSub = x.VodSub,
                    VodDirector = x.VodDirector,
                    VodActor = x.VodActor,
                    VodArea = x.VodArea,
                    VodLang = x.VodLang,
                    VodYear = x.VodYear,
                    VodTypeName = x.VodTypeName,
                    VodContent = x.VodContent,
                    VodCreateTime = x.CreateTime
                });
            });

            return items;
        }
    }
}