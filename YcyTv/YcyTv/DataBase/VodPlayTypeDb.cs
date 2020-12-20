using SQLite;

namespace YcyTv.DataBase
{
    /// <summary>
    /// 影片类型
    /// </summary>
    public class VodPlayTypeDb
    {
        [PrimaryKey, AutoIncrement]
        public long Id { get; set; }

        /// <summary>
        /// 影片类型名称
        /// </summary>
        public string Name { get; set; }
    }
}