using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;

using YcyTv;
using YcyTv.DataBase;

namespace YiciTV.Api
{
    public class OkzywApi
    {
        /// <summary>
        /// 视频列表api
        /// </summary>
        private const string VodListApi = "https://api.okzy.tv/api.php/provide/vod/?ac=list&t=";

        /// <summary>
        /// 视频详情api
        /// </summary>
        private const string VodDetailApi = "https://api.okzy.tv/api.php/provide/vod/at/json/?ac=detail&ids=";

        public static async Task StartVodPaChong()
        {
            var types = GetTvTypes().ToList();

            var updateList = new List<DicOkzywModel>();

            //获取第一页数据，得到最大页数
            foreach (var type in types)
            {
                Thread.Sleep(1000);

                //获得第一页数据
                var json = await GetFistPage(type.TypeId);

                if (string.IsNullOrWhiteSpace(json))
                    continue;

                try
                {
                    var obj = JsonConvert.DeserializeObject<OkzywModel>(json);

                    if (obj == null)
                        continue;

                    if (!obj.List.Any())
                        continue;

                    //保存了第一条数据
                    SaveVod(obj.List);

                    obj.CurrentPage++;

                    var cuPage = obj.CurrentPage;
                    var pageCount = obj.PageCount;

                    updateList.Add(new DicOkzywModel
                    {
                        TypeId = type.TypeId,
                        CurrentPage = cuPage,
                        PageCount = pageCount
                    });
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            var j = 0;

            //横向遍历二维数据
            while (true)
            {
                if (j >= updateList.Count - 1)
                {
                    if (updateList.All(x => x.CurrentPage >= x.PageCount))
                    {
                        break;
                    }

                    j = 0;
                }

                foreach (var item in updateList.Where(item => item.CurrentPage <= item.PageCount))
                {
                    Thread.Sleep(1000);

                    var client = new HttpClient();

                    var response = await client.GetAsync(VodListApi + item.TypeId + "&pg=" + item.CurrentPage);

                    var json = await response.Content.ReadAsStringAsync();

                    client.Dispose();

                    try
                    {
                        var obj = JsonConvert.DeserializeObject<OkzywModel>(json);

                        if (obj == null)
                        {
                            item.CurrentPage++;
                            continue;
                        }

                        if (!obj.List.Any())
                        {
                            item.CurrentPage++;
                            continue;
                        }

                        //保存了第一条数据
                        SaveVod(obj.List);
                        item.CurrentPage++;
                    }
                    catch (Exception)
                    {
                        // ignored
                        item.CurrentPage++;
                    }
                }

                j++;
            }
        }

        public static async Task<IEnumerable<VodPlayUrlDb>> UpdateVodDetailUrl(long vodId)
        {
            var vodPlayUrlList = new List<VodPlayUrlDb>();

            var vodPlayDb = await App.Database.Table<VodPlayDb>()
                .Where(x => x.VodId == vodId).FirstOrDefaultAsync();

            if (vodPlayDb == null)
                return vodPlayUrlList;

            //网络读取视频
            var detailList = await GetVodDetail(new List<long> { vodId });
            if (!detailList.Any())
                return vodPlayUrlList;

            var detail = detailList.First();

            //解析url
            vodPlayUrlList = GetPlayUrl(detail).ToList();

            vodPlayDb.CreateTime = string.IsNullOrWhiteSpace(detail.VodTime) ? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") : detail.VodTime; 

            await App.Database.Table<VodPlayUrlDb>()
                .Where(x => x.VodId == vodId)
                .DeleteAsync();

            await App.Database.InsertAllAsync(vodPlayUrlList);

            await App.Database.UpdateAsync(vodPlayDb);

            return vodPlayUrlList;
        }

        private static async Task<IList<OkzywDetailModel>> GetVodDetail(IEnumerable<long> vodIdList)
        {
            var items = new List<OkzywDetailModel>();
            if (!vodIdList.Any())
                return items;

            var client = new HttpClient();

            var ids = string.Join(",", vodIdList);

            var response = await client.GetAsync(VodDetailApi + ids);

            var json = await response.Content.ReadAsStringAsync();

            client.Dispose();

            if (string.IsNullOrWhiteSpace(json))
                return items;

            try
            {
                var obj = JsonConvert.DeserializeObject<OkzywDetailListModel>(json);

                if (obj == null)
                    return items;

                return !obj.List.Any() ? items : obj.List;
            }
            catch (Exception)
            {
                return items;
            }
        }

        private static async Task<string> GetFistPage(int typeId)
        {
            var client = new HttpClient();

            var response = await client.GetAsync(VodListApi + typeId);

            var json = await response.Content.ReadAsStringAsync();

            client.Dispose();

            return json;
        }

        private static IEnumerable<VodPlayUrlDb> GetPlayUrl(OkzywDetailModel detail)
        {
            var vodPlayUrlList = new List<VodPlayUrlDb>();

            var urls = detail.VodPlayUrl ?? string.Empty;
            if (string.IsNullOrWhiteSpace(urls))
                return vodPlayUrlList;


            var splits = urls?.Split('#').ToList();

            foreach (var x in splits)
            {
                var plays = x.Split('$').Where(item => !string.IsNullOrWhiteSpace(item)).ToList();

                if (plays.Count < 2)
                    continue;

                var m3 = plays.FirstOrDefault(item => item.EndsWith(".m3u8"));
                if (m3 == null)
                    continue;

                var index = plays.IndexOf(m3);

                index--;

                if (index < 0)
                    continue;

                vodPlayUrlList.Add(new VodPlayUrlDb
                {
                    VodId = detail.VodId,
                    PlayName = plays[index],
                    Play = m3
                });
            }


            return vodPlayUrlList;
        }

        private static async void SaveVod(IEnumerable<OkzywListModel> list)
        {
            var vodIds = list.Select(x => x.VodId).ToList();

            //添加视频数据到数据库
            var details = await GetVodDetail(vodIds);

            if (!details.Any())
                return;

            var listDb = new List<VodPlayDb>();
            var vodPlayUrlList = new List<VodPlayUrlDb>();

            foreach (var detail in details)
            {
                var vodPlay = await App.Database.Table<VodPlayDb>()
                    .FirstOrDefaultAsync(x => x.VodId == detail.VodId);

                if (vodPlay != null)
                {
                    vodPlay.CreateTime = string.IsNullOrWhiteSpace(detail.VodTime) ? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") : detail.VodTime;
                    //更新集数
                    var vodPlayUrls = GetPlayUrl(detail).ToList();

                    await App.Database.Table<VodPlayUrlDb>()
                        .Where(x => x.VodId == vodPlay.VodId)
                        .DeleteAsync();

                    await App.Database.InsertAllAsync(vodPlayUrls);

                    await App.Database.UpdateAsync(vodPlay);
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(detail.VodTypeName))
                    {
                        detail.VodTypeName = "未知";
                    }

                    if (string.IsNullOrWhiteSpace(detail.VodArea))
                    {
                        detail.VodArea = "未知";
                    }

                    if (string.IsNullOrWhiteSpace(detail.VodYear))
                    {
                        detail.VodYear = "未知";
                    }

                    if (detail.VodYear.Length > 4)
                    {
                        detail.VodYear = detail.VodYear.Substring(0, 4);
                    }

                    long.TryParse(detail.VodYear, out var year);

                    if (year < 2000)
                    {
                        detail.VodYear = "1999";
                    }

                    //添加分类
                    var vodPlayType = await App.Database.Table<VodPlayTypeDb>()
                        .Where(x => x.Name == detail.VodTypeName).FirstOrDefaultAsync();

                    if (vodPlayType == null)
                    {
                        //添加类型
                        vodPlayType = new VodPlayTypeDb
                        {
                            Name = detail.VodTypeName
                        };

                        await App.Database.InsertAsync(vodPlayType);
                    }

                    var vodPlayArea = await App.Database.Table<VodPlayAreaDb>()
                        .Where(x => x.Name == detail.VodArea).FirstOrDefaultAsync();

                    if (vodPlayArea == null)
                    {
                        //添加类型
                        vodPlayArea = new VodPlayAreaDb
                        {
                            Name = detail.VodArea
                        };

                        await App.Database.InsertAsync(vodPlayArea);
                    }

                    var vodPlayYear = await App.Database.Table<VodPlayYearDb>()
                        .Where(x => x.Name == detail.VodYear).FirstOrDefaultAsync();

                    if (vodPlayYear == null)
                    {
                        //添加类型
                        vodPlayYear = new VodPlayYearDb
                        {
                            Name = detail.VodYear
                        };

                        await App.Database.InsertAsync(vodPlayYear);
                    }

                    vodPlay = new VodPlayDb
                    {
                        ClassifyId = vodPlayType.Id,
                        VodId = detail.VodId,
                        VodName = detail.VodName,
                        VodPlayTypeId = vodPlayType.Id,
                        VodTypeName = vodPlayType.Name,
                        VodNamePinYin = Constants.Convert(detail.VodName),
                        VodPic = detail.VodPic,
                        VodSub = detail.VodSub,
                        VodDirector = detail.VodDirector,
                        VodActor = detail.VodActor,
                        VodAreaId = vodPlayArea.Id,
                        VodArea = detail.VodArea,
                        VodLang = detail.VodLang,
                        VodYearId = vodPlayYear.Id,
                        VodYear = detail.VodYear,
                        VodContent = detail.VodContent,
                        CreateTime = string.IsNullOrWhiteSpace(detail.VodTime) ? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") : detail.VodTime
                    };

                    var vodPlayUrls = GetPlayUrl(detail).ToList();

                    if (!vodPlayUrls.Any())
                        continue;

                    listDb.Add(vodPlay);
                    vodPlayUrlList.AddRange(vodPlayUrls);
                }
            }

            if (!vodPlayUrlList.Any() || !listDb.Any())
                return;

            await App.Database.InsertAllAsync(vodPlayUrlList);
            await App.Database.InsertAllAsync(listDb);
        }

        private static IEnumerable<VodType> GetTvTypes()
        {
            return new List<VodType>
            {
                new VodType
                {
                    Name = "韩国剧",
                    TypeId = 15
                },
                new VodType
                {
                    Name = "日韩动漫",
                    TypeId = 30
                },
                new VodType
                {
                    Name = "动作片",
                    TypeId = 6
                },
                new VodType
                {
                    Name = "喜剧片",
                    TypeId = 7
                },
                new VodType
                {
                    Name = "爱情片",
                    TypeId = 8
                },
                new VodType
                {
                    Name = "科幻片",
                    TypeId = 9
                },
                new VodType
                {
                    Name = "恐怖片",
                    TypeId = 10
                },
                new VodType
                {
                    Name = "剧情片",
                    TypeId = 11
                },
                new VodType
                {
                    Name = "战争片",
                    TypeId = 12
                },
                new VodType
                {
                    Name = "国产剧",
                    TypeId = 13
                },
                new VodType
                {
                    Name = "香港剧",
                    TypeId = 14
                },
                new VodType
                {
                    Name = "欧美剧",
                    TypeId = 16
                },
                new VodType
                {
                    Name = "台湾剧",
                    TypeId = 22
                },
                new VodType
                {
                    Name = "日本剧",
                    TypeId = 23
                },
                new VodType
                {
                    Name = "海外剧",
                    TypeId = 24
                },
                new VodType
                {
                    Name = "内地综艺",
                    TypeId = 25
                },
                new VodType
                {
                    Name = "港台综艺",
                    TypeId = 26
                },
                new VodType
                {
                    Name = "日韩综艺",
                    TypeId = 27
                },
                new VodType
                {
                    Name = "欧美综艺",
                    TypeId = 28
                },
                new VodType
                {
                    Name = "国产动漫",
                    TypeId = 29
                },

                new VodType
                {
                    Name = "欧美动漫",
                    TypeId = 31
                },
                new VodType
                {
                    Name = "港台动漫",
                    TypeId = 32
                },
                new VodType
                {
                    Name = "海外动漫",
                    TypeId = 33
                },
            };
        }


        private class DicOkzywModel
        {
            public int TypeId { get; set; }

            public int CurrentPage { get; set; }

            public int PageCount { get; set; }
        }

        private class VodType
        {
            /// <summary>
            /// 分类名称
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// 分类ID
            /// </summary>
            public int TypeId { get; set; }
        }

        public class OkzywModel
        {
            /// <summary>
            /// 数据条数
            /// </summary>
            [JsonProperty("limit")]
            public int Limit { get; set; }

            /// <summary>
            /// 当前页码
            /// </summary>
            [JsonProperty("page")]
            public int CurrentPage { get; set; }

            /// <summary>
            /// 总页数
            /// </summary>
            [JsonProperty("pagecount")]
            public int PageCount { get; set; }

            /// <summary>
            /// 总视频数
            /// </summary>
            [JsonProperty("total")]
            public int Total { get; set; }

            /// <summary>
            /// 视频列表
            /// </summary>
            [JsonProperty("list")]
            public List<OkzywListModel> List { get; set; } = new List<OkzywListModel>();
        }

        public class OkzywListModel
        {
            /// <summary>
            /// 视频编号
            /// </summary>
            [JsonProperty("vod_id")]
            public long VodId { get; set; }
        }

        public class OkzywDetailListModel
        {
            /// <summary>
            /// 详情数据
            /// </summary>
            [JsonProperty("list")]
            public List<OkzywDetailModel> List { get; set; } = new List<OkzywDetailModel>();
        }

        public class OkzywDetailModel
        {
            /// <summary>
            /// 分类编号
            /// </summary>
            [JsonProperty("type_id")]
            public int TypeId { get; set; }

            /// <summary>
            /// 视频名
            /// </summary>
            [JsonProperty("vod_name")]
            public string VodName { get; set; }

            /// <summary>
            /// 视频图片
            /// </summary>
            [JsonProperty("vod_pic")]
            public string VodPic { get; set; }

            /// <summary>
            /// 备注
            /// </summary>
            [JsonProperty("vod_remarks")]
            public string VodRemarks { get; set; }

            /// <summary>
            /// 别名
            /// </summary>
            [JsonProperty("vod_sub")]
            public string VodSub { get; set; }

            /// <summary>
            /// 导演
            /// </summary>
            [JsonProperty("vod_director")]
            public string VodDirector { get; set; }

            /// <summary>
            /// 年份
            /// </summary>
            [JsonProperty("vod_year")]
            public string VodYear { get; set; }

            /// <summary>
            /// 主演
            /// </summary>
            [JsonProperty("vod_actor")]
            public string VodActor { get; set; }

            [JsonProperty("vod_area")] public string VodArea { get; set; }

            /// <summary>
            /// 电影类型
            /// </summary>
            [JsonProperty("type_name")]
            public string VodTypeName { get; set; }

            /// <summary>
            /// 视频介绍
            /// </summary>
            [JsonProperty("vod_content")]
            public string VodContent { get; set; }

            /// <summary>
            /// 视频下载地址，需要解析
            /// </summary>
            [JsonProperty("vod_down_url")]
            public string VodDownUrl { get; set; } = string.Empty;

            /// <summary>
            /// 视频播放地址，需要解析
            /// </summary>
            [JsonProperty("vod_play_url")]
            public string VodPlayUrl { get; set; }

            /// <summary>
            /// 视频时间
            /// </summary>
            [JsonProperty("vod_time")]
            public string VodTime { get; set; }

            /// <summary>
            /// 视频集数
            /// </summary>
            [JsonProperty("vod_serial")]
            public int VodSerial { get; set; }

            /// <summary>
            /// 语种
            /// </summary>
            [JsonProperty("vod_lang")]
            public string VodLang { get; set; }

            /// <summary>
            /// 视频编号
            /// </summary>
            [JsonProperty("vod_id")]
            public int VodId { get; set; }

            /// <summary>
            /// 评分
            /// </summary>
            [JsonProperty("vod_douban_score")]
            public string VodScore { get; set; }
        }
    }
}