using System;
using System.Linq;
using System.Threading.Tasks;
using SQLite;
using YcyTv.Extensions;

namespace YcyTv.DataBase
{
    public class DatabaseHelper
    {
        private static readonly Lazy<SQLiteAsyncConnection> LazyInitializer =
            new Lazy<SQLiteAsyncConnection>(() => new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags));

        public static SQLiteAsyncConnection Database => LazyInitializer.Value;

        private static bool _initialized = false;

        public DatabaseHelper()
        {
            OnInitializeAsync().SafeFireAndForget(false);
        }

        private static async Task OnInitializeAsync()
        {
            if (!_initialized)
            {
                try
                {
                    if (Database.TableMappings.All(m => m.MappedType.Name != nameof(VodPlayDb)))
                    {
                        await Database.CreateTablesAsync(CreateFlags.None, typeof(VodPlayDb)).ConfigureAwait(false);
                    }

                    if (Database.TableMappings.All(m => m.MappedType.Name != nameof(VodPlayUrlDb)))
                    {
                        await Database.CreateTablesAsync(CreateFlags.None, typeof(VodPlayUrlDb)).ConfigureAwait(false);
                    }

                    if (Database.TableMappings.All(m => m.MappedType.Name != nameof(VodPlayTypeDb)))
                    {
                        await Database.CreateTablesAsync(CreateFlags.None, typeof(VodPlayTypeDb)).ConfigureAwait(false);
                    }

                    if (Database.TableMappings.All(m => m.MappedType.Name != nameof(VodPlayAreaDb)))
                    {
                        await Database.CreateTablesAsync(CreateFlags.None, typeof(VodPlayAreaDb)).ConfigureAwait(false);
                    }

                    if (Database.TableMappings.All(m => m.MappedType.Name != nameof(VodPlayYearDb)))
                    {
                        await Database.CreateTablesAsync(CreateFlags.None, typeof(VodPlayYearDb)).ConfigureAwait(false);
                    }

                    if (Database.TableMappings.All(m => m.MappedType.Name != nameof(VodPlayHistoryDb)))
                    {
                        await Database.CreateTablesAsync(CreateFlags.None, typeof(VodPlayHistoryDb)).ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("初始化数据库失败." + ex.ToString());
                }

                _initialized = true;
            }
        }
    }
}