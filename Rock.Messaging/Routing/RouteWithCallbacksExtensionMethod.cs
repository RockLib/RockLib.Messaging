using System;
using System.Threading.Tasks;

namespace Rock.Messaging.Routing
{
    public static class RouteWithCallbacksExtensionMethod
    {
        public static async Task Route(
            this IMessageRouter messageRouter,
            string rawMessage,
            Action<IMessage> onSuccess = null,
            Action<Exception> onFailue = null,
            Action onComplete = null)
        {
            try
            {
                var message = await messageRouter.Route(rawMessage);

                if (onSuccess != null)
                {
                    onSuccess(message);
                }
            }
            catch (Exception ex)
            {
                if (onFailue != null)
                {
                    onFailue(ex);
                }
            }

            if (onComplete != null)
            {
                onComplete();
            }
        }
    }
}
