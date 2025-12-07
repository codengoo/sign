using Microsoft.AspNetCore.Mvc;
using Signer.Services;
using System.Reflection.Metadata;
using Signer.Dto;

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
            throw new NotImplementedException();
            //return Ok(_signService.Test());
        }

        [HttpGet("certs")]
        public IActionResult ListCert([FromQuery] CertQuery query)
        {
            var data = _signService.ListCerts(query.Pin);
            return Ok(data);
        }

        [HttpPost("sign")]
        public IActionResult SignHash([FromBody] SignBody body)
        {
            var data = _signService.SignHash(body.Pin, body.Thumbprint, body.HashToSignBase64);
            return Ok(data);
        }

        [HttpPost("sign-file")]
        public IActionResult SignFile([FromForm] SignFileForm form)
        {
            //return Ok(_signService.SignHash(form.Pin, form.Thumbprint));
            return Ok(form);
        }
    }
}
