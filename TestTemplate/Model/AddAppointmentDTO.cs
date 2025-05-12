namespace test1template.Models;

public class AddAppointmentDTO
{
    public int appointmentId { get; set; }
    public int patientId { get; set; }
    public string pwz { get; set; }
    public List <ServiceDTO> services { get; set; }
}