using System;
using System.Threading;
using System.Threading.Tasks;

#if ROCKLIB
namespace RockLib.Messaging.SQS
#else
namespace Rock.Messaging.SQS
#endif
{
    internal static class Sync
    {
        public static TResult OverAsync<TResult>(Func<Task<TResult>> getTaskOfTResult)
        {
            SynchronizationContext old = SynchronizationContext.Current;
            try
            {
                SynchronizationContext.SetSynchronizationContext(null);
                return getTaskOfTResult().GetAwaiter().GetResult();
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(old);
            }
        }
    }
}
