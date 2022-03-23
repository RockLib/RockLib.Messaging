using Newtonsoft.Json;
using RockLib.Messaging.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RockLib.Messaging.CloudEvents
{
    /// <summary>
    /// Extension methods related to CloudEvents.
    /// </summary>
    public static partial class CloudEventExtensions
    {
        private static readonly ConcurrentDictionary<Type, CopyConstructor> _copyConstructors = new ConcurrentDictionary<Type, CopyConstructor>();
        private static readonly ConcurrentDictionary<Type, MessageConstructor> _messageConstructors = new ConcurrentDictionary<Type, MessageConstructor>();
        private static readonly ConcurrentDictionary<Type, ValidateMethod> _validateMethods = new ConcurrentDictionary<Type, ValidateMethod>();

        private static readonly ConditionalWeakTable<CloudEvent, object> _dataObjects = new ConditionalWeakTable<CloudEvent, object>();

        /// <summary>
        /// Sets the data (payload) of the <see cref="CloudEvent"/> as type <see cref="string"/>.
        /// <para>
        /// After calling this method, the <see cref="CloudEvent.StringData"/> property returns
        /// <paramref name="data"/>, and the <see cref="CloudEvent.BinaryData"/> property returns
        /// <see langword="null"/>.
        /// </para>
        /// </summary>
        /// <typeparam name="TCloudEvent">The type of <see cref="CloudEvent"/>.</typeparam>
        /// <param name="cloudEvent">The <see cref="CloudEvent"/> to set the data to.</param>
        /// <param name="data">
        /// The data of the <see cref="CloudEvent"/> as a <see cref="string"/>.
        /// </param>
        /// <returns>The same <typeparamref name="TCloudEvent"/> for method chaining.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="cloudEvent"/> is <see langword="null"/>.
        /// </exception>
        public static TCloudEvent SetData<TCloudEvent>(this TCloudEvent cloudEvent, string data)
            where TCloudEvent : CloudEvent
        {
            if (cloudEvent is null)
                throw new ArgumentNullException(nameof(cloudEvent));

            if (data != cloudEvent.StringData)
            {
                cloudEvent.SetDataField(data);
                cloudEvent.ClearDataObject();
            }

            return cloudEvent;
        }

        /// <summary>
        /// Sets the data (payload) of the <see cref="CloudEvent"/> as a <see cref="byte"/> array.
        /// <para>
        /// After calling this method, the <see cref="CloudEvent.BinaryData"/> property returns
        /// <paramref name="data"/>, and the <see cref="CloudEvent.StringData"/> property returns
        /// <see langword="null"/>.
        /// </para>
        /// </summary>
        /// <typeparam name="TCloudEvent">The type of <see cref="CloudEvent"/>.</typeparam>
        /// <param name="cloudEvent">The <see cref="CloudEvent"/> to set the data to.</param>
        /// <param name="data">
        /// The data of the <see cref="CloudEvent"/> as a <see cref="byte"/> array.
        /// </param>
        /// <returns>The same <typeparamref name="TCloudEvent"/> for method chaining.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="cloudEvent"/> is <see langword="null"/>.
        /// </exception>
        public static TCloudEvent SetData<TCloudEvent>(this TCloudEvent cloudEvent, byte[] data)
            where TCloudEvent : CloudEvent
        {
            if (cloudEvent is null)
                throw new ArgumentNullException(nameof(cloudEvent));

            var binaryData = cloudEvent.BinaryData;

            if (!ReferenceEquals(binaryData, data)
                && (binaryData == null || data == null || !binaryData.SequenceEqual(data)))
            {
                cloudEvent.SetDataField(data);
                cloudEvent.ClearDataObject();
            }

            return cloudEvent;
        }

        /// <summary>
        /// Sets the data (payload) of the <see cref="CloudEvent"/> as type <typeparamref name=
        /// "T"/>.
        /// <para>
        /// After calling this method, the <see cref="CloudEvent.StringData"/> property returns
        /// the JSON or XML serialized <paramref name="data"/>, and the <see cref=
        /// "CloudEvent.BinaryData"/> property returns <see langword="null"/>.
        /// </para>
        /// <para>
        /// The same instance of <typeparamref name="T"/> can be retrieved by calling the <see cref
        /// ="GetData"/> and <see cref="TryGetData"/> methods with the same instance of <see cref=
        /// "CloudEvent"/> and the same type parameter <typeparamref name="T"/>.
        /// </para>
        /// </summary>
        /// <typeparam name="TCloudEvent">The type of <see cref="CloudEvent"/>.</typeparam>
        /// <typeparam name="T">The type of the <see cref="CloudEvent"/> data.</typeparam>
        /// <param name="cloudEvent">The <see cref="CloudEvent"/> to set the data to.</param>
        /// <param name="data">
        /// The data of the <see cref="CloudEvent"/> as type <typeparamref name="T"/>.
        /// </param>
        /// <param name="serialization"></param>
        /// <returns>The same <typeparamref name="TCloudEvent"/> for method chaining.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="cloudEvent"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="serialization"/> is not defined.
        /// </exception>
        public static TCloudEvent SetData<TCloudEvent, T>(this TCloudEvent cloudEvent, T data,
            DataSerialization serialization = DataSerialization.Json)
            where TCloudEvent : CloudEvent
            where T : class
        {
            if (cloudEvent is null)
                throw new ArgumentNullException(nameof(cloudEvent));
            if (!Enum.IsDefined(typeof(DataSerialization), serialization))
                throw new ArgumentOutOfRangeException(nameof(serialization));

            if (data is null)
            {
                cloudEvent.ClearDataField();
                cloudEvent.ClearDataObject();
            }
            else
            {
                switch (serialization)
                {
                    case DataSerialization.Json:
                        cloudEvent.SetDataField(JsonSerialize(data));
                        break;
                    default:
                        cloudEvent.SetDataField(XmlSerialize(data));
                        break;
                }
                cloudEvent.SetDataObject(data);
            }

            return cloudEvent;
        }

        /// <summary>
        /// Gets the data (payload) of the <see cref="CloudEvent"/> as type <typeparamref name=
        /// "T"/>.
        /// <para>
        /// This method, along with the <see cref="TryGetData"/> method, is <em>idempotent</em>. In
        /// other words, every call to either of these methods with <em>same instance</em> of <see
        /// cref="CloudEvent"/> and the <em>same type</em> <typeparamref name="T"/> will return the
        /// <em>same instance</em> of type <typeparamref name="T"/>.
        /// </para>
        /// <para>
        /// If the data object of this cloud event was set using the <see cref=
        /// "SetData{TCloudEvent, T}(TCloudEvent, T, DataSerialization)"/> method, then the same
        /// instance of <typeparamref name="T"/> that was passed to that method will be returned by
        /// this method. Otherwise, the value of <see cref="CloudEvent.StringData"/> is used to
        /// deserialize the instance of <typeparamref name="T"/>.
        /// </para>
        /// </summary>
        /// <typeparam name="T">The type of the <see cref="CloudEvent"/> data.</typeparam>
        /// <param name="cloudEvent">The <see cref="CloudEvent"/> to get data from.</param>
        /// <param name="serialization">
        /// If <paramref name="cloudEvent"/> does not already has a data object associated with it,
        /// the kind of serialization that will be used to convert its <see cref=
        /// "CloudEvent.StringData"/> to type <typeparamref name="T"/>.
        /// </param>
        /// <returns>
        /// The data of the <see cref="CloudEvent"/> as type <typeparamref name="T"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="cloudEvent"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="serialization"/> is not defined.
        /// </exception>
        /// <exception cref="InvalidCastException">
        /// If <paramref name="cloudEvent"/> already has a data object associated with it, but that
        /// data object cannot be converted to type <typeparamref name="T"/>.
        /// </exception>
        /// <exception cref="JsonException">
        /// If an error occurs during JSON deserialization.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// If an error occurs during XML deserialization.
        /// </exception>
        public static T GetData<T>(this CloudEvent cloudEvent,
            DataSerialization serialization = DataSerialization.Json)
            where T : class
        {
            if (cloudEvent is null)
                throw new ArgumentNullException(nameof(cloudEvent));
            if (!Enum.IsDefined(typeof(DataSerialization), serialization))
                throw new ArgumentOutOfRangeException(nameof(serialization));

            var dataObject = cloudEvent.GetDataObject<T>(serialization);

            if (dataObject is T data)
                return data;

            throw new InvalidCastException(
                $"Unable to cast the CloudEvent's data of type '{dataObject?.GetType().FullName}' to type '{typeof(T).FullName}'.");
        }

        /// <summary>
        /// Gets the data (payload) of the <see cref="CloudEvent"/> as type <typeparamref name=
        /// "T"/>.
        /// <para>
        /// This method, along with the <see cref="GetData"/> method, is <em>idempotent</em>. In
        /// other words, every call to either of these methods with <em>same instance</em> of <see
        /// cref="CloudEvent"/> and the <em>same type</em> <typeparamref name="T"/> will return the
        /// <em>same instance</em> of type <typeparamref name="T"/>.
        /// </para>
        /// <para>
        /// If the data object of this cloud event was set using the <see cref=
        /// "SetData{TCloudEvent, T}(TCloudEvent, T, DataSerialization)"/> method, then the same
        /// instance of <typeparamref name="T"/> that was passed to that method will be returned by
        /// this method. Otherwise, the value of <see cref="CloudEvent.StringData"/> is used to
        /// deserialize the instance of <typeparamref name="T"/>.
        /// </para>
        /// </summary>
        /// <typeparam name="T">The type of the <see cref="CloudEvent"/> data.</typeparam>
        /// <param name="cloudEvent">The <see cref="CloudEvent"/> to get data from.</param>
        /// <param name="data">
        /// When this method returns, the data of the <paramref name="cloudEvent"/> if it exists
        /// as (or can be serialized to) type <typeparamref name="T"/>; otherwise, <see langword=
        /// "null"/>. This parameter is passed uninitialized.
        /// </param>
        /// <param name="serialization">
        /// If <paramref name="cloudEvent"/> does not already has a data object associated with it,
        /// the kind of serialization that will be used to convert its <see cref=
        /// "CloudEvent.StringData"/> to type <typeparamref name="T"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the data of the <paramref name="cloudEvent"/> exists as (or
        /// can be serialized to) type <typeparamref name="T"/>; otherwise <see langword="false"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="cloudEvent"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="serialization"/> is not defined.
        /// </exception>
        public static bool TryGetData<T>(this CloudEvent cloudEvent, out T data,
            DataSerialization serialization = DataSerialization.Json)
            where T : class
        {
            if (cloudEvent is null)
                throw new ArgumentNullException(nameof(cloudEvent));
            if (!Enum.IsDefined(typeof(DataSerialization), serialization))
                throw new ArgumentOutOfRangeException(nameof(serialization));

            try
            {
                var dataObject = cloudEvent.GetDataObject<T>(serialization);

                if (dataObject is T)
                {
                    data = (T)dataObject;
                    return true;
                }
            }
            catch { }

            data = default;
            return false;
        }

        /// <summary>
        /// Creates a new instance of the <typeparamref name="TCloudEvent"/> type and copies all
        /// cloud event attributes except for <see cref="CloudEvent.Id"/> and <see cref=
        /// "CloudEvent.Time"/> to the new instance. Note that the source's data is <em>not</em>
        /// copied to the new instance.
        /// <para>
        /// The <typeparamref name="TCloudEvent"/> type <em>must</em> define a public copy
        /// constructor - one with a single parameter of type <typeparamref name="TCloudEvent"/>.
        /// A <see cref="MissingMemberException"/> is immediately thrown if the class does not
        /// define such a constructor.
        /// </para>
        /// </summary>
        /// <typeparam name="TCloudEvent">
        /// The type of <see cref="CloudEvent"/> to create a copy of.
        /// </typeparam>
        /// <param name="cloudEvent">The <see cref="CloudEvent"/> to create a copy of.</param>
        /// <returns>A copy of the <paramref name="cloudEvent"/> parameter.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="cloudEvent"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="MissingMemberException">
        /// If the <typeparamref name="TCloudEvent"/> class does not define a public copy
        /// constructor - one with a single parameter of type <typeparamref name="TCloudEvent"/>.
        /// with the exact parameters <c>(<see cref="IReceiverMessage"/>, <see cref=
        /// "IProtocolBinding"/>)</c>.
        /// </exception>
        public static TCloudEvent Copy<TCloudEvent>(this TCloudEvent cloudEvent)
            where TCloudEvent : CloudEvent
        {
            if (cloudEvent is null)
                throw new ArgumentNullException(nameof(cloudEvent));

            var copyConstructor = _copyConstructors.GetOrAdd(typeof(TCloudEvent), CopyConstructor.Create)
                ?? throw MissingCopyConstructor(typeof(TCloudEvent));

            return (TCloudEvent)copyConstructor.Invoke(cloudEvent);
        }

        /// <summary>
        /// Creates an instance of <typeparamref name="TCloudEvent"/> with properties mapped from
        /// the headers of <paramref name="receiverMessage"/>.
        /// <para>
        /// The <typeparamref name="TCloudEvent"/> type <em>must</em> define a public constructor
        /// with the exact parameters <c>(<see cref="IReceiverMessage"/>, <see cref=
        /// "IProtocolBinding"/>)</c>. A <see cref="MissingMemberException"/> is immediately
        /// thrown if the class does not define such a constructor.
        /// </para>
        /// </summary>
        /// <typeparam name="TCloudEvent">The type of <see cref="CloudEvent"/> to create.</typeparam>
        /// <param name="receiverMessage">
        /// The <see cref="IReceiverMessage"/> to be mapped to the new <typeparamref name="TCloudEvent"/>.
        /// </param>
        /// <param name="protocolBinding">
        /// The <see cref="IProtocolBinding"/> used to map <see cref="IReceiverMessage"/> headers to
        /// CloudEvent attributes.
        /// </param>
        /// <returns>
        /// A new <typeparamref name="TCloudEvent"/> with properties mapped from the headers of the <see cref="IReceiverMessage"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="receiverMessage"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="MissingMemberException">
        /// If the <typeparamref name="TCloudEvent"/> class does not define a public constructor
        /// with the exact parameters <c>(<see cref="IReceiverMessage"/>, <see cref=
        /// "IProtocolBinding"/>)</c>.
        /// </exception>
        public static TCloudEvent To<TCloudEvent>(this IReceiverMessage receiverMessage, IProtocolBinding? protocolBinding = null)
            where TCloudEvent : CloudEvent
        {
            if (receiverMessage is null)
                throw new ArgumentNullException(nameof(receiverMessage));

            var messageConstructor = _messageConstructors.GetOrAdd(typeof(TCloudEvent), MessageConstructor.Create)
                ?? throw MissingReceiverConstructor(typeof(TCloudEvent));

            return (TCloudEvent)messageConstructor.Invoke(receiverMessage, protocolBinding);
        }

        /// <summary>
        /// Start listening for CloudEvents and handle them using the specified callback function.
        /// <para>
        /// The <typeparamref name="TCloudEvent"/> type <em>must</em> define a constructor with the
        /// exact parameters <c>(<see cref="IReceiverMessage"/>, <see cref="IProtocolBinding"/>)
        /// </c>. A <see cref="MissingMemberException"/> is immediately thrown if the class does
        /// not define such a constructor.
        /// </para>
        /// </summary>
        /// <param name="receiver">The receiver to start.</param>
        /// <param name="onEventReceivedAsync">
        /// A function that is invoked when a CloudEvent is received.
        /// </param>
        /// <param name="protocolBinding">
        /// The <see cref="IProtocolBinding"/> used to map <see cref="IReceiverMessage"/> headers to
        /// CloudEvent attributes.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="receiver"/> or <paramref name="onEventReceivedAsync"/> is <see
        /// langword="null"/>.
        /// </exception>
        /// <exception cref="MissingMemberException">
        /// If the <typeparamref name="TCloudEvent"/> class does not define a public constructor
        /// with the exact parameters <c>(<see cref="IReceiverMessage"/>, <see cref=
        /// "IProtocolBinding"/>)</c>.
        /// </exception>
        public static void Start<TCloudEvent>(this IReceiver receiver,
            Func<TCloudEvent, IReceiverMessage, Task> onEventReceivedAsync, IProtocolBinding? protocolBinding = null)
            where TCloudEvent : CloudEvent
        {
            if (receiver is null)
                throw new ArgumentNullException(nameof(receiver));
            if (onEventReceivedAsync is null)
                throw new ArgumentNullException(nameof(onEventReceivedAsync));

            if (!MessageConstructor.Exists(typeof(TCloudEvent)))
                throw MissingReceiverConstructor(typeof(TCloudEvent));

            receiver.Start(message => onEventReceivedAsync(message.To<TCloudEvent>(protocolBinding), message));
        }

        /// <summary>
        /// Adds a <see cref="ValidatingSender"/> decorator that ensures messages are valid
        /// CloudEvents.
        /// <para>
        /// The <typeparamref name="TCloudEvent"/> type <em>must</em> define a public static method
        /// named "Validate" with the exact parameters <c>(<see cref="SenderMessage"/>, <see cref=
        /// "IProtocolBinding"/>)</c>. A <see cref="MissingMemberException"/> is immediately thrown if
        /// the class does not define such a method.
        ///  </para>
        /// </summary>
        /// <typeparam name="TCloudEvent">The type of CloudEvent used to apply validation.</typeparam>
        /// <param name="builder">The <see cref="ISenderBuilder"/>.</param>
        /// <param name="protocolBinding">
        /// The <see cref="IProtocolBinding"/> used to map CloudEvent attributes to <see cref="SenderMessage"/>
        /// headers.
        /// </param>
        /// <returns>The same <see cref="ISenderBuilder"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="builder"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="MissingMemberException">
        /// If the <typeparamref name="TCloudEvent"/> class does not define a public static method
        /// named "Validate" with the exact parameters <c>(<see cref="SenderMessage"/>, <see cref=
        /// "IProtocolBinding"/>)</c>.
        /// </exception>
        public static ISenderBuilder AddValidation<TCloudEvent>(this ISenderBuilder builder, IProtocolBinding? protocolBinding = null)
            where TCloudEvent : CloudEvent
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));

            var validateMethod = _validateMethods.GetOrAdd(typeof(TCloudEvent), ValidateMethod.Create)
                ?? throw MissingValidateMethod(typeof(TCloudEvent));

            return builder.AddValidation(message => validateMethod.Invoke(message, protocolBinding));
        }

        private static MissingMemberException MissingReceiverConstructor(Type cloudEventType) =>
            new MissingMemberException($"CloudEvent type '{cloudEventType.Name}' must have a public constructor"
                + $" with the exact parameters ({nameof(IReceiverMessage)}, {nameof(IProtocolBinding)}).");

        private static MissingMemberException MissingCopyConstructor(Type cloudEventType) =>
            new MissingMemberException($"CloudEvent type '{cloudEventType.Name}' must have a public copy constructor" +
                $" (a constructor with a single parameter of type '{cloudEventType.Name}').");

        private static MissingMemberException MissingValidateMethod(Type cloudEventType) =>
            new MissingMemberException($"CloudEvent type '{cloudEventType.Name}' must have a public static method" +
                $" named '{nameof(CloudEvent.Validate)}' with the exact parameters ({nameof(SenderMessage)}, {nameof(IProtocolBinding)}).");

        private static void SetDataObject<T>(this CloudEvent cloudEvent, T data)
            where T : class
        {
            cloudEvent.ClearDataObject();
            _dataObjects.Add(cloudEvent, data);
        }

        private static void ClearDataObject(this CloudEvent cloudEvent)
        {
            if (_dataObjects.TryGetValue(cloudEvent, out _))
                _dataObjects.Remove(cloudEvent);
        }

        private static object? GetDataObject<T>(this CloudEvent cloudEvent, DataSerialization serialization)
            where T : class
        {
            return _dataObjects.GetValue(cloudEvent, evt =>
            {
                var stringData = evt.StringData;

                if (string.IsNullOrEmpty(stringData))
                {
                    if (evt.BinaryData != null)
                        stringData = Encoding.UTF8.GetString(evt.BinaryData);

                    if (string.IsNullOrEmpty(stringData))
                        return null!;
                }

                return serialization == DataSerialization.Json
                    ? JsonDeserialize<T>(stringData)!
                    : XmlDeserialize<T>(stringData)!;
            });
        }

        internal static bool TryGetDataObject(this CloudEvent cloudEvent, out object data) =>
            _dataObjects.TryGetValue(cloudEvent, out data);

        private static string JsonSerialize(object data) =>
            JsonConvert.SerializeObject(data);

        private static T? JsonDeserialize<T>(string data) =>
            JsonConvert.DeserializeObject<T>(data);

        private static string? XmlSerialize(object data)
        {
            if (data == null)
                return null;

            var sb = new StringBuilder();
            var serializer = new XmlSerializer(data.GetType());
            using (var writer = new StringWriter(sb))
                serializer.Serialize(writer, data);
            return sb.ToString();
        }

        private static T? XmlDeserialize<T>(string data)
            where T : class
        {
            if (data == null)
                return null;

            var serializer = new XmlSerializer(typeof(T));
            using (var reader = new StringReader(data))
                return (T)serializer.Deserialize(reader)!;
        }
    }
}
