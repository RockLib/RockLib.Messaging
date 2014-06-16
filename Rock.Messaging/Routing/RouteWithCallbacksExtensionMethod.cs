using System;
using System.Threading.Tasks;

namespace Rock.Messaging.Routing
{
    /// <summary>
    /// 
    /// </summary>
    public static class RouteWithCallbacksExtensionMethod
    {
        public static async Task Route(
            this IMessageRouter messageRouter,
            string rawMessage,
            Action<IMessage> onSuccess = null,
            Action<Exception> onFailue = null,
            Action onComplete = null)
        {
            RouteResult result;

            try
            {
                result = await messageRouter.Route(rawMessage);
            }
            catch (Exception ex)
            {
                result = new RouteResult(ex);
            }

            if (onSuccess != null && result.Message != null)
            {
                onSuccess(result.Message);
            }
            else if (onFailue != null && result.Exception != null)
            {
                onFailue(result.Exception);
            }

            if (onComplete != null)
            {
                onComplete();
            }
        }
    }
}
