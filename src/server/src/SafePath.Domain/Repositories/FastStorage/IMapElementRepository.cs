using SafePath.Entities;
using SafePath.Entities.FastStorage;
using System;
using System.Collections.Generic;

namespace SafePath.Repositories.FastStorage
{
    public interface IMapElementRepository : IFastStorageRepositoryBase<MapElement>
    {
        IList<MapElement> GetByAreaId(Guid areaId);

        MapElement? GetById(int id, bool includeDetails = true);

        IList<MapElement> GetByEdgeId(ulong edgeId, bool includeDetails = true);

        int BulkDeleteCrimeDataByCoordinates(IEnumerable<CoordinatesDto> coordinates);

        IList<MapElement>? FindCrimeDataByCoordinates(IEnumerable<CoordinatesDto> coordinates);

        IList<MapElement> GetByCoordinates(float latitude, float longitude);
        IList<MapElement> FindCrimeDataByEdgeIds(IList<uint> list);
    }
}
