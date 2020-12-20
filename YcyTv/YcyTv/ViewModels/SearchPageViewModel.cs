using Prism.Commands;
using Prism.Mvvm;

using System;
using System.Collections.Generic;
using System.Linq;
using Prism.Navigation;

namespace YcyTv.ViewModels
{
    public class SearchPageViewModel : ViewModelBase
    {
        public SearchPageViewModel(INavigationService navigationService) : base(navigationService)
        {
        }
    }
}
