using Prism.Mvvm;

namespace YcyTv.Models
{
    public class VodPlayUrlModel : BindableBase
    {
        private string _playName;
        private string _play;

        public string PlayName
        {
            get => _playName;
            set => SetProperty(ref _playName, value);
        }

        public string Play
        {
            get => _play;
            set => SetProperty(ref _play, value);
        }
    }
}