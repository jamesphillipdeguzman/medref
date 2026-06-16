using Microsoft.AspNetCore.Mvc;
using MedRef.Server.Services;

namespace MedRef.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MedlineProxyController : ControllerBase
    {
        private readonly IMedlineService _medlineService;

        public MedlineProxyController(IMedlineService medlineService)
        {
            _medlineService = medlineService;
        }

        [HttpGet]
        public async Task<IActionResult> Get(string code)
        {
            var result = await _medlineService.GetMedlineDataAsync(code);
            return Ok(result);
        }
    }
}