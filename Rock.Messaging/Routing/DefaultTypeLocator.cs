using System;
using Rock.Immutable;

namespace Rock.Messaging.Routing
{
    public static class DefaultTypeLocator
    {
        private static readonly Semimutable<ITypeLocator> _typeLocator = new Semimutable<ITypeLocator>(GetDefault);

        public static ITypeLocator Current
        {
            get { return _typeLocator.Value; }
        }

        public static void SetCurrent(ITypeLocator typeLocator)
        {
            _typeLocator.Value = typeLocator;
        }

        private static ITypeLocator GetDefault()
        {
            return new AppDomainTypeLocator(
                DefaultMessageParser.Current as XmlMessageParser ?? new XmlMessageParser(),
                AppDomain.CurrentDomain);
        }
    }
}