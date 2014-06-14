using System;
using Rock.Defaults;

namespace Rock.Messaging.Defaults.Implementation
{
    public static partial class Default
    {
        private static readonly DefaultHelper<IExceptionHandler> _exceptionHandler = new DefaultHelper<IExceptionHandler>(() => NullExceptionHandler.Instance);

        public static IExceptionHandler ExceptionHandler
        {
            get { return _exceptionHandler.Current; }
        }

        public static IExceptionHandler DefaultExceptionHandler
        {
            get { return _exceptionHandler.DefaultInstance; }
        }

        public static void SetExceptionHandler(Func<IExceptionHandler> getExceptionHandlerInstance)
        {
            _exceptionHandler.SetCurrent(getExceptionHandlerInstance);
        }

        public static void RestoreDefaultExceptionHandler()
        {
            _exceptionHandler.RestoreDefault();
        }
    }
}
