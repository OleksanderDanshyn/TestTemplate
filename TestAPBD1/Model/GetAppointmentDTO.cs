namespace test1template.Models;

public class GetAppointmentDTO
{
    public DateTime date { get; set; }
    public PatientDTO patient { get; set; }
    public DoctorDTO doctor { get; set; }
    public List<ServiceDTO> services { get; set; }
}