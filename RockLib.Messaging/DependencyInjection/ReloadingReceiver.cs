#if !NET451
using Microsoft.Extensions.Options;
using System;

namespace RockLib.Messaging.DependencyInjection
{
    internal class ReloadingReceiver<TReceiverOptions> : IReceiver
        where TReceiverOptions : class, new()
    {
        public ReloadingReceiver(string name, Func<TReceiverOptions, IReceiver> createReceiver,
            IOptionsMonitor<TReceiverOptions> optionsMonitor, Action<TReceiverOptions> configureOptions)
        {
            Name = name;
            CreateReceiver = createReceiver;
            ConfigureOptions = configureOptions;

            var options = optionsMonitor.GetOptions(Name, configureOptions);
            Receiver = CreateReceiver.Invoke(options);
            ChangeListener = optionsMonitor.OnChange(OptionsMonitorChanged);
        }

        public string Name { get; }

        public IMessageHandler MessageHandler
        {
            get => Receiver.MessageHandler;
            set => Receiver.MessageHandler = value;
        }

        public Func<TReceiverOptions, IReceiver> CreateReceiver { get; }

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
                var newReceiver = CreateReceiver.Invoke(options);

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
