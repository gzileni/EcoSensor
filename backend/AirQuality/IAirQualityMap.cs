namespace EcoSensorApi.AirQuality;

/// <summary>
/// Interface representing an air quality map.
/// </summary>
public interface IAirQualityMap : IDataMap
{
    /// <summary>
    /// Gets or sets the pollution level.
    /// </summary>
    /// <value>The pollution level.</value>
    int Pollution { get; set; }
    
    /// <summary>
    /// Gets or sets the description of the pollution level.
    /// </summary>
    /// <value>The description of the pollution level.</value>
    string PollutionDescription { get; set; }
}