using System;
using SQLite;

namespace YcyTv.DataBase
{
    /// <summary>
    /// 视频列表
    /// </summary>
    public class VodPlayDb
    {
        [PrimaryKey, AutoIncrement]
        public long Id { get; set; }

        /// <summary>
        /// 分类ID
        /// </summary>
        [Indexed]
        public long ClassifyId { get; set; }

        /// <summary>
        /// 视频编号
        /// </summary>
        [Indexed]
        public long VodId { get; set; }

        /// <summary>
        /// 影片类型编号 <![CDATA[VodPlayTypeDb]]>
        /// </summary>
        [Indexed]
        public long VodPlayTypeId { get; set; }

        /// <summary>
        /// 影片年份编号 <![CDATA[VodPlayYearDb]]>
        /// </summary>
        [Indexed]
        public long VodYearId { get; set; }

        /// <summary>
        /// 影片年份编号 <![CDATA[VodPlayAreaDb]]>
        /// </summary>
        [Indexed]
        public long VodAreaId { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string VodName { get; set; }

        /// <summary>
        /// 名称的拼音
        /// </summary>
        public string VodNamePinYin { get; set; }

        /// <summary>
        /// 图片
        /// </summary>
        public string VodPic { get; set; }

        /// <summary>
        /// 别名
        /// </summary>
        public string VodSub { get; set; }

        /// <summary>
        /// 导演
        /// </summary>
        public string VodDirector { get; set; }

        /// <summary>
        /// 主演
        /// </summary>
        public string VodActor { get; set; }

        /// <summary>
        /// 地区
        /// </summary>
        public string VodArea { get; set; }

        /// <summary>
        /// 语言
        /// </summary>
        public string VodLang { get; set; }

        /// <summary>
        /// 上映时间
        /// </summary>
        public string VodYear { get; set; }

        /// <summary>
        /// 电影类型
        /// </summary>
        public string VodTypeName { get; set; }

        /// <summary>
        /// 视频介绍
        /// </summary>
        public string VodContent { get; set; }

        /// <summary>
        /// 数据创建时间
        /// </summary>
        public string CreateTime { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
}