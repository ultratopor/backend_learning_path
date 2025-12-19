using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Event_Gateway
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly struct SensorEvent(Guid sensorId, int eventTypeId, double value)
    {
        public readonly Guid SensorId = sensorId;     // 16 bytes
        public readonly int EventTypeId = eventTypeId;   // 4 bytes
        public readonly double Value = value;      // 8 bytes
        public readonly long TimestampUtc = DateTime.UtcNow.Ticks; // 8 bytes (using long is more efficient than DateTime for struct)
    }
}
