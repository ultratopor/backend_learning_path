using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogScalpel
{
    public enum LogSeverity
    {
        Unknown,
        Info,
        Warning,
        Error,
        Critical
    }

    public class LogEntryV2
    {
        public LogSeverity Severity { get; set; }
        public DateTime Date { get; set; }
        public string Message { get; set; } 
    }

    internal class SpanLogParser
    {
        public void Parse(ReadOnlySpan<char> logLine, out LogEntryV2 result)
        {
            
            var severityEnd = logLine.IndexOf(']');
            var severitySpan = logLine.Slice(1, severityEnd - 1);
            LogSeverity severity = ParseSeverity(severitySpan);

            // Парсим дату
            ReadOnlySpan<char> dateAndMessage = logLine.Slice(severityEnd + 2);
            
            var dateEnd = dateAndMessage.IndexOf(' ');
            var dateSpan = dateAndMessage.Slice(0, dateEnd);
            var date = DateTime.Parse(dateSpan);
            // Остаток - сообщение
            var messageSpan = dateAndMessage.Slice(dateEnd + 1);
            var message = messageSpan.ToString();

            result = new LogEntryV2
            {
                Severity = severity,
                Date = date,
                Message = message
            };
        }

        private static LogSeverity ParseSeverity(ReadOnlySpan<char> severitySpan)
        {
            if (severitySpan.SequenceEqual("INFO".AsSpan()))
                return LogSeverity.Info;
            if (severitySpan.SequenceEqual("WARNING".AsSpan()))
                return LogSeverity.Warning;
            if (severitySpan.SequenceEqual("ERROR".AsSpan()))
                return LogSeverity.Error;
            if (severitySpan.SequenceEqual("CRITICAL".AsSpan()))
                return LogSeverity.Critical;
            return LogSeverity.Unknown;
        }
    }

    public class LogEntry
    {
        public string Severity { get; set; }
        public DateTime Date { get; set; }
        public string Message { get; set; }
    }

    public class LegacyLogParser
    {
        public LogEntry Parse(string logLine)
        {
            // 1. Аллокация массива строк + аллокация каждой подстроки
            var parts = logLine.Split(' ');

            // "[INFO]" -> "INFO" (аллокация подстроки)
            var severity = parts[0].Trim('[', ']');

            // Аллокация при парсинге даты (внутри Parse)
            var date = DateTime.Parse(parts[1]);

            // Ужас: склейка остатка массива обратно в строку + аллокации
            var message = string.Join(" ", parts.Skip(2));

            return new LogEntry
            {
                Severity = severity,
                Date = date,
                Message = message
            };
        }
    }
}
