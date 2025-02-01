using z76_backend.Models;

namespace z76_backend.Services
{
    public class ExportDeclarationService : BaseService<ExportDeclarationEntity>, IExportDeclarationService
    {
        private readonly IConfiguration _configuration;
        public ExportDeclarationService(IConfiguration configuration) : base(configuration)
        {
            _configuration = configuration;
        }
    }
}
