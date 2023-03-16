using Microsoft.AspNetCore.Mvc;
using Dadata;
using Dadata.Model;

namespace testproj.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {

        [ProducesResponseType(typeof(Address), 200)]
        [ApiVersion("1.0")]
        [HttpGet("")]
        public async Task<Address> GetData(string? address)
        {
            var token = "2b60223f7fd69d133ad48173ab12d3f6b1b69555";
            var secret = "a0ddc97df9d2514e2ddf8745b4354b3319fed777 ";
            var api = new CleanClientAsync(token, secret);
            var result = await api.Clean<Address>(address);
            return result;
        }
    }
}