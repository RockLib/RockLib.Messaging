#if !NET451
using Microsoft.Extensions.Options;
using System;

namespace RockLib.Messaging.DependencyInjection
{
    internal class ReloadingReceiver<TReceiverOptions> : IReceiver
    {
        public ReloadingReceiver(IServiceProvider serviceProvider, string name,
            Func<TReceiverOptions, IServiceProvider, IReceiver> createReceiver, TReceiverOptions initialOptions,
            IOptionsMonitor<TReceiverOptions> optionsMonitor, Action<TReceiverOptions> configureOptions)
        {
            ServiceProvider = serviceProvider;
            Name = name;
            CreateReceiver = createReceiver;
            ConfigureOptions = configureOptions;

            Receiver = CreateReceiver.Invoke(initialOptions, ServiceProvider);
            ChangeListener = optionsMonitor.OnChange(OptionsMonitorChanged);
        }

        public IServiceProvider ServiceProvider { get; }

        public string Name { get; }

        public IMessageHandler MessageHandler
        {
            get => Receiver.MessageHandler;
            set => Receiver.MessageHandler = value;
        }

        public Func<TReceiverOptions, IServiceProvider, IReceiver> CreateReceiver { get; }

        public Action<TReceiverOptions> ConfigureOptions { get; }

        public IReceiver Receiver { get; private set; }

        public IDisposable ChangeListener { get; }

        public EventHandler ConnectedHandler { get; private set; }

        public EventHandler<DisconnectedEventArgs> DisconnectedHandler { get; private set; }

        public EventHandler<ErrorEventArgs> ErrorHandler { get; private set; }

        public event EventHandler Connected
        {
            add
            {
                Receiver.Connected += value;
                ConnectedHandler += value;
            }
            remove
            {
                Receiver.Connected -= value;
                ConnectedHandler -= value;
            }
        }

        public event EventHandler<DisconnectedEventArgs> Disconnected
        {
            add
            {
                Receiver.Disconnected += value;
                DisconnectedHandler += value;
            }
            remove
            {
                Receiver.Disconnected -= value;
                DisconnectedHandler -= value;
            }
        }

        public event EventHandler<ErrorEventArgs> Error
        {
            add
            {
                Receiver.Error += value;
                ErrorHandler += value;
            }
            remove
            {
                Receiver.Error -= value;
                ErrorHandler -= value;
            }
        }

        public void Dispose()
        {
            ChangeListener.Dispose();
            Receiver.Dispose();
        }

        private void OptionsMonitorChanged(TReceiverOptions options, string name)
        {
            if (string.Equals(Name, name, StringComparison.OrdinalIgnoreCase))
            {
                ConfigureOptions?.Invoke(options);

                var oldReceiver = Receiver;
                var newReceiver = CreateReceiver.Invoke(options, ServiceProvider);

                newReceiver.Connected += ConnectedHandler;
                newReceiver.Disconnected += DisconnectedHandler;
                newReceiver.Error += ErrorHandler;
                if (oldReceiver.MessageHandler != null && newReceiver.MessageHandler == null)
                    newReceiver.MessageHandler = oldReceiver.MessageHandler;

                Receiver = newReceiver;
                oldReceiver.Dispose();
            }
        }
    }
}
#endif
