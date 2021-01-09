using System;
using Android.Content;
using Android.Runtime;
using Android.Util;
using AndroidX.Activity;
using DkVideoPlayer.Ijk;
using DkVideoPlayer.VideoController;
using DkVideoPlayer.VideoController.component;
using YcyTv.Droid.Constants;
using YcyTv.Renderers.Constants;
using YcyTv.Renderers.ExtensionMethods;
using Timer = System.Timers.Timer;
using IjkVideoView = DkVideoPlayer.VideoPlayer.Player.VideoView;

namespace YcyTv.Droid.Widget
{
    [Register("YiciTV.Droid.Widget.VideoView")]
    public class VideoView : IjkVideoView
    {
        #region Properties

        /// <summary>
        /// Gets or sets the time elapsed interval.
        /// </summary>
        /// <value>
        /// The time elapsed interval.
        /// </value>
        public double TimeElapsedInterval
        {
            get => _timeElapsedInterval;
            set
            {
                if (value > 0)
                {
                    _timeElapsedInterval = value;
                    _timer.Interval = _timeElapsedInterval * 1000;
                }
                else
                {
                    _timer?.Stop();
                }
            }
        }

        /// <summary>
        /// Gets the state of the MediaPlayer.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public MediaPlayerStatus Status { get; private set; }

        /// <summary>
        /// Gets or sets the video scaling fill mode of the player on the view surface.
        /// </summary>
        /// <value>
        /// The fill mode.
        /// </value>
        public FillMode FillMode { get; set; }

        #endregion Properties

        #region Fields

        /// <summary>
        /// The timer used to control time elapsed event firings.
        /// </summary>
        private Timer _timer;

        /// <summary>
        /// The time elapsed interval.
        /// </summary>
        private double _timeElapsedInterval;

        private TitleView _titleView;
        private StandardVideoController controller;

        #endregion Fields

        #region Constructors

        public VideoView(Context p0, IAttributeSet p1, int p2) : base(p0, p1, p2)
        {
            Init(p0);
        }

        public VideoView(Context p0, IAttributeSet p1) : base(p0, p1)
        {
            Init(p0);
        }

        public VideoView(Context p0) : base(p0)
        {
            Init(p0);
        }

        #endregion Constructors

        #region Events

        /// <summary>
        /// Event notification fires when the video player's play button has been pressed.
        /// </summary>
        public event EventHandler Started;

        public event EventHandler Prepared;

        /// <summary>
        /// Event notification fires when the video player's play button has been pressed.
        /// </summary>
        public event EventHandler Paused;

        /// <summary>
        /// Event notification fires when the video player's stop button has been pressed.
        /// </summary>
        public event EventHandler Stopped;

        public event EventHandler Completion;

        public event EventHandler Error;

        /// <summary>
        /// Event notification fires when the video player's pause or stop button has been pressed.
        /// </summary>
        public event EventHandler TimeElapsed;

        #endregion Events

        #region Methods

        /// <summary>
        /// Initializes this VideoView instance.
        /// </summary>
        private void Init(Context context)
        {
            Status = MediaPlayerStatus.Idle;
            _timer = new Timer();
            _timer.Elapsed += (sender, args) => TimeElapsed.RaiseEvent(this);

            controller = new StandardVideoController(context);

            var completeView = new CompleteView(Context);
            var errorView = new ErrorView(Context);
            var prepareView = new PrepareView(Context);
            prepareView.SetClickStart();
            _titleView = new TitleView(Context) { Title = "" };
            
            controller.AddControlComponent(completeView, errorView, prepareView, _titleView);
            controller.AddControlComponent(new VodControlView(Context));
            controller.AddControlComponent(new GestureView(Context));
            
            controller.CanChangePosition = true;

            VideoController = controller;

            // Register Events  
            AddOnStateChangeListener(new StateChangeListener(this));
        }

        public void SetTitle(string title)
        {
            _titleView.Title = title;
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timer = null;

                Started = null;
                Paused = null;
                Stopped = null;
                TimeElapsed = null;
            }

            base.Dispose(disposing);
        }

        public override void Release()
        {
            base.Release();           
            Stopped.RaiseEvent(this);
        }
        

        private class StateChangeListener : IOnStateChangeListener
        {
            private readonly VideoView _videoView;

            public StateChangeListener(VideoView videoView)
            {
                this._videoView = videoView;
            }

            /// <summary>
            ///  播放屏幕状态
            /// </summary>
            /// <param name="playerState"></param>
            public void OnPlayerStateChanged(int playerState)
            {
                switch (playerState)
                {
                    case IjkVideoView.PLAYER_NORMAL:
                        //小屏
                        break;
                    case IjkVideoView.PLAYER_FULL_SCREEN:
                        //全屏
                        break;
                }
            }

            public void OnPlayStateChanged(int playState)
            {
                switch (playState)
                {
                    case IjkVideoView.STATE_IDLE:
                        _videoView.Status = MediaPlayerStatus.Idle;
                        _videoView.StopFullScreen();
                        break;
                    case IjkVideoView.STATE_PREPARING:
                        //在STATE_PREPARING时设置setMute(true)可实现静音播放
                        _videoView.Status = MediaPlayerStatus.Preparing;
                        break;
                    case IjkVideoView.STATE_PREPARED:
                        _videoView.Status = MediaPlayerStatus.Prepared;
                        _videoView.Prepared.RaiseEvent(this);
                        break;
                    case IjkVideoView.STATE_PLAYING:
                        _videoView.Status = MediaPlayerStatus.Playing;
                        _videoView.Started.RaiseEvent(this);
                        if (_videoView.TimeElapsedInterval > 0)
                            _videoView._timer?.Start();
                        break;
                    case IjkVideoView.STATE_PAUSED:
                        _videoView._timer?.Stop();    
                        _videoView.Status = MediaPlayerStatus.Paused;
                        _videoView.Paused.RaiseEvent(this);
                        break;
                    case IjkVideoView.STATE_BUFFERING:
                        break;
                    case IjkVideoView.STATE_BUFFERED:
                        break;
                    case IjkVideoView.STATE_PLAYBACK_COMPLETED:
                        _videoView._timer?.Stop();
                        _videoView.Status = MediaPlayerStatus.PlaybackCompleted;
                        _videoView.Completion.RaiseEvent(this);
                        break;
                    case IjkVideoView.STATE_ERROR:
                        _videoView._timer?.Stop();
                        _videoView.StopFullScreen();
                        _videoView.Status = MediaPlayerStatus.Error;
                        _videoView.Error.RaiseEvent(this);
                        break;
                }
            }
        }
        #endregion Methods
    }
}