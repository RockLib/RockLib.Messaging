using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using static RockLib.Messaging.HttpUtils;

namespace RockLib.Messaging.CloudEvents
{
    /// <summary>
    /// Defines a cloud event.
    /// </summary>
    public class CloudEvent
    {
        /// <summary>The name of the <see cref="Id"/> attribute.</summary>
        public const string IdAttribute = "id";

        /// <summary>The name of the <see cref="Source"/> attribute.</summary>
        public const string SourceAttribute = "source";

        /// <summary>The name of the <see cref="SpecVersion"/> attribute.</summary>
        public const string SpecVersionAttribute = "specversion";

        /// <summary>The name of the <see cref="Type"/> attribute.</summary>
        public const string TypeAttribute = "type";

        /// <summary>The name of the <see cref="DataContentType"/> attribute.</summary>
        public const string DataContentTypeAttribute = "datacontenttype";

        /// <summary>The name of the <see cref="DataSchema"/> attribute.</summary>
        public const string DataSchemaAttribute = "dataschema";

        /// <summary>The name of the <see cref="Subject"/> attribute.</summary>
        public const string SubjectAttribute = "subject";

        /// <summary>The name of the <see cref="Time"/> attribute.</summary>
        public const string TimeAttribute = "time";

        /// <summary>The name of the content type header for <c>structured mode</c>.</summary>
        public const string StructuredModeContentTypeHeader = "content-type";

        /// <summary>
        /// The prefix of the media type of a 'content-type' header that indicates that its
        /// <see cref="IReceiverMessage"/> is rendered in <c>structured mode</c>.
        /// </summary>
        public const string StructuredModeMediaTypePrefix = "application/cloudevents";

        /// <summary>
        /// The suffix of a media type indicating that the content is rendered as JSON.
        /// </summary>
        public const string JsonMediaTypeSuffix = "+json";

        /// <summary>
        /// The media type of a 'content-type' header that indicates that an <see cref=
        /// "IReceiverMessage"/> is rendered as JSON in <c>structured mode</c>.
        /// </summary>
        public const string StructuredModeJsonMediaType = StructuredModeMediaTypePrefix + JsonMediaTypeSuffix;

        private const string _specVersion1_0 = "1.0";

        private static IProtocolBinding _defaultProtocolBinding = ProtocolBindings.Default;
        private IProtocolBinding? _protocolBinding;

