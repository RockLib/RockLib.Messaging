using System;
using Rock.Defaults;
using Rock.Messaging.Routing;

namespace Rock.Messaging.Defaults.Implementation
{
    public static partial class Default
    {
        private static readonly DefaultHelper<ITypeLocator> _typeLocator = new DefaultHelper<ITypeLocator>(() => new AppDomainTypeLocator(DefaultMessageParser, AppDomain.CurrentDomain));

        public static ITypeLocator TypeLocator
        {
            get { return _typeLocator.Current; }
        }

        public static ITypeLocator DefaultTypeLocator
        {
            get { return _typeLocator.DefaultInstance; }
        }

        public static void SetTypeLocator(Func<ITypeLocator> getTypeLocatorInstance)
        {
            _typeLocator.SetCurrent(getTypeLocatorInstance);
        }

        public static void RestoreDefaultTypeLocator()
        {
            _typeLocator.RestoreDefault();
        }
    }
}
