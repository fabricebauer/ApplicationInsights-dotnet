﻿namespace Microsoft.ApplicationInsights.DataContracts
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading;
    using Microsoft.ApplicationInsights.Channel;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.ApplicationInsights.Extensibility.Implementation;
    using Microsoft.ApplicationInsights.Extensibility.Implementation.External;

    /// <summary>
    /// Telemetry type used to track custom events.
    /// <a href="https://go.microsoft.com/fwlink/?linkid=525722#trackevent">Learn more</a>
    /// </summary>
    public sealed class EventTelemetry : ITelemetry, ISupportProperties, ISupportSampling, ISupportMetrics, IExtension
    {
        internal const string TelemetryName = "Event";

        internal readonly string BaseType = typeof(EventData).Name;
        internal readonly EventData Data;
        private readonly TelemetryContext context;
        private IExtension extension;

        private double? samplingPercentage;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventTelemetry"/> class.
        /// </summary>
        public EventTelemetry()
        {
            this.Data = new EventData();
            this.context = new TelemetryContext(this.Data.properties);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventTelemetry"/> class with the given <paramref name="name"/>.
        /// </summary>
        public EventTelemetry(string name) : this()
        {
            this.Name = name;
        }

        private EventTelemetry(EventTelemetry source)
        {
            this.Data = source.Data.DeepClone();
            this.context = source.context.DeepClone(this.Data.properties);
            this.Sequence = source.Sequence;
            this.Timestamp = source.Timestamp;
            this.samplingPercentage = source.samplingPercentage;
            this.extension = source.extension?.DeepClone();
        }

        /// <summary>
        /// Gets or sets date and time when event was recorded.
        /// </summary>
        public DateTimeOffset Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the value that defines absolute order of the telemetry item.
        /// </summary>
        public string Sequence { get; set; }

        /// <summary>
        /// Gets the context associated with the current telemetry item.
        /// </summary>
        public TelemetryContext Context
        {
            get { return this.context; }
        }

        /// <summary>
        /// Gets or sets gets the extension used to extend this telemetry instance using new strong typed object.
        /// </summary>
        public IExtension Extension
        {
            get { return this.extension; }
            set { this.extension = value; }
        }

        /// <summary>
        /// Gets or sets the name of the event.
        /// </summary>
        public string Name
        {
            get { return this.Data.name; }
            set { this.Data.name = value; }
        }

        /// <summary>
        /// Gets a dictionary of application-defined event metrics.
        /// <a href="https://go.microsoft.com/fwlink/?linkid=525722#properties">Learn more</a>
        /// </summary>
        public IDictionary<string, double> Metrics
        {
            get { return this.Data.measurements; }
        }

        /// <summary>
        /// Gets a dictionary of application-defined property names and values providing additional information about this event.
        /// <a href="https://go.microsoft.com/fwlink/?linkid=525722#properties">Learn more</a>
        /// </summary>
        public IDictionary<string, string> Properties
        {
            get { return this.Data.properties; }
        }

        /// <summary>
        /// Gets or sets data sampling percentage (between 0 and 100).
        /// Should be 100/n where n is an integer. <a href="https://go.microsoft.com/fwlink/?linkid=832969">Learn more</a>
        /// </summary>
        double? ISupportSampling.SamplingPercentage
        {
            get { return this.samplingPercentage; }
            set { this.samplingPercentage = value; }
        }

        /// <summary>
        /// Deeply clones a <see cref="EventTelemetry"/> object.
        /// </summary>
        /// <returns>A cloned instance.</returns>
        public ITelemetry DeepClone()
        {
            return new EventTelemetry(this);
        }

        /// <summary>
        /// Sanitizes the properties based on constraints.
        /// </summary>
        void ITelemetry.Sanitize()
        {
            this.Name = this.Name.SanitizeEventName();
            this.Name = Utils.PopulateRequiredStringValue(this.Name, "name", typeof(EventTelemetry).FullName);
            this.Properties.SanitizeProperties();
            this.Metrics.SanitizeMeasurements();
        }

        /// <summary>
        /// Deeply clones the Extension of <see cref="EventTelemetry"/> object.
        /// </summary>
        IExtension IExtension.DeepClone()
        {
            return new EventTelemetry(this);
        }

        /// <inheritdoc/>
        public void Serialize(ISerializationWriter serializationWriter)
        {            
            serializationWriter.WriteProperty("name", this.WriteTelemetryName(TelemetryName));
            serializationWriter.WriteProperty("time", this.Timestamp.UtcDateTime.ToString("o", CultureInfo.InvariantCulture));
            serializationWriter.WriteProperty("sampleRate", this.samplingPercentage);
            serializationWriter.WriteProperty("seq", this.Sequence);

            serializationWriter.WriteProperty("iKey", this.Context.InstrumentationKey);
            serializationWriter.WriteProperty("flags", this.Context.Flags);

            serializationWriter.WriteDictionary("tags", this.Context.SanitizedTags);
            Utils.CopyDictionary(this.Context.GlobalProperties, this.Data.properties);
            serializationWriter.WriteStartObject("data");
            serializationWriter.WriteProperty("baseType", this.BaseType);
            serializationWriter.WriteStartObject("baseData");

            serializationWriter.WriteProperty("ver", this.Data.ver);            
            serializationWriter.WriteProperty("name", this.Data.name);            

            serializationWriter.WriteDictionary("properties", this.Data.properties);
            serializationWriter.WriteDictionary("measurements", this.Data.measurements);
            serializationWriter.WriteEndObject(); // basedata
            serializationWriter.WriteEndObject(); // data            
        }
    }
}
