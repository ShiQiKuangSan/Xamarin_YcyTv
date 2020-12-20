using SQLite;

namespace YcyTv.DataBase
{
    /// <summary>
    /// 影片区域
    /// </summary>
    public class VodPlayAreaDb
    {
        [PrimaryKey, AutoIncrement]
        public long Id { get; set; }

        public string Name { get; set; }
    }
}