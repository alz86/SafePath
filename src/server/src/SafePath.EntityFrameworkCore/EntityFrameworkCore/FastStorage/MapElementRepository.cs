using SafePath.Entities.FastStorage;
using SafePath.Repositories.FastStorage;
using System.Linq.Dynamic.Core;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System;
using SafePath.Entities;

namespace SafePath.EntityFrameworkCore.FastStorage
{
    public class MapElementRepository : FastStorageRepositoryBase<MapElement, SqliteDbContext>, IMapElementRepository
    {
        public MapElementRepository(SqliteDbContext dbContext) : base(dbContext)
        {
        }

        public IList<MapElement> GetByAreaId(Guid areaId) => DbContext.MapElements.ToList();

        public MapElement? GetById(int id, bool includeDetails = true)
        {
            var query = DbContext.MapElements.Where(m => m.Id == id);
            if (includeDetails) query = query.Include(q => q.SafetyScoreElements);
            return query.FirstOrDefault();
        }

        public IList<MapElement> GetByEdgeId(ulong edgeId, bool includeDetails = true)
        {
            var query = DbContext.MapElements.Where(m => m.EdgeId == edgeId);
            if (includeDetails) query = query.Include(q => q.SafetyScoreElements);
            return query.ToList();
        }

        public IList<MapElement>? FindCrimeDataByCoordinates(IEnumerable<CoordinatesDto> coordinates)
        {
            //filter for types
            var typesToFilter = new SecurityElementTypes[] { SecurityElementTypes.CrimeReport_Severity_1, SecurityElementTypes.CrimeReport_Severity_2, SecurityElementTypes.CrimeReport_Severity_3, SecurityElementTypes.CrimeReport_Severity_4, SecurityElementTypes.CrimeReport_Severity_5 };
            var typesFilter = string.Join(',', typesToFilter.Select(e => ((int)e).ToString()));

            //filter for coordinates
            var coordList = coordinates.Select(c => $"({nameof(MapElement.Lat)} = {c.Lat} AND {nameof(MapElement.Lng)} = {c.Lng})");
            var coordFilter = string.Join(" OR ", coordList);

            var query = $"SELECT * FROM MapElements WHERE {nameof(MapElement.Type)} IN ({typesFilter}) AND ({coordFilter})";
            return DbContext.MapElements.FromSqlRaw(query).ToList();
        }

        public int BulkDeleteCrimeDataByCoordinates(IEnumerable<CoordinatesDto> coordinates)
        {
            //filter for types
            var typesToFilter = new SecurityElementTypes[] { SecurityElementTypes.CrimeReport_Severity_1, SecurityElementTypes.CrimeReport_Severity_2, SecurityElementTypes.CrimeReport_Severity_3, SecurityElementTypes.CrimeReport_Severity_4, SecurityElementTypes.CrimeReport_Severity_5 };
            var typesFilter = string.Join(',', typesToFilter.Select(e => ((int)e).ToString()));

            //filter for coordinates
            var coordList = coordinates.Select(c => $"({nameof(MapElement.Lat)} = {c.Lat} AND {nameof(MapElement.Lng)} = {c.Lng})");
            var coordFilter = string.Join(" OR ", coordList);

            //we need firs to list of the SafetyScore ids impacted by this deletion
            /*

            var selectQuery = @$"
SELECT ssem.MapElementId 
FROM MapElements me 
INNER JOIN SafetyScoreElementMapElement ssem ON me.Id = ssem.SafetyScoreElementId  
WHERE {nameof(MapElement.Type)} IN ({typesFilter}) AND ({coordFilter})";

            var ids = DbContext.Set<IdDTO>()
                 .FromSqlRaw(selectQuery)
                 .Select(dto => dto.MapElementId)
                 .ToList();

            */
            var query = $"DELETE FROM MapElements WHERE {nameof(MapElement.Type)} IN ({typesFilter}) AND ({coordFilter})";
            return DbContext.Database.ExecuteSqlRaw(query);
        }

        public IList<MapElement> GetByCoordinates(float latitude, float longitude) =>
            DbContext.MapElements.Where(m => m.Lat == latitude && m.Lng == longitude).ToList();

        public IList<MapElement> FindCrimeDataByEdgeIds(IList<uint> list)
        {
            SecurityElementTypes[] typesToFilter = [SecurityElementTypes.CrimeReport_Severity_1,
                SecurityElementTypes.CrimeReport_Severity_2,
                SecurityElementTypes.CrimeReport_Severity_3,
                SecurityElementTypes.CrimeReport_Severity_4,
                SecurityElementTypes.CrimeReport_Severity_5];

            return DbContext.MapElements
                .Where(m => m.EdgeId != null && list.Contains(m.EdgeId.Value) &&
                        typesToFilter.Contains(m.Type)
                ).ToList();
        }
    }
}
