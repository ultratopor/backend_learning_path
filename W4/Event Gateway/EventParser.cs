using System;
using System.Collections.Generic;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;

namespace Event_Gateway
{
    public static class EventParser
    {
        public const int EventSize = 16/*GUID*/ + sizeof(int) + sizeof(double) + sizeof(long); // 36

        // Метод пытается распарсить событие из буфера без аллокаций.
        // Возвращает true, если парсинг успешен.
        public static bool TryParse(ReadOnlySpan<byte> buffer, out SensorEvent sensorEvent)
        {
            sensorEvent = default;

            if (buffer.Length < EventSize)
            {
                return false;
            }

            sensorEvent = MemoryMarshal.Read<SensorEvent>(buffer);

            return true;
        }
    }
}
