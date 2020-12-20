using Prism.Mvvm;

namespace YcyTv.Models
{
    public class VodTypeModel : BindableBase
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

        /// <summary>
        /// 分类ID
        /// </summary>
        public long Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }
    }
}