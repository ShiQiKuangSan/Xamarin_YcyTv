using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace YcyTv.Api
{
    public class DownloadHelper
    {
        private int ByteSize = 1024;

        /// <summary>
        /// 下载中的后缀，下载完成去掉
        /// </summary>
        private const string Suffix = ".downloading";

        public event Action<long> ShowDownloadPercent;

        public event Action<long> SetDownloadMaxFileLength;

        /// <summary>
        /// Http方式下载文件
        /// </summary>
        /// <param name="url">http地址</param>
        /// <param name="localfile">本地文件</param>
        /// <returns></returns>
        public async Task<int> DownloadFile(string url, string localfile, bool isFug = false)
        {
            var ret = 0;
            var localfileReal = localfile;
            var localfileWithSuffix = localfileReal + Suffix;

            try
            {
                long startPosition = 0;
                FileStream writeStream;

                if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(localfileReal))
                    return 1;

                if (isFug && File.Exists(localfileReal))
                {
                    File.Delete(localfileReal);
                }
                else if (File.Exists(localfileReal))
                {
                    return 0;
                }

                //取得远程文件长度
                var remoteFileLength = await GetHttpLength(url);
                if (remoteFileLength == 0)
                    return 2;

                SetDownloadMaxFileLength?.Invoke(remoteFileLength);

                //判断要下载的文件是否存在
                if (File.Exists(localfileWithSuffix))
                {
                    writeStream = File.OpenWrite(localfileWithSuffix);
                    startPosition = writeStream.Length;
                    if (startPosition > remoteFileLength)
                    {
                        writeStream.Close();
                        File.Delete(localfileWithSuffix);
                        writeStream = new FileStream(localfileWithSuffix, FileMode.Create);
                    }
                    else if (startPosition == remoteFileLength)
                    {
                        DownloadFileOk(localfileReal, localfileWithSuffix);
                        writeStream.Close();
                        return 0;
                    }
                    else
                        writeStream.Seek(startPosition, SeekOrigin.Begin);
                }
                else
                    writeStream = new FileStream(localfileWithSuffix, FileMode.Create);

                WebResponse rsp = null;
                try
                {
                    var request = WebRequest.Create(url);

                    if (request is HttpWebRequest req)
                    {
                        if (startPosition > 0)
                            req.AddRange((int)startPosition);

                        rsp = await req.GetResponseAsync();
                        using (var readStream = rsp.GetResponseStream())
                        {
                            var btArray = new byte[ByteSize];
                            var currPostion = startPosition;
                            var contentSize = 0;
                            var index = 0;
                            while (readStream != null && (contentSize = await readStream.ReadAsync(btArray, 0, btArray.Length)) > 0)
                            {
                                index++;
                                await writeStream.WriteAsync(btArray, 0, contentSize);
                                currPostion += contentSize;

                                if (index >= 1024)
                                {
                                    ShowDownloadPercent?.Invoke(currPostion);
                                    index = 0;
                                }
                            }
                        }

                        req.Abort();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("获取远程文件失败！exception：\n" + ex.ToString());
                    ret = 3;
                    throw new Exception("获取远程文件失败[001] exception：\n" + ex.ToString());
                }
                finally
                {
                    writeStream.Close();
                    rsp?.Close();

                    if (ret == 0)
                        DownloadFileOk(localfileReal, localfileWithSuffix);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("获取远程文件失败！exception：\n" + ex.ToString());
                ret = 4;
                throw new Exception("获取远程文件失败[002] exception：\n" + ex.ToString());
            }

            return ret;
        }

        /// <summary>
        /// 下载完成
        /// </summary>
        private void DownloadFileOk(string localfileReal, string localfileWithSuffix)
        {
            try
            {
                //去掉.downloading后缀
                var fi = new FileInfo(localfileWithSuffix);
                fi.MoveTo(localfileReal);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw new Exception("获取远程文件失败[003] exception：\n" + ex.ToString());
            }
            finally
            {
                //通知完成
                ShowDownloadPercent?.Invoke(100);
            }
        }

        // 从文件头得到远程文件的长度
        private static async Task<long> GetHttpLength(string url)
        {
            long length = 0;
            WebRequest req = null;
            try
            {
                req = WebRequest.Create(url);
                var response = await req.GetResponseAsync();
                if (response is HttpWebResponse rsp)
                {
                    if (rsp.StatusCode == HttpStatusCode.OK)
                        length = rsp.ContentLength;

                    rsp.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("获取远程文件大小失败！exception：\n" + ex.ToString());
                throw new Exception("获取远程文件大小失败[001] exception：\n" + ex.ToString());
            }
            finally
            {
                req?.Abort();
            }

            return length;
        }
    }
}