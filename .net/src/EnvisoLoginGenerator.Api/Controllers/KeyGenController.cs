using System.Text;
using Api.Model;
using Library;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto.Parameters;

namespace EnvisoLoginGenerator.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class KeyGenController : ControllerBase
    {

        private readonly ILogger<KeyGenController> _logger;

        public KeyGenController(ILogger<KeyGenController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public async Task<LoginRequestDTO> GetSignature([FromBody] GetSignatureDto loginRequestDto)
        {
            LoginGenerator generator = new LoginGenerator();
            return generator.GenerateLogin(loginRequestDto.ApiKey, loginRequestDto.RsaKey);
        }
    }
}