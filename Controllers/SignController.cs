using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Ocsp;
using Signer.Dto;
using Signer.Services;
using Signer.Services.Shared;
using System.Reflection.Metadata;

namespace Signer.Controllers
{
    [Route("api")]
    [ApiController]
    public class SignController(ISignService signService, IFileUpload fileUpload) : ControllerBase
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
        public async Task<IActionResult> SignFile([FromForm] SignFileForm form)
        {
            var savedPath = await fileUpload.SaveFileAsync(form.File, "doc");
            return Ok(_signService.SignFile(form.Pin, form.Thumbprint, savedPath, ""));
        }
    }
}
