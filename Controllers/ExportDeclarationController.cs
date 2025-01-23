using Microsoft.AspNetCore.Mvc;
using z76_backend.Controllers;
using z76_backend.Models;
using z76_backend.Services;

namespace MyWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExportDeclarationsController : BaseController<ExportDeclartionEntity, IExportDeclarationService>
    {
        public ExportDeclarationsController(IExportDeclarationService service) : base(service)
        {

        }

    }
}
