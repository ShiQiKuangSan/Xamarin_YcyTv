using Prism.Commands;
using Prism.Mvvm;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Prism.Navigation;
using Syncfusion.ListView.XForms;
using Xamarin.Essentials;
using YcyTv.DataBase;
using YcyTv.Extensions;
using YcyTv.Models;
using YcyTv.Views;

namespace YcyTv.ViewModels
{
    public class TVListPageViewModel : ViewModelBase
    {
        private ObservableCollection<VodTypeModel> _vodTypes;
        private ObservableCollection<VodYearModel> _vodYears;
        private ObservableCollection<VodAreaModel> _vodAreas;
        private ObservableCollection<VodPlayModel> _vodPlays;

        /// <summary>
        /// 分类列表
        /// </summary>
        public ObservableCollection<VodTypeModel> VodTypes
        {
            get => _vodTypes;
            set => SetProperty(ref _vodTypes, value);
        }

        /// <summary>
        /// 年份列表
        /// </summary>
        public ObservableCollection<VodYearModel> VodYears
        {
            get => _vodYears;
            set => SetProperty(ref _vodYears, value);
        }

        /// <summary>
        /// 区域列表
        /// </summary>
        public ObservableCollection<VodAreaModel> VodAreas
        {
            get => _vodAreas;
            set => SetProperty(ref _vodAreas, value);
        }

        /// <summary>
        /// 视频列表
        /// </summary>
        public ObservableCollection<VodPlayModel> VodPlays
        {
            get => _vodPlays;
            set => SetProperty(ref _vodPlays, value);
        }

        /// <summary>
        /// 电影分类被选中
        /// </summary>
        public DelegateCommand<SfListView> VodTypeSelectionCommand { get; set; }

        /// <summary>
        /// 加载更多视频事件
        /// </summary>
        public DelegateCommand<SfListView> VodListLoadMoreCommand { get; set; }

        /// <summary>
        /// 点击视频查看详情事件
        /// </summary>
        public DelegateCommand<SfListView> ClickVodDetailCommand { get; set; }

        //选中的类型id
        private long _vodTypeId = 0;

        private long _vodAreaId = 0;
        private long _vodYearId = 0;

        /// <summary>
        /// 当前页
        /// </summary>
        private int _pageIndex = 0;

        public TVListPageViewModel(INavigationService navigationService) : base(navigationService)
        {
            VodTypes = new ObservableCollection<VodTypeModel>
            {
                new VodTypeModel
                {
                    Id = 0,
                    Name = "全部"
                }
            };
            VodYears = new ObservableCollection<VodYearModel>
            {
                new VodYearModel
                {
                    Id = 0,
                    Name = "全部"
                }
            };
            VodAreas = new ObservableCollection<VodAreaModel>
            {
                new VodAreaModel
                {
                    Id = 0,
                    Name = "全部"
                }
            };
            VodPlays = new ObservableCollection<VodPlayModel>();

            VodTypeSelectionCommand = new DelegateCommand<SfListView>(OnVodTypeSelection);
            VodListLoadMoreCommand = new DelegateCommand<SfListView>(OnVodListLoadMore);
            ClickVodDetailCommand = new DelegateCommand<SfListView>(OnClickVodDetail);
            InitData().SafeFireAndForget(false);
        }

        public async Task InitData()
        {
            try
            {
                if (VodYears.Count <= 1)
                {
                    var years = await App.Database.Table<VodPlayYearDb>().OrderByDescending(x => x.Name).ToListAsync();

                    years.ForEach(x => { VodYears.Add(new VodYearModel { Id = x.Id, Name = x.Name }); });
                }

                if (VodAreas.Count <= 1)
                {
                    var areas = await App.Database.Table<VodPlayAreaDb>().ToListAsync();

                    areas.ForEach(x => { VodAreas.Add(new VodAreaModel { Id = x.Id, Name = x.Name }); });
                }

                if (VodTypes.Count <= 1)
                {
                    var types = await App.Database.Table<VodPlayTypeDb>().ToListAsync();

                    types.ForEach(x => { VodTypes.Add(new VodTypeModel { Id = x.Id, Name = x.Name }); });
                }

                _pageIndex = 0;
            }
            catch (System.Exception)
            {
                // ignored
            }
        }


        public override async void Initialize(INavigationParameters parameters)
        {
            _pageIndex = 0;
            try
            {
                VodPlays = await GetVodPlays(_pageIndex);

            }
            catch (Exception)
            {
                // ignored
            }
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

            var list = await GetVodPlays(_pageIndex, 25, _vodTypeId, _vodYearId, _vodAreaId);

            var items = list.ToList();

            _oldvodlist = items.Any();

            foreach (var playModel in list)
            {
                VodPlays.Add(playModel);
            }

            sfListView.IsBusy = false;
        }

        /// <summary>
        /// 选中分类
        /// </summary>
        /// <param name="sfListView"></param>
        private async void OnVodTypeSelection(SfListView sfListView)
        {
            var cuItem = sfListView.CurrentItem;

            switch (cuItem)
            {
                case VodTypeModel typeModel:
                    _vodTypeId = typeModel.Id;
                    break;

                case VodAreaModel areaModel:
                    _vodAreaId = areaModel.Id;
                    break;

                case VodYearModel yearModel:
                    _vodYearId = yearModel.Id;
                    break;
            }

            _pageIndex = 0;
            _oldvodlist = true;
            VodPlays = await GetVodPlays(_pageIndex, 25, _vodTypeId, _vodYearId, _vodAreaId);
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
        private static async Task<ObservableCollection<VodPlayModel>> GetVodPlays(int pageIndex = 0, int pageSize = 25,
            long vodTypeId = 0, long vodYearId = 0,
            long vodAreaId = 0
        )
        {
            var vodListCondition = App.Database.Table<VodPlayDb>();

            if (vodTypeId > 0)
            {
                vodListCondition = vodListCondition.Where(x => x.ClassifyId == vodTypeId);
            }

            if (vodYearId > 0)
            {
                vodListCondition = vodListCondition.Where(x => x.VodYearId == vodYearId);
            }

            if (vodAreaId > 0)
            {
                vodListCondition = vodListCondition.Where(x => x.VodAreaId == vodAreaId);
            }

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
