using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SafePath.Classes;
using Volo.Abp.Application.Services;
using Volo.Abp.DependencyInjection;

namespace SafePath.Services
{
    public interface IAreaSetupProgressService : IApplicationService
    {
        AreaSetupProgress GetProgress(Guid areaId);

        //TODO: should we have the method to report progress in the
        //same place than get allow to get it
        //TODO: hide this method from the API
        void MarkStepCompleted(Guid areaId, AreaSetupProgress progress);
    }

    

    [Dependency(ServiceLifetime.Singleton, ReplaceServices = true)]
    public class AreaSetupProgressService : SafePathAppService, IAreaSetupProgressService
    {
        private readonly ConcurrentDictionary<Guid, AreaSetupProgress> jobStatuses = new();

        public AreaSetupProgress GetProgress(Guid areaId)
        {
            if (jobStatuses.TryGetValue(areaId, out var progress))
            {
                return progress;
            }
            return AreaSetupProgress.NotStarted;
        }

        public void MarkStepCompleted(Guid areaId, AreaSetupProgress progress)
        {
            //since many tasks in the importing process, it may happen
            //that a step reports a progress and a previous one later on
            //reports that its tasks completed. For now, we always return
            //the greatest progress reported
            var current = GetProgress(areaId);
            if (current < progress)
                jobStatuses[areaId] = progress;
        }
    }
}