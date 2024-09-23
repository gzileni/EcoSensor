using System.Text;
using EcoSensorApi.AirQuality.Properties;
using EcoSensorApi.MeasurementPoints;
using Gis.Net.Aws.AWSCore.Exceptions;
using Gis.Net.Aws.AWSCore.S3.Dto;
using Gis.Net.Aws.AWSCore.S3.Services;
using Gis.Net.Vector;
using NetTopologySuite.Features;

namespace EcoSensorApi.Aws;

/// <inheritdoc />
public class EcoSensorAws : IEcoSensorAws
{
    private readonly AirQualityPropertiesService _airQualityPropertiesService;
    private readonly IAwsBucketService _awsBucketService;
    private readonly DynamoDbEcoSensorService _dynamoDbEcoSensorService;
    private readonly ILogger<EcoSensorAws> _logger;

    public EcoSensorAws(AirQualityPropertiesService airQualityPropertiesService,
        IAwsBucketService awsBucketService, 
        DynamoDbEcoSensorService dynamoDbEcoSensorService, 
        ILogger<EcoSensorAws> logger)
    {
        _airQualityPropertiesService = airQualityPropertiesService;
        _awsBucketService = awsBucketService;
        _dynamoDbEcoSensorService = dynamoDbEcoSensorService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task SaveAitQualityToDynamoDb(string key, AwsS3ObjectDto objS3)
    {
        try
        {
            // Add a converter for the S3 object
            _dynamoDbEcoSensorService.AddConverter<AwsS3ObjectDto>();
            
            // Get the last date measure
            var nextTs = await _airQualityPropertiesService.LastDateMeasureAsync(new MeasurementsQuery
            {
                EntityKey = key,
                TypeMonitoringData = ETypeMonitoringData.AirQuality
            }) ?? DateTime.UtcNow.ToString("O");
            
            // Create a new DynamoDbEcoSensorModel item
            var item = new DynamoDbEcoSensorModel
            {
                Key = key,
                Data = objS3,
                NextTimeStamp = nextTs
            };
        
            // Insert or update the item in DynamoDb
            await _dynamoDbEcoSensorService.InsertOrUpdateAsync(item);
        }
        catch (Exception ex)
        {
            // Log the exception
            var msg = $"An error occurred while save object to DynamoDb - {ex.Message}";
            _logger.LogError(msg);
        }
    }

    private async Task<AwsS3ObjectDto?> WriteStreamToS3(string bucketName, string prefix, string key, MemoryStream memoryStream)
    {
        try
        {
            // Upload the FeatureCollection to S3
            var resultS3 = await _awsBucketService.Upload(new AwsS3BucketUploadDto(memoryStream)
            {
                BucketName = bucketName,
                Prefix = prefix,
                Key = key,
                Replace = true,
                Share = false
            }, default);

            _logger.LogInformation("The FeatureCollection was successfully uploaded to S3 with the result: {0}", key);

            return resultS3;
        } catch (AwsExceptions awsEx)
        {
            // Log the exception
            var msg = $"An error occurred while uploading the data to S3 - {awsEx.Message}";
            _logger.LogError(msg);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<AwsS3ObjectDto?> SaveFeatureCollectionToS3(string bucketName, string prefix, string key, FeatureCollection? featureCollection)
    {
        // Serialize the FeatureCollection to GeoJSON
        var geoJson = GisUtility.SerializeFeatureCollection(featureCollection);
        
        // Create a memory stream from the GeoJSON string
        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(geoJson));
        
        // Upload the FeatureCollection to S3
        return await WriteStreamToS3(bucketName, prefix, key, memoryStream);
    }

    /// <inheritdoc />
    public async Task<AwsS3ObjectDto?> SaveNextTimeStampToS3(string bucketName, string prefix, string key, string nextTs)
    {
        // Create a memory stream from the next timestamp
        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(nextTs));
        
        // Upload the next timestamp to S3
        return await WriteStreamToS3(bucketName, prefix, key, memoryStream);
    }
}