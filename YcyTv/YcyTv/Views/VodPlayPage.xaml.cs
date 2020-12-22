using System;

using Xamarin.Forms;

using YcyTv.DataBase;
using YcyTv.Renderers.Constants;
using YcyTv.Renderers.Events;
using YcyTv.ViewModels;

namespace YcyTv.Views
{
    public partial class VodPlayPage : ContentPage
    {
        private bool _isInit = true;
        public VodPlayPage()
        {
            InitializeComponent();
        }

        private async void VideoPlayer_OnTimeElapsed(object sender, VideoPlayerEventArgs e)
        {
            //每30秒保存一次当前的播放进度
            if (BindingContext is VodPlayPageViewModel vm && vm.Vod != null && vm.CuUrl != null)
            {
                var index = vm.Urls.IndexOf(vm.CuUrl);

                if (index == -1)
                    return;

                var history = await App.Database.Table<VodPlayHistoryDb>()
                    .Where(x => x.VodId == vm.Vod.VodId)
                    .FirstOrDefaultAsync();

                if (history != null)
                {
                    history.SeekTime = (int)e.CurrentTime.TotalSeconds;
                    history.PlayIndex = index;
                    history.PlayTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    await App.Database.UpdateAsync(history);
                }
            }
        }

        private async void VodPlayer_OnPlayerStateChanged(object sender, VideoPlayerStateChangedEventArgs e)
        {
            if (e.CurrentState == PlayerState.Playing && BindingContext is VodPlayPageViewModel vm && vm.Vod != null && vm.CuUrl != null)
            {
                var index = vm.Urls.IndexOf(vm.CuUrl);

                //跳转到30秒的地方
                var history = await App.Database.Table<VodPlayHistoryDb>()
                    .Where(x => x.VodId == vm.Vod.VodId)
                    .FirstOrDefaultAsync();

                if (history == null)
                {
                    //添加初始记录
                    history = new VodPlayHistoryDb
                    {
                        VodId = vm.Vod.VodId,
                        PlayIndex = index,
                        SeekTime = 0
                    };

                    await App.Database.InsertAsync(history);
                }

                if (history.PlayIndex != index)
                {
                    history.SeekTime = 0;
                    history.PlayIndex = index;
                    history.PlayTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    await App.Database.UpdateAsync(history);
                }

                if (history.SeekTime > 0 && _isInit)
                {
                    //跳转到历史观看位置
                    VodPlayer.Seek(history.SeekTime);
                }
                
                _isInit = false;
            }
        }

        /// <summary>
        /// 视频播放完成，自动播放下一集
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VodPlayer_Completed(object sender, VideoPlayerEventArgs e)
        {
            if (BindingContext is VodPlayPageViewModel vm)
            {
                var index = vm.Urls.IndexOf(vm.CuUrl);

                if (index == -1)
                    return;

                index++;

                if (index > vm.Urls.Count - 1)
                    return;

                var url = vm.Urls[index];

                _isInit = true;
                
                vm.CuUrl = url;
            }
        }

        protected override void OnDisappearing()
        {
            VodPlayer?.Release();
            base.OnDisappearing();
        }
                      
        protected override bool OnBackButtonPressed()
        {
            if (!VodPlayer.OnBackPressed())
            {
                return base.OnBackButtonPressed();
            }

            return false;
        }
    }
}
