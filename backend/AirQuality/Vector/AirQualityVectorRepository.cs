using AutoMapper;
using EcoSensorApi.AirQuality.Properties;
using Gis.Net.Vector.Repositories;

namespace EcoSensorApi.AirQuality.Vector;

/// <inheritdoc />
public class AirQualityVectorRepository : 
    GisVectorCoreManyRepository<AirQualityVectorModel, 
        AirQualityVectorDto, 
        AirQualityVectorQuery, 
        EcoSensorDbContext, 
        AirQualityPropertiesModel, 
        AirQualityPropertiesDto>
{
    /// <inheritdoc />
    public AirQualityVectorRepository(
        ILogger<AirQualityVectorRepository> logger,
        EcoSensorDbContext context,
        IMapper mapper) : 
        base(logger, context, mapper)
    {
    }

    /// <inheritdoc />
    protected override IQueryable<AirQualityVectorModel> ParseQueryParams(IQueryable<AirQualityVectorModel> query, AirQualityVectorQuery? queryByParams)
    {
        if (queryByParams?.SourceData != null) 
            query = query.Where(x => x.SourceData.Equals(queryByParams.SourceData));
        
        return base.ParseQueryParams(query, queryByParams);
    }
}