        private object? _data;
        private MediaTypeHeaderValue? _contentType;

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudEvent"/> class.
        /// </summary>
        public CloudEvent() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudEvent"/> class based on the source
        /// cloud event. All cloud event attributes except <see cref="Id"/> and <see cref="Time"/>
        /// are copied to the new instance. Note that the source event's data is <em>not</em>
        /// copied to the new instance.
        /// </summary>
        /// <param name="source">
        /// The source for cloud event attribute values.
        /// </param>
        public CloudEvent(CloudEvent source)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));

            ProtocolBinding = source.ProtocolBinding;

            foreach (var additionalAttribute in source.Attributes)
                if (additionalAttribute.Key != IdAttribute && additionalAttribute.Key != TimeAttribute)
                    Attributes.Add(additionalAttribute);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudEvent"/> class and sets its data,
        /// attributes, and headers according to the payload and headers of the <paramref name=
        /// "receiverMessage"/>.
        /// </summary>
        /// <param name="receiverMessage">
        /// The <see cref="IReceiverMessage"/> with headers that map to cloud event attributes.
        /// </param>
        /// <param name="protocolBinding">
        /// The <see cref="IProtocolBinding"/> used to map <see cref="IReceiverMessage"/> headers to
        /// CloudEvent attributes. If <see langword="null"/>, then <see cref="DefaultProtocolBinding"/>
        /// is used instead.
        /// </param>
        public CloudEvent(IReceiverMessage receiverMessage, IProtocolBinding? protocolBinding = null)
        {
            if (receiverMessage is null)
                throw new ArgumentNullException(nameof(receiverMessage));

            FromReceiverMessage(receiverMessage, protocolBinding);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudEvent"/> class and sets its data and
        /// attributes according to the <a href=
        /// "https://github.com/cloudevents/spec/blob/v1.0/json-format.md">JSON Formatted
        /// CloudEvent</a>.
        /// </summary>
        /// <param name="jsonFormattedCloudEvent">
        /// A JSON Formatted CloudEvent.
        /// </param>
        public CloudEvent(string jsonFormattedCloudEvent)
        {
            if (string.IsNullOrEmpty(jsonFormattedCloudEvent))
                throw new ArgumentNullException(nameof(jsonFormattedCloudEvent));

            FromJson(jsonFormattedCloudEvent);
        }

        /// <summary>
        /// Whether to indent the JSON payload of sender messages rendered in Structured Mode.
        /// </summary>
        public static bool IndentStructuredMode { get; set; }

        /// <summary>
        /// Whether to indent the string data of an event when loading from a JSON-formatted event
        /// string. Applicable only when the 'data' memeber is a JSON object or array.
        /// </summary>
        public static bool IndentDataFromJson { get; set; }

        /// <summary>
        /// Gets or sets the default <see cref="IProtocolBinding"/>. The <see cref=
        /// "ProtocolBinding"/> property defaults to the value of this property if not set
        /// explicitly.
        /// </summary>
        public static IProtocolBinding DefaultProtocolBinding
        {
            get => _defaultProtocolBinding;
            set => _defaultProtocolBinding = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// The <see cref="IProtocolBinding"/> used to map CloudEvent attributes from <see cref=
        /// "IReceiverMessage"/> headers and to <see cref="SenderMessage"/> headers. Defaults to
        /// the value of the <see cref="DefaultProtocolBinding"/> property if not set explicitly.
        /// </summary>
        public IProtocolBinding ProtocolBinding
        {
            get => _protocolBinding ?? _defaultProtocolBinding;
            set => _protocolBinding = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// The attributes of this CloudEvent.
        /// </summary>
        public IDictionary<string, object> Attributes { get; } = new Dictionary<string, object>();

        /// <summary>
        /// REQUIRED. Identifies the event. Producers MUST ensure that source + id is unique for each
        /// distinct event. If a duplicate event is re-sent (e.g. due to a network error) it MAY have
        /// the same id. Consumers MAY assume that Events with identical source and id are duplicates.
        /// </summary>
        public string Id
        {
            get
            {
                if (Attributes.TryGetValue(IdAttribute, out var value) && value is string id)
                    return id;

                id = NewId();
                Attributes[IdAttribute] = id;
                return id;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException(nameof(value));
                Attributes[IdAttribute] = value;
            }
        }

        /// <summary>
        /// REQUIRED. Identifies the context in which an event happened. Often this will include
        /// information such as the type of the event source, the organization publishing the event
        /// or the process that produced the event. The exact syntax and semantics behind the data
        /// encoded in the URI is defined by the event producer.
        /// <para>Must be a valid relative or absolute URI.</para>
        /// </summary>
        public string? Source
        {
            get => Attributes.TryGetValue(SourceAttribute, out var value) && value is string source
                ? source
                : null;
            set
            {
                if (value != null)
                {
                    new Uri(value, UriKind.RelativeOrAbsolute);
                    Attributes[SourceAttribute] = value;
                }
                else
                    Attributes.Remove(SourceAttribute);
            }
        }

        /// <summary>
        /// REQUIRED. The version of the CloudEvents specification which the event uses. This
        /// enables the interpretation of the context. Always returns '1.0'.
        /// </summary>
        public string SpecVersion => _specVersion1_0;

        /// <summary>
        /// REQUIRED. This attribute contains a value describing the type of event related to the
        /// originating occurrence. Often this attribute is used for routing, observability, policy
        /// enforcement, etc. The format of this is producer defined and might include information
        /// such as the version of the type.
        /// </summary>
        public string? Type
        {
            get => Attributes.TryGetValue(TypeAttribute, out var value) && value is string type
                ? type
                : null;
            set
            {
                if (value != null)
                    Attributes[TypeAttribute] = value;
                else
                    Attributes.Remove(TypeAttribute);
            }
        }

        /// <summary>
        /// Content type of data value.
        /// <para>Must be a valid Content-Type value according to RFC 2616.</para>
        /// </summary>
        public string? DataContentType
        {
            get => Attributes.TryGetValue(DataContentTypeAttribute, out var value) && value is string dataContentType
                ? dataContentType
                : null;
            set
            {
                if (value != null)
                {
                    _contentType = MediaTypeHeaderValue.Parse(value);
                    Attributes[DataContentTypeAttribute] = value;
                }
                else
                {
                    _contentType = null;
                    Attributes.Remove(DataContentTypeAttribute);
                }
            }
        }

        /// <summary>
        /// Content type of data value as a <see cref="MediaTypeHeaderValue"/>.
        /// </summary>
        public MediaTypeHeaderValue? ContentType
        {
            get
            {
                if (_contentType != null)
                    return _contentType;
                if (string.IsNullOrEmpty(DataContentType))
                    return null;
                _contentType = MediaTypeHeaderValue.Parse(DataContentType);
                return _contentType;
            }
        }

        /// <summary>
        /// Identifies the schema that data adheres to. Incompatible changes to the schema SHOULD be
        /// reflected by a different URI.
        /// <para>Must be a valid relative or absolute URI.</para>
        /// </summary>
        public string? DataSchema
        {
            get => Attributes.TryGetValue(DataSchemaAttribute, out var value) && value is string dataSchema
                ? dataSchema
                : null;
            set
            {
                if (value != null)
                {
                    new Uri(value, UriKind.RelativeOrAbsolute);
                    Attributes[DataSchemaAttribute] = value;
                }
                else
                    Attributes.Remove(DataSchemaAttribute);
            }
        }

        /// <summary>
        /// This describes the subject of the event in the context of the event producer (identified
        /// by source). In publish-subscribe scenarios, a subscriber will typically subscribe to events
        /// emitted by a source, but the source identifier alone might not be sufficient as a qualifier
        /// for any specific event if the source context has internal sub-structure.
        /// 
        /// <para>Identifying the subject of the event in context metadata (opposed to only in the data
        /// payload) is particularly helpful in generic subscription filtering scenarios where middleware
        /// is unable to interpret the data content. In the above example, the subscriber might only be
        /// interested in blobs with names ending with '.jpg' or '.jpeg' and the subject attribute allows
        /// for constructing a simple and efficient string-suffix filter for that subset of events.</para>
        /// </summary>
        public string? Subject
        {
            get => Attributes.TryGetValue(SubjectAttribute, out var value) && value is string subject
                ? subject
                : null;
            set
            {
                if (value != null)
                    Attributes[SubjectAttribute] = value;
                else
                    Attributes.Remove(SubjectAttribute);
            }
        }

        /// <summary>
        /// Timestamp of when the occurrence happened.
        /// </summary>
        public DateTime Time
        {
            get
            {
                if (Attributes.TryGetValue(TimeAttribute, out var value))
                {
                    if (value is DateTime time)
                        return time;
                    if (value is string timeString && DateTime.TryParse(timeString, null, DateTimeStyles.RoundtripKind, out time))
                    {
                        Attributes[TimeAttribute] = time;
                        return time;
                    }
                }

                var currentTime = CurrentTime();
                Attributes[TimeAttribute] = currentTime;
                return currentTime;
            }
            set => Attributes[TimeAttribute] = value;
        }

        /// <summary>
        /// Message headers not related to CloudEvents.
        /// </summary>
        public IDictionary<string, object> Headers { get; } = new Dictionary<string, object>();

        /// <summary>
        /// Domain-specific information about the occurrence (i.e. the payload) as a string. This
        /// might include information about the occurrence, details about the data that was
        /// changed, or more.
        /// <para>To set this value, call either the <see cref="CloudEventExtensions
        /// .SetData{TCloudEvent}(TCloudEvent, string)"/> or <see cref="CloudEventExtensions
        /// .SetData{TCloudEvent, T}(TCloudEvent, T, DataSerialization)"/> extension method.</para>
        /// </summary>
        public string? StringData => _data as string;

        /// <summary>
        /// Domain-specific information about the occurrence (i.e. the payload) as a byte array.
        /// This might include information about the occurrence, details about the data that was
        /// changed, or more.
        /// <para>To set this value, call the <see cref="CloudEventExtensions.SetData{TCloudEvent}(
        /// TCloudEvent, byte[])"/> extension method.</para>
        /// </summary>
        public byte[]? BinaryData => _data as byte[];

        /// <summary>
        /// Converts the cloud event to a string in the
        /// <a href='https://github.com/cloudevents/spec/blob/v1.0/json-format.md'>JSON Format for
        /// CloudEvents</a>.
        /// </summary>
        /// <param name="indent">Whether to indent the resulting JSON string.</param>
        /// <returns>A JSON string representing the current <see cref="CloudEvent"/>.</returns>
        public virtual string ToJson(bool indent = false)
        {
            Validate();

            var jobject = new JObject
            {
                { "specversion", new JValue("1.0") }
            };

            foreach (var attribute in Attributes)
                jobject.Add(attribute.Key, JToken.FromObject(attribute.Value));

            if (_data is byte[] binaryData)
                jobject["data_base64"] = Convert.ToBase64String(binaryData);
            else if (_data is string stringData)
            {
                JToken dataToken;
                try { dataToken = JToken.Parse(stringData); }
                catch { dataToken = new JValue(stringData); }
                jobject["data"] = dataToken;
            }

            return jobject.ToString(indent ? Formatting.Indented : Formatting.None);
        }

        private void FromJson(string json)
        {
            var jobject = JObject.Parse(json);

            if (jobject.TryGetValue("data_base64", out var token))
            {
                if (token is JValue jvalue)
                {
                    if (jvalue.Value is string stringValue)
                        try { _data = Convert.FromBase64String(stringValue); }
                        catch (Exception ex) { throw new CloudEventValidationException("'data_base64' must have a valid base-64 encoded binary value.", ex); }
                    else if (jvalue.Value != null)
                        throw new CloudEventValidationException("'data_base64' must have a string value.");
                }
                else
                    throw new CloudEventValidationException("'data_base64' must have a string value.");

                if (_data != null
                    && jobject.TryGetValue("data", out token)
                    && (!(token is JValue jv) || jv.Value != null))
                    throw new CloudEventValidationException("'data_base64' and 'data' cannot both have values.");
            }
            
            if (jobject.TryGetValue("data", out token))
            {
                if (token is JValue jvalue)
                {
                    if (jvalue.Value is string stringValue)
                        _data = stringValue;
                    else if (jvalue.Value is DateTime dateTime)
                        _data = dateTime.ToString("O");
                    else if (jvalue.Value is bool b)
                        _data = b ? "true" : "false";
                    else
                        _data = jvalue.Value?.ToString();
                }
                else
                    _data = token.ToString(IndentDataFromJson ? Formatting.Indented : Formatting.None);
            }

            foreach (var attribute in jobject)
            {
                if (attribute.Key == "data" || attribute.Key == "data_base64")
                    continue;

                if (attribute.Key == "specversion")
                {
                    if (attribute.Value is JValue jv && jv.Value?.ToString() == "1.0")
                        continue;

                    throw new CloudEventValidationException(
                        $"Invalid '{SpecVersionAttribute}' attribute. Expected '{_specVersion1_0}', but was '{attribute.Value}'.");
                }

                if (attribute.Value is JValue jvalue)
                    Attributes[attribute.Key] = jvalue.Value;
                else
                    throw new CloudEventValidationException(
                        $"Invalid value for '{attribute.Key}' member: {attribute.Value.ToString(Formatting.Indented)}");
            }
        }

        /// <summary>
        /// Creates a <see cref="SenderMessage"/> with headers mapped from the attributes of this cloud event.
        /// </summary>
        /// <param name="structuredMode">
        /// <see langword="true"/> to render in Structured Mode, otherwise <see langword="false"/>
        /// to render in Binary Mode.
        /// </param>
        /// <returns>The mapped <see cref="SenderMessage"/>.</returns>
        /// <exception cref="CloudEventValidationException">If the cloud event is invalid.</exception>
        public virtual SenderMessage ToSenderMessage(bool structuredMode = false)
        {
            Validate();

            SenderMessage senderMessage;

            if (structuredMode)
            {
                senderMessage = new SenderMessage(ToJson(IndentStructuredMode));
                senderMessage.Headers[StructuredModeContentTypeHeader] = StructuredModeJsonMediaType;
                
                foreach (var header in Headers)
                    senderMessage.Headers[header.Key] = header.Value;
            }
            else
            {
                if (_data is string stringData)
                    senderMessage = new SenderMessage(stringData);
                else if (_data is byte[] binaryData)
                    senderMessage = new SenderMessage(binaryData);
                else
                    senderMessage = new SenderMessage("");

                senderMessage.Headers[ProtocolBinding.GetHeaderName(SpecVersionAttribute)] = SpecVersion;

                foreach (var attribute in Attributes)
                    senderMessage.Headers[ProtocolBinding.GetHeaderName(attribute.Key)] = attribute.Value;

                foreach (var header in Headers)
                    senderMessage.Headers[header.Key] = header.Value;

                ProtocolBinding.Bind(this, senderMessage); 
            }

            return senderMessage;
        }

        private void FromReceiverMessage(IReceiverMessage receiverMessage, IProtocolBinding? protocolBinding)
        {
            ProtocolBinding = protocolBinding ?? DefaultProtocolBinding;

            if (IsStructuredMode(receiverMessage))
            {
                FromJson(receiverMessage.StringPayload);

                foreach (var header in receiverMessage.Headers)
                    if (header.Key != StructuredModeContentTypeHeader)
                        Headers.Add(header);
            }
            else
            {
                _data = receiverMessage.IsBinary()
                    ? (object)receiverMessage.BinaryPayload
                    : receiverMessage.StringPayload;

                foreach (var header in receiverMessage.Headers)
                {
                    var attributeName = ProtocolBinding.GetAttributeName(header.Key, out bool isCloudEventAttribute);

                    if (isCloudEventAttribute)
                        Attributes.Add(attributeName, header.Value);
                    else
                        Headers.Add(attributeName, header.Value);
                }

                if (Attributes.TryGetValue(SpecVersionAttribute, out var value) && value is string specVersion)
                {
                    if (specVersion != _specVersion1_0)
                        throw new CloudEventValidationException(
                            $"Invalid '{SpecVersionAttribute}' attribute. Expected '{_specVersion1_0}', but was '{specVersion}'.");
                    Attributes.Remove(SpecVersionAttribute);
                }

                ProtocolBinding.Bind(receiverMessage, this);
            }
        }

        /// <summary>
        /// Creates an <see cref="HttpRequestMessage"/> with headers mapped from the attributes of this cloud event.
        /// </summary>
        /// <param name="requestUri">A string that represents the request <see cref="Uri"/>.</param>
        /// <param name="structuredMode">
        /// <see langword="true"/> to render in Structured Mode, otherwise <see langword="false"/>
        /// to render in Binary Mode.
        /// </param>
        /// <returns>The mapped <see cref="HttpRequestMessage"/>.</returns>
        public HttpRequestMessage ToHttpRequestMessage(string? requestUri = null, bool structuredMode = false) =>
            ToHttpRequestMessage(HttpMethod.Get, requestUri, structuredMode);

        /// <summary>
        /// Creates an <see cref="HttpRequestMessage"/> with headers mapped from the attributes of this cloud event.
        /// </summary>
        /// <param name="method">The HTTP method of the request.</param>
        /// <param name="requestUri">A string that represents the request <see cref="Uri"/>.</param>
        /// <param name="structuredMode">
        /// <see langword="true"/> to render in Structured Mode, otherwise <see langword="false"/>
        /// to render in Binary Mode.
        /// </param>
        /// <returns>The mapped <see cref="HttpRequestMessage"/>.</returns>
        public HttpRequestMessage ToHttpRequestMessage(HttpMethod method, string? requestUri = null, bool structuredMode = false)
        {
            if (method is null)
                throw new ArgumentNullException(nameof(method));

            var message = ToSenderMessage(structuredMode);
            var request = new HttpRequestMessage(method, requestUri);

            if (message.IsBinary)
                request.Content = new ByteArrayContent(message.BinaryPayload);
            else
                request.Content = new StringContent(message.StringPayload);

            if (DataContentType != null)
                request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(DataContentType);

            foreach (var header in message.Headers)
            {
                var headers = IsContentHeader(header.Key)
                    ? request.Content.Headers
                    : (HttpHeaders)request.Headers;

                AddHeader(headers, header.Key, header.Value?.ToString());
            }

            return request;
        }

        /// <summary>
        /// Converts the <see cref="CloudEvent"/> to a <see cref="SenderMessage"/>.
        /// </summary>
        /// <param name="cloudEvent">The <see cref="CloudEvent"/> to convert to a <see cref="SenderMessage"/>.</param>
        public static implicit operator SenderMessage(CloudEvent cloudEvent) =>
            cloudEvent?.ToSenderMessage()!;

        /// <summary>
        /// Ensures that the cloud event is valid - throws a <see cref="CloudEventValidationException"/>
        /// if it is not.
        /// </summary>
        /// <exception cref="CloudEventValidationException">If the cloud event is invalid.</exception>
        public virtual void Validate()
        {
            // Ensure that the id attribute exists.
            _ = Id;

            if (Source is null)
                throw new CloudEventValidationException("Source cannot be null.");

            if (string.IsNullOrEmpty(Type))
                throw new CloudEventValidationException("Type cannot be null or empty.");

            // Ensure that the time attribute exists.
            _ = Time;
        }

        /// <summary>
        /// Ensures that the required CloudEvent attributes are present.
        /// </summary>
        /// <param name="senderMessage">The <see cref="SenderMessage"/> to validate.</param>
        /// <param name="protocolBinding">
        /// The <see cref="IProtocolBinding"/> used to map CloudEvent attributes to <see cref="SenderMessage"/>
        /// headers. If <see langword="null"/>, then <see cref="DefaultProtocolBinding"/> is used instead.
        /// </param>
        /// <exception cref="CloudEventValidationException">
        /// If the <see cref="SenderMessage"/> is not valid.
        /// </exception>
        public static void Validate(SenderMessage senderMessage, IProtocolBinding? protocolBinding = null)
        {
            if (senderMessage is null)
                throw new ArgumentNullException(nameof(senderMessage));

            if (protocolBinding is null)
                protocolBinding = DefaultProtocolBinding;

            var specVersionHeader = protocolBinding.GetHeaderName(SpecVersionAttribute);
            if (!TryGetHeaderValue<string>(senderMessage, specVersionHeader, out var specVersion))
                throw new CloudEventValidationException($"The '{specVersionHeader}' header is missing from the SenderMessage.");
            else if (specVersion != _specVersion1_0)
                throw new CloudEventValidationException($"The '{specVersionHeader}' header must have a value of '{_specVersion1_0}'.");

            var idHeader = protocolBinding.GetHeaderName(IdAttribute);
            if (!ContainsHeader<string>(senderMessage, idHeader))
                senderMessage.Headers[idHeader] = NewId();

            var sourceHeader = protocolBinding.GetHeaderName(SourceAttribute);
            if (!ContainsHeader<Uri>(senderMessage, sourceHeader))
                throw new CloudEventValidationException($"The '{sourceHeader}' header is missing from the SenderMessage.");

            var typeHeader = protocolBinding.GetHeaderName(TypeAttribute);
            if (!ContainsHeader<string>(senderMessage, typeHeader))
                throw new CloudEventValidationException($"The '{typeHeader}' header is missing from the SenderMessage.");

            var timeHeader = protocolBinding.GetHeaderName(TimeAttribute);
            if (!ContainsHeader<DateTime>(senderMessage, timeHeader))
                senderMessage.Headers[timeHeader] = CurrentTime();
        }

        /// <summary>
        /// Returns whether the <paramref name="senderMessage"/> has a header with a name matching
        /// the <paramref name="headerName"/> and a value of either type <typeparamref name="T"/>
        /// or a type convertible to <typeparamref name="T"/>.
        /// </summary>
        /// <param name="senderMessage">The sender message to check</param>
        /// <param name="headerName">The name of the header.</param>
        /// <typeparam name="T">The type of the header value.</typeparam>
        /// <returns>
        /// <see langword="true"/> if the the <paramref name="senderMessage"/> has a header with a
        /// name matching the <paramref name="headerName"/> and a value of either type
        /// <typeparamref name="T"/> or a type convertible to <typeparamref name="T"/>; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        protected static bool ContainsHeader<T>(SenderMessage senderMessage, string headerName)
        {
            if (senderMessage.Headers.TryGetValue(headerName, out var objectValue))
            {
                switch (objectValue)
                {
                    case T _:
                        return true;
                    case null:
                        return false;
                }

                var converter = TypeDescriptor.GetConverter(typeof(T));
                if (converter.CanConvertFrom(objectValue.GetType()))
                    try
                    {
                        converter.ConvertFrom(objectValue);
                        return true;
                    }
                    catch { }

                converter = TypeDescriptor.GetConverter(objectValue);
                if (converter.CanConvertTo(typeof(T)))
                    try
                    {
                        converter.ConvertTo(objectValue, typeof(T));
                        return true;
                    }
                    catch { }
            }

            return false;
        }

        /// <summary>
        /// Gets the value of the header with the specified name as type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the header value.</typeparam>
        /// <param name="senderMessage">The <see cref="SenderMessage"/>.</param>
        /// <param name="headerName">The name of the header.</param>
        /// <param name="value">
        /// When this method returns, the value of the header with the specified name, if the
        /// header is found; otherwise, the default value for the type of the <paramref name="
        /// value"/> parameter. This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the the <paramref name="senderMessage"/> has a header with a
        /// name matching the <paramref name="headerName"/> and a value of either type
        /// <typeparamref name="T"/> or a type convertible to <typeparamref name="T"/>; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        protected static bool TryGetHeaderValue<T>(SenderMessage senderMessage, string headerName, out T value)
        {
            if (senderMessage.Headers.TryGetValue(headerName, out var objectValue))
            {
                switch (objectValue)
                {
                    case T variable:
                        value = variable;
                        return true;
                    case null:
                        value = default;
                        return false;
                }

                if (typeof(T) == typeof(DateTime) && objectValue is string stringValue)
                {
                    if (DateTime.TryParse(stringValue, null, DateTimeStyles.RoundtripKind, out var dateTimeValue))
                    {
                        value = (T)(object)dateTimeValue;
                        return true;
                    }
                }

                var converter = TypeDescriptor.GetConverter(typeof(T));
                if (converter.CanConvertFrom(objectValue.GetType()))
                {
                    try
                    {
                        value = (T)converter.ConvertFrom(objectValue);
                        return true;
                    }
                    catch
                    {
                    }
                }

                converter = TypeDescriptor.GetConverter(objectValue);
                if (converter.CanConvertTo(typeof(T)))
                {
                    try
                    {
                        value = (T)converter.ConvertTo(objectValue, typeof(T));
                        return true;
                    }
                    catch
                    {
                    }
                }
            }

            value = default;
            return false;
        }

        internal void SetDataField(string data) => _data = data;

        internal void SetDataField(byte[] data) => _data = data;

        internal void ClearDataField() => _data = null;

        private static string NewId() => Guid.NewGuid().ToString();

        private static DateTime CurrentTime() => DateTime.UtcNow;

        private static bool IsStructuredMode(IReceiverMessage receiverMessage) =>
            receiverMessage.Headers.TryGetValue(StructuredModeContentTypeHeader, out string contentType)
                && contentType.StartsWith(StructuredModeMediaTypePrefix);

    }
}
