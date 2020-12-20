using System;
using SQLite;

namespace YcyTv.DataBase
{
    /// <summary>
    /// 视频历史记录
    /// </summary>
    public class VodPlayHistoryDb
    {
        [PrimaryKey, AutoIncrement]
        public long Id { get; set; }

        /// <summary>
        /// 视频编号
        /// </summary>
        [Indexed]
        public long VodId { get; set; }

        /// <summary>
        /// 集数下标
        /// </summary>
        [Indexed]
        public int PlayIndex { get; set; }

        /// <summary>
        /// 跳过的时间
        /// </summary>
        public int SeekTime { get; set; } = 0;

        /// <summary>
        /// 记录创建时间
        /// </summary>
        public string PlayTime { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
}