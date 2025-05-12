using Microsoft.AspNetCore.Mvc;
using test1template.Models;
using test1template.Services;

namespace test1template.Controllers
{
    [ApiController]
    [Route("api/appointments")]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentService _service;

        public AppointmentController(IAppointmentService service)
        {
            _service = service;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAppointment(int id)
        {
            var result = await _service.GetAppointment(id);
            return result is null ? NotFound("Not found.") : Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> AddAppointment([FromBody] AddAppointmentDTO dto)
        {
                var result = await _service.AddAppointment(dto);
                return result == "Success" ? Created("", null) :
                    result.Contains("not found") ? NotFound(result) :
                    result.Contains("already") ? Conflict(result) : 
                    BadRequest(result);
        }
    }
}