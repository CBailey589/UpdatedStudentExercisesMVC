using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using StudentExercisesMVC.Models;

namespace StudentExercisesMVC.Repositories
{
    public class InstructorRepository
    {
        private static IConfiguration _config;

        public static void SetConfig(IConfiguration configuration)
        {
            _config = configuration;
        }

        public static SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }

        public static List<Instructor> GetInstructors(string orderBy, string sortDirection)
        {
            string sql = @"
                            SELECT s.Id,
                                i.FirstName,
                                i.LastName,
                                i.SlackHandle,
                                i.CohortId,
                                c.Id CohortPk,
                                c.Designation
                            FROM Instructor i 
                            JOIN Cohort c ON s.CohortId = c.Id
                        ";

            if (orderBy != null)
            {
                sql += $"ORDER BY s.{orderBy} {sortDirection}";
            }

            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;
                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Instructor> instructors = new List<Instructor>();
                    while (reader.Read())
                    {
                        Instructor instructor = new Instructor
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                            Specialty = reader.GetString(reader.GetOrdinal("Specialty")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                            Cohort = new Cohort
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("CohortPk")),
                                Name = reader.GetString(reader.GetOrdinal("Designation"))
                            }
                        };

                        instructors.Add(instructor);
                    }

                    reader.Close();

                    return instructors;
                }
            }
        }

        public static Instructor CreateInstructor(Instructor instructor)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Instructor (FirstName, LastName, SlackHandle, Specialty, CohortId)         
                                         OUTPUT INSERTED.Id                                                       
                                         VALUES (@firstName, @lastName, @handle, @specialty, @cId)";
                    cmd.Parameters.Add(new SqlParameter("@firstName", instructor.FirstName));
                    cmd.Parameters.Add(new SqlParameter("@lastName", instructor.LastName));
                    cmd.Parameters.Add(new SqlParameter("@handle", instructor.SlackHandle));
                    cmd.Parameters.Add(new SqlParameter("@specialty", instructor.Specialty));
                    cmd.Parameters.Add(new SqlParameter("@cId", instructor.CohortId));

                    int newId = (int)cmd.ExecuteScalar();
                    instructor.Id = newId;
                    return instructor;
                }
            }
        }

        public static bool DeleteInstructor(int id)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"DELETE FROM Instructor WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected == 0) return false;
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        public static void UpdateInstructor(Instructor instructor)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"UPDATE Instructor
                                            SET FirstName = @firstName,
                                                LastName = @lastName,
                                                SlackHandle = @handle,
                                                Specialty = @specialty,
                                                CohortId = @cId
                                            WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@firstName", instructor.FirstName));
                    cmd.Parameters.Add(new SqlParameter("@lastName", instructor.LastName));
                    cmd.Parameters.Add(new SqlParameter("@handle", instructor.SlackHandle));
                    cmd.Parameters.Add(new SqlParameter("@specialty", instructor.Specialty));
                    cmd.Parameters.Add(new SqlParameter("@cId", instructor.CohortId));
                    cmd.Parameters.Add(new SqlParameter("@id", instructor.Id));

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static Instructor GetInstructor(int id)
        {
            Instructor instructor = null;
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                            SELECT i.Id,
                                i.FirstName,
                                i.LastName,
                                i.SlackHandle,
                                i.Specialty
                                i.CohortId,
                                c.Id CohortPk,
                                c.Designation
                            FROM Instructor  i
                            JOIN Cohort c ON i.CohortId = c.Id
                        ";
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        if (instructor == null)
                        {
                            instructor = new Instructor
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                                Specialty = reader.GetString(reader.GetOrdinal("Specialty")),
                                CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                Cohort = new Cohort
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("CohortPk")),
                                    Name = reader.GetString(reader.GetOrdinal("Designation"))
                                }
                            };
                        }
                    }

                    reader.Close();
                    return instructor;
                }
            }
        }
    }
}
