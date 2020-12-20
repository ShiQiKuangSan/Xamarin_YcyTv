using SQLite;

namespace YcyTv.DataBase
{
    /// <summary>
    /// 视频播放地址表
    /// </summary>
    public class VodPlayUrlDb
    {
        /// <summary>
        /// TvVodPlayDb 自增 ID
        /// </summary>
        [Indexed]
        public int VodId { get; set; }

        /// <summary>
        /// 集数名称
        /// </summary>
        [Indexed]
        public string PlayName { get; set; }

        /// <summary>
        /// 视频地址。
        /// </summary>
        public string Play { get; set; }
    }
}