using Microsoft.AspNetCore.Mvc;
using Signer.Dto;
using Signer.Models;
using Signer.Services;
using Signer.Services.Shared.FileUpload;

namespace Signer.Controllers
{
    [Route("api")]
    [ApiController]
    public class SignController(ISignService signService, IFileUpload fileUpload, IHostEnvironment env) : ControllerBase
    {
        private readonly ISignService _signService = signService;

        [HttpGet]
        public IActionResult Get()
        {
            throw new NotImplementedException();
            //return Ok(_signService.Test());
        }

        [HttpGet("provider")]
        public IActionResult GetProvider()
        {
            var data = _signService.ScanProvider();
            return Ok(data);
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

        [HttpPost("sign-pdf-file")]
        public async Task<IActionResult> SignFile([FromForm] SignFileForm form)
        {
            var outputRoot = Path.Combine(env.ContentRootPath, "outputs");
            Directory.CreateDirectory(outputRoot);

            var inputPdfPath = await fileUpload.SaveFileAsync(form.File, "doc");
            var inputImagePath = await fileUpload.SaveFileAsync(form.Image, "image");
            var outputPdfPath = Path.Combine(outputRoot, Guid.NewGuid() + "_signed.pdf").Replace("\\", "/");

            _signService.SignPdfFile(form.Pin, form.Thumbprint, inputPdfPath, outputPdfPath, inputImagePath, new Position(form.Page, form.PosX, form.PosY, form.Width, form.Height));

            return File(
                new FileStream(outputPdfPath, FileMode.Open, FileAccess.Read),
                "application/pdf",
                Path.GetFileName(outputPdfPath),
                enableRangeProcessing: true
            );
        }
    }
}
