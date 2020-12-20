using Prism.Mvvm;
using Prism.Navigation;

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
        public virtual void Initialize(INavigationParameters parameters)
        {
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
