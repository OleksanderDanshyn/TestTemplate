using System.Globalization;
using test1template.Models;
using test1template.Services;
using Microsoft.Data.SqlClient;

namespace WorkshopApi.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly string _connectionString =
            "Data Source=db-mssql;Initial Catalog=2019SBD;Integrated Security=True;Trust Server Certificate=True";
        public async Task<GetAppointmentDTO?> GetAppointment(int id)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                string command = @"SELECT a.date, 
                                    p.first_name, p.last_name, p.date_of_birth,
                                    d.doctor_id, d.PWZ
                                    FROM Appointment a
                                    JOIN Patient p ON a.patient_id = p.patient_id
                                    JOIN Doctor d ON a.doctor_id = d.doctor_id
                                    WHERE a.appointment_id = @id";
                using (SqlCommand cmd = new SqlCommand(command, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);

                    using var reader = await cmd.ExecuteReaderAsync();
                    if (!reader.HasRows)
                    {
                        return null;
                    }

                    await reader.ReadAsync();
                    var appointment = new GetAppointmentDTO()
                    {
                        date = reader.GetDateTime(0),
                        patient = new PatientDTO()
                        {
                            firstName = reader.GetString(1),
                            lastName = reader.GetString(2),
                            dateOfBirth = reader.GetDateTime(3),
                        },
                        doctor = new DoctorDTO()
                        {
                            doctorId = reader.GetInt32(4),
                            pwz = reader.GetString(5),
                        },
                        services = new List<ServiceDTO>()
                    };
                    reader.Close();

                    string query = @"SELECT s.name, aps.service_fee
                                        FROM Appointment_Service aps
                                        JOIN Service s ON s.service_id = aps.service_id
                                        WHERE aps.appointment_id = @id";


                    using (SqlCommand getService = new SqlCommand(query, conn))
                    {
                        getService.Parameters.AddWithValue("@id", id);
                        using var serviceReader = await getService.ExecuteReaderAsync();

                        while (await serviceReader.ReadAsync())
                        {
                            appointment.services.Add(new ServiceDTO()
                            {
                                name = serviceReader.GetString(0),
                                serviceFee = serviceReader.GetDecimal(1),
                            });
                        }
                    }

                    return appointment;
                }
            }
        }

        public async Task<string> AddAppointment(AddAppointmentDTO dto)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                using (SqlCommand cmd = new SqlCommand("SELECT 1 FROM Appointment WHERE appointment_id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", dto.appointmentId);
                    var exists = await cmd.ExecuteScalarAsync();
                    if (exists != null)
                    {
                        return "Appointment already exists";
                    }
                }

                using (SqlCommand cmd = new SqlCommand("SELECT 1 FROM Patient WHERE patient_id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", dto.patientId);
                    var exists = await cmd.ExecuteScalarAsync();
                    if (exists == null)
                    {
                        return "Patient not found";
                    }
                }

                int? doctorId = null;
                using (SqlCommand cmd = new SqlCommand("SELECT doctor_id FROM Doctor WHERE PWZ = @pwz", conn))
                {
                    cmd.Parameters.AddWithValue("@pwz", dto.pwz);
                    var exists = await cmd.ExecuteScalarAsync();
                    if (exists == null)
                    {
                        return "Doctor not found";
                    }

                    doctorId = (int)exists;
                }

                foreach (var service in dto.services)
                {
                    using var serviceCheck = new SqlCommand("SELECT 1 FROM Service WHERE name = @name", conn);
                    serviceCheck.Parameters.AddWithValue("@name", service.name);
                    var exists = await serviceCheck.ExecuteScalarAsync();
                    if (exists == null)
                    {
                        return "Service not found";
                    }
                }

                string insertcmd = @"INSERT INTO Appointment (appointment_id, patient_id, doctor_id, date)
                                 VALUES (@id, @patientId, @doctorId, @date)";

                using (SqlCommand cmd = new SqlCommand(insertcmd, conn))
                {
                    cmd.Parameters.AddWithValue("@id", dto.appointmentId);
                    cmd.Parameters.AddWithValue("@patientId", dto.patientId);
                    cmd.Parameters.AddWithValue("@doctorId", doctorId);
                    cmd.Parameters.AddWithValue("@date", DateTime.Now);
                    await cmd.ExecuteNonQueryAsync();
                }

                foreach (var service in dto.services)
                {
                    int serviceId;
                    using (SqlCommand cmd = new SqlCommand("SELECT service_id FROM Service WHERE name = @name", conn))
                    {
                        cmd.Parameters.AddWithValue("@name", service.name);
                        serviceId = (int)(await cmd.ExecuteScalarAsync())!;
                    }

                    using (SqlCommand cmd = new SqlCommand(
                               @"INSERT INTO Appointment_Service (appointment_id, service_id, service_fee)
                                                            VALUES (@appId, @serviceId, @fee)", conn))
                    {
                        cmd.Parameters.AddWithValue("@appId", serviceId);
                        cmd.Parameters.AddWithValue("@serviceId", serviceId);
                        cmd.Parameters.AddWithValue("@serviceFee", service.serviceFee);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
                return "Appointment added";
            }
        }
    }
}