using System.Data;
using Microsoft.Data.SqlClient;

namespace StudentSafeApp
{
    // DEFECT 12: The class and its members had no access modifiers and no documentation
    // RISK: unclear what is public, harder to maintain and review
    // FIX: made the class public, marked the connection string const/private, and added comments.
    public class StudentRepository
    {
        // DEFECT 1: The connection string had a hard-coded "sa" login and password ("admin123")
        // RISK: anyone who sees the code or the compiled file gets full administrator access to the
        // server, and the same password may be reused elsewhere
        // FIX: removed the password and used Windows (integrated) authentication. The string now
        // lives in this one constant only.
        private const string ConnectionString =
            "Server=(localdb)\\MSSQLLocalDB;Database=SchoolDb;Integrated Security=True;TrustServerCertificate=True";

        // CREATE: adds a new student. Returns the number of rows inserted.
        // DEFECT 2: Insert built its SQL by concatenating the text box values into the statement
        // RISK: SQL injection (for example a name of ' OR '1'='1 or '); DROP TABLE Students;--
        // could change or destroy data), and names with an apostrophe (O'Connor) would break it
        // FIX: used a parameterized query so user input is sent as data, never as SQL.
        //
        // DEFECT 3: The INSERT had no column list (INSERT INTO Students VALUES ...)
        // RISK: if columns are ever added or reordered, the values land in the wrong columns or the
        // statement fails
        // FIX: named the columns explicitly in the statement.
        //
        // DEFECT 4: The connection and command were never closed or disposed
        // RISK: leaked connections stay open, the connection pool runs out, and the app eventually
        // stops being able to reach the database
        // FIX: used "using var" so the connection and command are disposed automatically, and the
        // connection is opened only just before it is needed.
        //
        // DEFECT 5: Insert returned void, so the caller could not tell what happened
        // RISK: the form could not confirm a row was really added
        // FIX: the method now returns the number of rows affected.
        public int Insert(string id, string name, string course)
        {
            const string sql =
                "INSERT INTO Students (StudentId, Name, Course) VALUES (@StudentId, @Name, @Course)";

            using var connection = new SqlConnection(ConnectionString);
            using var command = new SqlCommand(sql, connection);

            command.Parameters.Add("@StudentId", SqlDbType.NVarChar, 20).Value = id;
            command.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = name;
            command.Parameters.Add("@Course", SqlDbType.NVarChar, 50).Value = course;

            connection.Open();
            return command.ExecuteNonQuery();
        }

        // READ: returns all students so the grid can show the latest database state.
        public DataTable GetAll()
        {
            const string sql = "SELECT StudentId, Name, Course FROM Students ORDER BY StudentId";

            DataTable table = new DataTable();

            using var connection = new SqlConnection(ConnectionString);
            using var command = new SqlCommand(sql, connection);

            connection.Open();
            using SqlDataReader reader = command.ExecuteReader();
            table.Load(reader);

            return table;
        }

        // UPDATE: changes the course of ONE student. Returns the number of rows affected.
        // DEFECT 6: The UPDATE statement had NO WHERE condition
        // RISK: it changed the course of EVERY student in the table, which is why the app
        // "updates the wrong student"
        // FIX: added WHERE StudentId = @StudentId so only the selected student changes, and
        // return the rows affected so the form can show "not found" when it is 0.
        public int UpdateCourse(string id, string course)
        {
            const string sql = "UPDATE Students SET Course = @Course WHERE StudentId = @StudentId";

            using var connection = new SqlConnection(ConnectionString);
            using var command = new SqlCommand(sql, connection);

            command.Parameters.Add("@Course", SqlDbType.NVarChar, 50).Value = course;
            command.Parameters.Add("@StudentId", SqlDbType.NVarChar, 20).Value = id;

            connection.Open();
            return command.ExecuteNonQuery();
        }

        // DELETE: removes ONE student. Returns the number of rows affected (0 = nothing deleted).
        // DEFECT 7: Delete concatenated the ID into the SQL, never released the connection, and
        // returned void
        // RISK: SQL injection (an ID such as ' OR '1'='1 would delete every student), leaked
        // connections, and no way to tell whether anything was removed
        // FIX: parameterized the ID, used "using var", and returned the rows affected.
        public int Delete(string id)
        {
            const string sql = "DELETE FROM Students WHERE StudentId = @StudentId";

            using var connection = new SqlConnection(ConnectionString);
            using var command = new SqlCommand(sql, connection);

            command.Parameters.Add("@StudentId", SqlDbType.NVarChar, 20).Value = id;

            connection.Open();
            return command.ExecuteNonQuery();
        }
    }
}