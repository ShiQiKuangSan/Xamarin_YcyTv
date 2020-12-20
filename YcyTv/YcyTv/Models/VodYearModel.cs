using Prism.Mvvm;

namespace YcyTv.Models
{
    public class VodYearModel : BindableBase
    {
        private string _name;
        private long _id;

        /// <summary>
        /// 分类名称
        /// </summary>
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public long Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }
    }
}