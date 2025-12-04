using Microsoft.AspNetCore.Mvc;
using Signer.Services;
using Signer.Models;

namespace Signer.Controllers
{
    [Route("api")]
    [ApiController]
    public class SignController(ISignService signService) : ControllerBase
    {
        private readonly ISignService _signService = signService;

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_signService.Test());
        }

        [HttpGet("certs")]
        public IActionResult ListCert([FromQuery] CertQuery query)
        {
            return Ok(_signService.ListCerts(query.Pin));
        }
    }
}
