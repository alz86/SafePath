using System.Threading.Tasks;
using SafePath.Jobs.Args;
using SafePath.Services;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;

namespace SafePath.Jobs
{
    public class AreaParsingBackgroundJob : AsyncBackgroundJob<AreaParsingBackgroundJobArgs>, ITransientDependency
    {
        private readonly IOSMDataParsingService parsingService;
        private readonly IDataFilter dataFilter;

        public AreaParsingBackgroundJob(IOSMDataParsingService parsingService, IDataFilter dataFilter)
        {
            this.parsingService = parsingService;
            this.dataFilter = dataFilter;
        }

        public override async Task ExecuteAsync(AreaParsingBackgroundJobArgs args)
        {
            //multi-tenancy has to be disabled because this method is run in the context
            //of a system user, so by default it will filter all data by the host tenant
            using (dataFilter.Disable<IMultiTenant>())
            {
                await parsingService.ImportData(args.AreaId);
            }
        }
    }
}