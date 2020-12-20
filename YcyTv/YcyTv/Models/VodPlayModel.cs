using Prism.Mvvm;

namespace YcyTv.Models
{
    public class VodPlayModel : BindableBase
    {
        private long _id;
        private long _vodId;
        private string _vodName;
        private string _vodNamePinYin;
        private string _vodPic;
        private string _vodSub;
        private string _vodDirector;
        private string _vodActor;
        private string _vodArea;
        private string _vodLang;
        private string _vodYear;
        private string _vodTypeName;
        private string _vodContent;
        private string _vodCreateTime;

        /// <summary>
        /// 编号
        /// </summary>
        public long Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        /// <summary>
        /// 视频编号
        /// </summary>
        public long VodId
        {
            get => _vodId;
            set => SetProperty(ref _vodId, value);
        }

        /// <summary>
        /// 视频名称
        /// </summary>
        public string VodName
        {
            get => _vodName;
            set => SetProperty(ref _vodName, value);
        }

        /// <summary>
        /// 视频名称拼音
        /// </summary>
        public string VodNamePinYin
        {
            get => _vodNamePinYin;
            set => SetProperty(ref _vodNamePinYin, value);
        }
        
        /// <summary>
        /// 视频图片
        /// </summary>
        public string VodPic
        {
            get => _vodPic;
            set => SetProperty(ref _vodPic, value);
        }

        /// <summary>
        /// 视频别名
        /// </summary>
        public string VodSub
        {
            get => _vodSub;
            set => SetProperty(ref _vodSub, value);
        }

        /// <summary>
        /// 导演
        /// </summary>
        public string VodDirector
        {
            get => _vodDirector;
            set => SetProperty(ref _vodDirector, value);
        }

        /// <summary>
        /// 主演
        /// </summary>
        public string VodActor
        {
            get => _vodActor;
            set => SetProperty(ref _vodActor, value);
        }

        /// <summary>
        /// 地区
        /// </summary>
        public string VodArea
        {
            get => _vodArea;
            set => SetProperty(ref _vodArea, value);
        }

        /// <summary>
        /// 语言
        /// </summary>
        public string VodLang
        {
            get => _vodLang;
            set => SetProperty(ref _vodLang, value);
        }

        /// <summary>
        /// 年份
        /// </summary>
        public string VodYear
        {
            get => _vodYear;
            set => SetProperty(ref _vodYear, value);
        }

        /// <summary>
        /// 视频类型名称
        /// </summary>
        public string VodTypeName
        {
            get => _vodTypeName;
            set => SetProperty(ref _vodTypeName, value);
        }

        /// <summary>
        /// 视频介绍
        /// </summary>
        public string VodContent
        {
            get => _vodContent;
            set => SetProperty(ref _vodContent, value);
        }

        /// <summary>
        /// 视频爬取时间
        /// </summary>
        public string VodCreateTime
        {
            get => _vodCreateTime;
            set => SetProperty(ref _vodCreateTime, value);
        }
    }
}