using SQLite;

namespace YcyTv.DataBase
{
    /// <summary>
    /// 影片年份
    /// </summary>
    public class VodPlayYearDb
    {
        [PrimaryKey, AutoIncrement]
        public long Id { get; set; }

        public string Name { get; set; }
    }
}