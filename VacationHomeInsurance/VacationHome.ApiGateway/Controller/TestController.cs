using Microsoft.AspNetCore.Mvc;


namespace VacationHome.ApiGateway.Controllers
{
    [ApiController]
    [Route("test")]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                Message = "Gateway Controller Working",
                Time = DateTime.UtcNow
            });
        }


        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok("Pong");
        }
    }
}
