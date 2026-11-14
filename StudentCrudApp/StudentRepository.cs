using System.Data;
using Microsoft.Data.SqlClient;

namespace StudentCrudApp
{
    // Contains ALL database code for the Student table
    public class StudentRepository
    {
        private const string ConnectionString =
            "Server=(localdb)\\MSSQLLocalDB;Database=SchoolDb;Integrated Security=True;TrustServerCertificate=True";

        // CREATE: adds a new student.
        public int Insert(string id, string name, string course)
        {
            const string sql =
                "INSERT INTO Students (StudentId, Name, Course) VALUES (@StudentId, @Name, @Course)";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@StudentId", SqlDbType.NVarChar, 20).Value = id;
                command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = name;
                command.Parameters.Add("@Course", SqlDbType.NVarChar, 50).Value = course;

                connection.Open();
                return command.ExecuteNonQuery();
            }
        }

        // READ: returns every student as a DataTable.
        public DataTable GetAll()
        {
            const string sql = "SELECT StudentId, Name, Course FROM Students ORDER BY StudentId";

            DataTable table = new DataTable();

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    table.Load(reader);
                }
            }

            return table;
        }

        // UPDATE: changes the name and course of ONE student.
        public int Update(string id, string name, string course)
        {
            const string sql =
                "UPDATE Students SET Name = @Name, Course = @Course WHERE StudentId = @StudentId";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = name;
                command.Parameters.Add("@Course", SqlDbType.NVarChar, 50).Value = course;
                command.Parameters.Add("@StudentId", SqlDbType.NVarChar, 20).Value = id;

                connection.Open();
                return command.ExecuteNonQuery();
            }
        }

        // DELETE: removes ONE student.
        public int Delete(string id)
        {
            const string sql = "DELETE FROM Students WHERE StudentId = @StudentId";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@StudentId", SqlDbType.NVarChar, 20).Value = id;

                connection.Open();
                return command.ExecuteNonQuery();
            }
        }
    }
}