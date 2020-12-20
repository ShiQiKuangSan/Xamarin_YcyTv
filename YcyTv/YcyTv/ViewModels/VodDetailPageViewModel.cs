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
    public class VodDetailPageViewModel : ViewModelBase
    {
        private VodPlayModel _vod;
        private ObservableCollection<VodPlayUrlModel> _playUrls;

        public VodPlayModel Vod
        {
            get => _vod;
            set => SetProperty(ref _vod, value);
        }

        public ObservableCollection<VodPlayUrlModel> PlayUrls
        {
            get => _playUrls;
            set => SetProperty(ref _playUrls, value);
        }

        /// <summary>
        /// 播放事件
        /// </summary>
        public DelegateCommand<SfListView> VodPlayCommand { get; set; }

        public VodDetailPageViewModel(INavigationService navigationService) : base(navigationService)
        {
            PlayUrls = new ObservableCollection<VodPlayUrlModel>();
            VodPlayCommand = new DelegateCommand<SfListView>(OnVodPlay);
        }

        public override async void Initialize(INavigationParameters parameters)
        {
            if (parameters.ContainsKey("Vod"))
            {
                Vod = parameters.GetValue<VodPlayModel>("Vod");
                if (!Vod.VodDirector.StartsWith("主演"))
                {
                    Vod.VodDirector = "主演：" + Vod.VodDirector;
                }

                Vod.VodYear = "年份：" + Vod.VodYear;
                Vod.VodArea = "地区：" + Vod.VodArea;
                Vod.VodContent = "介绍：" + Vod.VodContent;

                PlayUrls = await GetPlayUrls(Vod.VodId);
            }
        }

        private async void OnVodPlay(SfListView sfListView)
        {
            if (sfListView.CurrentItem is VodPlayUrlModel cuItem)
            {
                var urls = PlayUrls.ToList();

                await NavigationService.NavigateAsync(nameof(VodPlayPage), new NavigationParameters { { "Vod", Vod }, { "CuUrl", cuItem }, { "Urls", urls } }, false, true);
            }
        }

        /// <summary>
        /// 读取数据库中的视频地址
        /// </summary>
        /// <param name="vodId"></param>
        /// <returns></returns>
        private static async Task<ObservableCollection<VodPlayUrlModel>> GetPlayUrls(long vodId)
        {
            var items = new ObservableCollection<VodPlayUrlModel>();

            var dbList = await App.Database.Table<VodPlayUrlDb>()
                .Where(x => x.VodId == vodId)
                .ToListAsync();

            dbList.ForEach(x =>
            {
                items.Add(new VodPlayUrlModel
                {
                    Play = x.Play,
                    PlayName = x.PlayName
                });
            });

            return items;
        }
    }
}
