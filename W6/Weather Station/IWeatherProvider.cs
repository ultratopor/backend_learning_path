namespace Weather_Station;

public interface IWeatherProvider
{
    string GetTemperature();
}

public class LocalSensor : IWeatherProvider
{
    public string GetTemperature() => "Local Sensor: +25C (Sunny)";
}

public class RemoteSatellite : IWeatherProvider
{
    public string GetTemperature() => "Arctic Satellite: -50C (Freezing)";
}