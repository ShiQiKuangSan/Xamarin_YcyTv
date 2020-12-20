using Prism.Commands;
using Prism.Mvvm;

using System;
using System.Collections.Generic;
using System.Linq;
using Prism.Navigation;
using YcyTv.Models;

namespace YcyTv.ViewModels
{
    public class VodPlayPageViewModel : ViewModelBase
    {
        private VodPlayModel _vod;
        private VodPlayUrlModel _cuUrl;
        private List<VodPlayUrlModel> _urls;
        private string _vodTitle;

        public VodPlayUrlModel CuUrl { get => _cuUrl; set => SetProperty(ref _cuUrl, value); }
        public List<VodPlayUrlModel> Urls { get => _urls; set => SetProperty(ref _urls, value); }
        public VodPlayModel Vod { get => _vod; set => SetProperty(ref _vod, value); }

        public string VodTitle { get => _vodTitle; set => SetProperty(ref _vodTitle, value); }

        public VodPlayPageViewModel(INavigationService navigationService) : base(navigationService)
        {
        }

        public override void Initialize(INavigationParameters parameters)
        {
            if (parameters.ContainsKey("Vod") && parameters.ContainsKey("CuUrl") && parameters.ContainsKey("Urls"))
            {
                Vod = parameters.GetValue<VodPlayModel>("Vod");
                CuUrl = parameters.GetValue<VodPlayUrlModel>("CuUrl");
                Urls = parameters.GetValue<List<VodPlayUrlModel>>("Urls");
                Title = Vod.VodName;
                
                var i = Urls.IndexOf(CuUrl);
                if (i > 0)
                {
                    i++;
                    VodTitle = $"{Vod.VodName}  第 {i} 集";
                }
                else
                {
                    VodTitle = $"{Vod.VodName}";
                }

            }

        }
    }
}
