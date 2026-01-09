namespace Plugin_System;

public interface IDataExporter
{
    string Export(MatchData data);
}

public class JsonExporter : IDataExporter
{
    public string Export(MatchData data)
    {
        return $"[JSON] {{\"title\": \"{data.Title}\", \"score\": {data.Score} }}";
    }
}

public class XmlExporter : IDataExporter
{
    public string Export(MatchData data)
    {
        return $"[XML] <Match><Title>{data.Title}</Title><Score>{data.Score}</Score></Match>";
    }
}

public class CsvExporter : IDataExporter
{
    public string Export(MatchData data)
    {
        return $"[CSV] Title,Score,Date\n{data.Title},{data.Score},{data.Date}";
    }
}