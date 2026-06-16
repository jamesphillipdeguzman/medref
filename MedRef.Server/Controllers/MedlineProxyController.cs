using Microsoft.AspNetCore.Mvc;
using MedRef.Server.Services;

namespace MedRef.Server.Controllers
{
    // This controller serves as a proxy to the MedlineService, allowing the client to fetch Medline data without exposing the service directly.
    [ApiController]
    [Route("api/[controller]")]

    // The MedlineProxyController provides an endpoint for the frontend to request medical information based on an ICD code. It uses the IMedlineService to retrieve the data and returns it in the response.
    public class MedlineProxyController : ControllerBase
    {
        private readonly IMedlineService _medlineService;

        public MedlineProxyController(IMedlineService medlineService)
        {
            _medlineService = medlineService;
        }
        // GET: api/medlineproxy?code=ICD_CODE
        [HttpGet]
        public async Task<IActionResult> Get(string code)
        {
            var result = await _medlineService.GetMedlineDataAsync(code);
            return Ok(result);
        }
    }
}