using System;
using System.Threading.Tasks;

namespace YcyTv.Extensions
{
    public static class TaskExtensions
    {
        /// <summary>
        /// 注意：此处有意使用异步void。 这提供了一种方法
        /// 从构造函数中调用异步方法
        /// </summary>
        /// <param name="task"></param>
        /// <param name="returnToCallingContext">返回到调用上下文</param>
        /// <param name="onException">异常</param>
        public static async void SafeFireAndForget(this Task task, bool returnToCallingContext,
            Action<Exception> onException = null)
        {
            try
            {
                await task.ConfigureAwait(returnToCallingContext);
            }
            catch (Exception ex) when (onException != null)
            {
                onException(ex);
            }
        }

        public static T SafeFireAndForget<T>(this Task<T> task, bool returnToCallingContext = true,
            Action<Exception> onException = null)
        {
            try
            {
                return task.ConfigureAwait(returnToCallingContext).GetAwaiter().GetResult();
            }
            catch (Exception ex) when (onException != null)
            {
                onException(ex);
            }

            return default;
        }
    }
}