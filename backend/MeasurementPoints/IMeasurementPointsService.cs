namespace EcoSensorApi.MeasurementPoints;

/// <summary>
/// Provides methods for synchronizing measurement points, seeding geographic features, and reading air quality values.
/// </summary>
public interface IMeasurementPointsService
{
    /// <summary>
    /// Synchronize the geometries read from the data sources (OpenStreetMap, etc.) and detect the measurement points
    /// starting from the centroid of each geometry, the South-West and North-East coordinates, if they are further away
    /// by a predefined constant distance from the centroid.
    /// </summary>
    /// <exception cref="Exception"></exception>
    Task<int> MeasurementPoints();

    /// <summary>
    /// Creates the database for geographic features and populates it with the data read from the data sources
    /// </summary>
    /// <returns></returns>
    Task<int> SeedFeatures();

    /// <summary>
    /// Reads air quality values from the API and stores them in the database
    /// </summary>
    /// <returns></returns>
    Task<int> AirQuality();
}