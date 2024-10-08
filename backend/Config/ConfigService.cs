using Gis.Net.Core.Services;
using Gis.Net.Istat;
using Gis.Net.Istat.Models;

namespace EcoSensorApi.Config;

/// <inheritdoc />
public class ConfigService : ServiceCore<ConfigModel, ConfigDto, ConfigQuery, ConfigRequest, EcoSensorDbContext>
{
    private readonly IIStatService<IstatContext> _istatService;
    
    /// <inheritdoc />
    public ConfigService(ILogger<ConfigService> logger, ConfigRepository repository, IIStatService<IstatContext> istatService) : 
        base(logger, repository)
    {
        _istatService = istatService;
    }

    /// <summary>
    /// Reading the GeoJson file of the limits of the Italian regions and municipalities
    /// Info: https://github.com/openpolis/geojson-italy/blob/master/geojson/
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<List<BBoxConfig>> BBoxGeometries(ConfigQuery? query)
    {
        var layers = await List(query);
        
        if (layers is null)
        {
            const string msg = "I can't read the configuration layers";
            Logger.LogError(msg);
            throw new Exception(msg);
        }

        var resultBboxConfigList = new List<BBoxConfig>();

        foreach (var layer in layers)
        {
            var istat = await _istatService.GetMunicipalities(new LimitsItMunicipality
            {
                RegName = layer.RegionName,
                RegIstatCodeNum = layer.RegionCode,
                ProvName = layer.ProvName,
                ProvIstatCodeNum = layer.ProvCode,
                ComIstatCodeNum = layer.CityCode,
                Name = layer.CityName,
            });
            
            if (istat is null)
            {
                const string msg = "I can't read the Istat data";
                Logger.LogError(msg);
                throw new Exception(msg);
            }

            // For each item in the istat list, add a new BBoxConfig object to the resultBboxConfigList
            foreach (var item in istat)
                if (item.WkbGeometry is not null)
                    resultBboxConfigList.Add(new BBoxConfig(item.WkbGeometry, $"{layer.EntityKey}:{layer.TypeMonitoringData}", layer.CityName ?? layer.EntityKey));
        }

        return resultBboxConfigList;
    }
}