using System;
using System.IO;
using System.Text;
using Microsoft.International.Converters.PinYinConverter;
using SQLite;
using Xamarin.Forms;

namespace YcyTv
{
    public class Constants
    {
        private const string DatabaseFilename = "YiciTv.db3";

        public const SQLiteOpenFlags Flags = SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create |
                                              SQLiteOpenFlags.SharedCache;

        public static string DatabasePath
        {
            get
            {
                switch (Device.RuntimePlatform)
                {
                    case Device.iOS:
                        {
                            var docFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                            var libFolder = Path.Combine(docFolder, "..", "Library");

                            if (!Directory.Exists(libFolder))
                            {
                                Directory.CreateDirectory(libFolder);
                            }

                            return Path.Combine(libFolder, DatabaseFilename);
                        }
                    case Device.Android:
                        {
                            var path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                            return Path.Combine(path, DatabaseFilename);
                        }
                    case Device.UWP:
                        {
                            var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                            return Path.Combine(basePath, DatabaseFilename);
                        }
                    default:
                        return "/";
                }
            }
        }

        public const string DowloadDbUrl = "https://onedrive.gimhoy.com/1drv/aHR0cHM6Ly8xZHJ2Lm1zL3UvcyFBcEluUjVpMXk0bWdoamY3M01EYnJSNmZ3eHV2P2U9ZmhvQ2lR.db3";

        public static string Convert(string chr)
        {
            try
            {
                if (chr.Length != 0)
                {
                    var fullSpell = new StringBuilder();
                    foreach (var t in chr)
                    {
                        var isChineses = ChineseChar.IsValidChar(t);
                        if (isChineses)
                        {
                            var chineseChar = new ChineseChar(t);
                            foreach (var value in chineseChar.Pinyins)
                            {
                                if (string.IsNullOrEmpty(value)) continue;

                                var str = value.Remove(value.Length - 1, 1);
                                var startStr = str[0];
                                fullSpell.Append(startStr);
                                break;
                            }
                        }
                        else
                        {
                            fullSpell.Append(t);
                        }
                    }

                    return fullSpell.ToString().ToUpper();
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return string.Empty;
        }
    }
}