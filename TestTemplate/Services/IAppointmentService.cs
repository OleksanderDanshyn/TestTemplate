using test1template.Models;

namespace test1template.Services;

public interface IAppointmentService
{
    Task<GetAppointmentDTO?> GetAppointment(int id);
    Task<string> AddAppointment(AddAppointmentDTO dto);
}