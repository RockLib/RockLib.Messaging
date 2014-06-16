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
            Action<Exception> onFailure = null,
            Action onComplete = null)
        {
            if (messageRouter == null)
            {
                throw new ArgumentNullException("messageRouter");
            }

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
                try
                {
                    onSuccess(result.Message);
                }
                catch
                {
                }
            }
            else if (onFailure != null && result.Exception != null)
            {
                try
                {
                    onFailure(result.Exception);
                }
                catch
                {
                }
            }

            if (onComplete != null)
            {
                try
                {
                    onComplete();
                }
                catch
                {
                }
            }
        }
    }
}
