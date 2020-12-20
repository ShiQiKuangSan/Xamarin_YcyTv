using Prism.Commands;
using Prism.Mvvm;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Prism.Navigation;
using Syncfusion.ListView.XForms;
using YcyTv.DataBase;
using YcyTv.Models;
using YcyTv.Views;

namespace YcyTv.ViewModels
{
    public class HistoryItemsPageViewModel : ViewModelBase
    {
        private ObservableCollection<VodPlayModel> _historyVodPlays;
        private bool _isHideData;
        private bool _isHideNoData;

        /// <summary>
        /// 视频列表
        /// </summary>
        public ObservableCollection<VodPlayModel> HistoryVodPlays
        {
            get => _historyVodPlays;
            set => SetProperty(ref _historyVodPlays, value);
        }


        public bool IsHideData
        {
            get => _isHideData;
            set => SetProperty(ref _isHideData, value);
        }

        public bool IsHideNoData
        {
            get => _isHideNoData;
            set => SetProperty(ref _isHideNoData, value);
        }

        /// <summary>
        /// 点击视频查看详情事件
        /// </summary>
        public DelegateCommand<SfListView> ClickVodDetailCommand { get; set; }

        /// <summary>
        /// 加载更多视频事件
        /// </summary>
        public DelegateCommand<SfListView> VodListLoadMoreCommand { get; set; }

        /// <summary>
        /// 当前页
        /// </summary>
        private int _pageIndex = 0;

        public HistoryItemsPageViewModel(INavigationService navigationService) : base(navigationService)
        {
            HistoryVodPlays = new ObservableCollection<VodPlayModel>();
            ClickVodDetailCommand = new DelegateCommand<SfListView>(OnClickVodDetail);
            VodListLoadMoreCommand = new DelegateCommand<SfListView>(OnVodListLoadMore);
            IsHideData = true;
            IsHideNoData = false;
        }

        public async Task InitData()
        {
            try
            {
                _pageIndex = 0;
                HistoryVodPlays = await GetVodPlays(_pageIndex);
            }
            catch (System.Exception)
            {
                // ignored
            }
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

            var list = await GetVodPlays(_pageIndex);

            var items = list.ToList();

            _oldvodlist = items.Any();

            foreach (var playModel in list)
            {
                HistoryVodPlays.Add(playModel);
            }

            sfListView.IsBusy = false;
        }

        /// <summary>
        /// 获得视频列表
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        private static async Task<ObservableCollection<VodPlayModel>> GetVodPlays(int pageIndex = 0, int pageSize = 30)
        {
            var items = new ObservableCollection<VodPlayModel>();

            var historyDbs = await App.Database.Table<VodPlayHistoryDb>()
                .OrderByDescending(x => x.PlayTime)
                .Skip(pageIndex * pageSize).Take(pageSize)
                .ToListAsync();

            if (!historyDbs.Any())
                return items;

            var list = new List<VodPlayDb>();

            foreach (var item in historyDbs)
            {
                var vodPlay = await App.Database.Table<VodPlayDb>()
                    .Where(x => x.VodId == item.VodId)
                    .FirstOrDefaultAsync();

                if (vodPlay != null)
                {
                    list.Add(vodPlay);
                }
            }

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

        /// <summary>
        /// 跳转视频详情
        /// </summary>
        /// <param name="sfListView"></param>
        private async void OnClickVodDetail(SfListView sfListView)
        {
            if (sfListView.CurrentItem is VodPlayModel cuItem)
            {
                await NavigationService.NavigateAsync(nameof(VodDetailPage), new NavigationParameters { { "Vod", cuItem } }, false, true);
            }
        }
    }
}
