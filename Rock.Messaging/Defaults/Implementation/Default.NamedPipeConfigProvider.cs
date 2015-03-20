using System;
using Rock.Defaults;
using Rock.Messaging.NamedPipes;

namespace Rock.Messaging.Defaults.Implementation
{
    public static partial class Default
    {
        private static readonly DefaultHelper<INamedPipeConfigProvider> _namedPipeConfigProvider = new DefaultHelper<INamedPipeConfigProvider>(() => new SimpleNamedPipeConfigProvider());

        public static INamedPipeConfigProvider NamedPipeConfigProvider
        {
            get { return _namedPipeConfigProvider.Current; }
        }

        public static INamedPipeConfigProvider DefaultNamedPipeConfigProvider
        {
            get { return _namedPipeConfigProvider.DefaultInstance; }
        }

        public static void SetNamedPipeConfigProvider(Func<INamedPipeConfigProvider> getNamedPipeConfigProviderInstance)
        {
            _namedPipeConfigProvider.SetCurrent(getNamedPipeConfigProviderInstance);
        }

        public static void RestoreDefaultNamedPipeConfigProvider()
        {
            _namedPipeConfigProvider.RestoreDefault();
        }
    }
}
