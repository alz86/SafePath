using SafePath.Entities.FastStorage;
using System.Collections.Generic;

namespace SafePath.Repositories.FastStorage
{
    public interface IMapElementRepository : IFastStorageRepositoryBase<MapElement>
    {
        MapElement? GetById(int id, bool includeDetails = true);

        IList<MapElement>? GetByEdgeId(ulong edgeId, bool includeDetails = true);
    }
}
