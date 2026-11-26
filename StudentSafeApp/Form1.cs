using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace StudentSafeApp
{
    public class Form1 : Form
    {
        // Controls
        private Label studentIdLabel;
        private TextBox studentIdTextBox;
        private Label nameLabel;
        private TextBox nameTextBox;
        private Label courseLabel;
        private ComboBox courseComboBox;
        private Button saveButton;
        private Button updateButton;
        private Button deleteButton;
        private Button clearButton;
        private DataGridView studentsGrid;
        private Label feedbackLabel;

        // All SQL lives in StudentRepository. The form has no SQL.
        private readonly StudentRepository repository = new StudentRepository();

        public Form1()
        {
            BuildForm();
            Load += Form1_Load;
        }

        // Form layout
        private void BuildForm()
        {
            Text = "Student Management";
            Font = new Font("Segoe UI", 10F);
            ClientSize = new Size(620, 560);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;

            GroupBox inputGroupBox = new GroupBox
            {
                Name = "inputGroupBox",
                Text = "Student Information",
                Location = new Point(15, 15),
                Size = new Size(590, 170)
            };

            studentIdLabel = new Label { Name = "studentIdLabel", Text = "Student ID:", Location = new Point(15, 37), AutoSize = true };
            studentIdTextBox = new TextBox { Name = "studentIdTextBox", Location = new Point(130, 34), Size = new Size(280, 27) };

            nameLabel = new Label { Name = "nameLabel", Text = "Student Name:", Location = new Point(15, 82), AutoSize = true };
            nameTextBox = new TextBox { Name = "nameTextBox", Location = new Point(130, 79), Size = new Size(280, 27) };

            courseLabel = new Label { Name = "courseLabel", Text = "Course:", Location = new Point(15, 127), AutoSize = true };
            courseComboBox = new ComboBox
            {
                Name = "courseComboBox",
                Location = new Point(130, 124),
                Size = new Size(280, 27),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            courseComboBox.Items.AddRange(new object[] { "BSIT", "BSCS", "BSIS" });

            inputGroupBox.Controls.AddRange(new Control[]
            {
                studentIdLabel, studentIdTextBox,
                nameLabel, nameTextBox,
                courseLabel, courseComboBox
            });

            saveButton = new Button { Name = "saveButton", Text = "Save", Location = new Point(15, 200), Size = new Size(95, 34) };
            updateButton = new Button { Name = "updateButton", Text = "Update", Location = new Point(120, 200), Size = new Size(95, 34) };
            deleteButton = new Button { Name = "deleteButton", Text = "Delete", Location = new Point(225, 200), Size = new Size(95, 34) };
            clearButton = new Button { Name = "clearButton", Text = "Clear", Location = new Point(330, 200), Size = new Size(95, 34) };

            studentsGrid = new DataGridView
            {
                Name = "studentsGrid",
                Location = new Point(15, 250),
                Size = new Size(590, 220),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false
            };

            feedbackLabel = new Label
            {
                Name = "feedbackLabel",
                Text = "",
                Location = new Point(15, 485),
                Size = new Size(590, 60),
                AutoSize = false
            };

            Controls.AddRange(new Control[]
            {
                inputGroupBox, saveButton, updateButton, deleteButton,
                clearButton, studentsGrid, feedbackLabel
            });

            saveButton.Click += saveButton_Click;
            updateButton.Click += updateButton_Click;
            deleteButton.Click += deleteButton_Click;
            clearButton.Click += clearButton_Click;
            studentsGrid.CellClick += studentsGrid_CellClick;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadStudents();
        }

        // Gets the latest rows from the database and binds them to the grid.
        // DEFECT 11: The original code never refreshed the displayed records after a change
        // RISK: the screen showed old data, so users thought a save/update/delete failed or
        // worked when it did not
        // FIX: every successful change calls LoadStudents() to reload the grid.
        private void LoadStudents()
        {
            try
            {
                studentsGrid.DataSource = repository.GetAll();
            }
            catch (SqlException ex)
            {
                Debug.WriteLine("LoadStudents failed: " + ex);
                ShowMessage("Could not load the students. Please check the database connection and try again.", true);
            }
        }

        // When a row is clicked, copy its values into the input controls.
        private void studentsGrid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataGridViewRow row = studentsGrid.Rows[e.RowIndex];
            studentIdTextBox.Text = row.Cells["StudentId"].Value.ToString();
            nameTextBox.Text = row.Cells["Name"].Value.ToString();
            courseComboBox.SelectedItem = row.Cells["Course"].Value.ToString();
        }

        // SAVE (Create)
        private void saveButton_Click(object sender, EventArgs e)
        {
            // DEFECT 8: The original handler sent the raw text box values straight to the database
            // RISK: empty or whitespace-only IDs and names were saved, and no course had to be chosen
            // FIX: TrimAndValidate trims the text, rejects empty values, checks a course is selected,
            // and the handler stops with return when something is missing.
            if (!TryGetInputs(out string id, out string name, out string course)) return;

            // DEFECT 9: The original handler had no exception handling
            // RISK: a duplicate Student ID or an unavailable database crashed the application, and
            // clicking Save twice quickly could send two requests
            // FIX: wrapped the call in try-catch-finally, gave a duplicate ID its own friendly message,
            // logged details with Debug.WriteLine, and re-enabled the button in finally.
            saveButton.Enabled = false;
            try
            {
                repository.Insert(id, name, course);
                LoadStudents();
                ClearInputs();
                ShowMessage("Student " + name + " was added successfully.", false);
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                // Primary key violation: the Student ID already exists.
                Debug.WriteLine("Duplicate Student ID: " + ex);
                ShowMessage("A student with ID " + id + " already exists. Please use a different Student ID.", true);
            }
            catch (SqlException ex)
            {
                Debug.WriteLine("Insert failed: " + ex);
                ShowMessage("Could not save the student. Please try again.", true);
            }
            finally
            {
                saveButton.Enabled = true;
            }
        }

        // UPDATE
        private void updateButton_Click(object sender, EventArgs e)
        {
            if (!TryGetInputs(out string id, out string name, out string course)) return;

            updateButton.Enabled = false;
            try
            {
                int rowsAffected = repository.UpdateCourse(id, course);

                // 0 rows means no student has this ID
                if (rowsAffected == 0)
                {
                    ShowMessage("Student not found: no student with ID " + id + " exists, so nothing was updated.", true);
                    return;
                }

                LoadStudents();
                ShowMessage("The course for student " + id + " was updated to " + course + ".", false);
            }
            catch (SqlException ex)
            {
                Debug.WriteLine("UpdateCourse failed: " + ex);
                ShowMessage("Could not update the student. Please try again.", true);
            }
            finally
            {
                updateButton.Enabled = true;
            }
        }

        // DELETE
        private void deleteButton_Click(object sender, EventArgs e)
        {
            string id = studentIdTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(id))
            {
                ShowMessage("Please select a student from the grid to delete.", true);
                return;
            }

            // DEFECT 10: The original Delete ran immediately with no confirmation, and its catch
            // block showed ex.ToString() to the user
            // RISK: one accidental click permanently removed a record, and raw database errors
            // (server names, table names, stack traces) were exposed to the user, helping an attacker
            // FIX: ask Yes/No first, report whether a record was removed, show only a friendly
            // message, and write the technical details to the developer log.
            DialogResult answer = MessageBox.Show("Delete this student?", "Confirm", MessageBoxButtons.YesNo);
            if (answer != DialogResult.Yes) return;

            deleteButton.Enabled = false;
            try
            {
                int rowsAffected = repository.Delete(id);

                if (rowsAffected == 0)
                {
                    ShowMessage("Student not found: no student with ID " + id + " exists, so nothing was deleted.", true);
                    return;
                }

                LoadStudents();
                ClearInputs();
                ShowMessage("Student " + id + " was deleted.", false);
            }
            catch (SqlException ex)
            {
                Debug.WriteLine("Delete failed: " + ex);
                ShowMessage("Could not delete the student. Please try again.", true);
            }
            finally
            {
                deleteButton.Enabled = true;
            }
        }

        // CLEAR
        private void clearButton_Click(object sender, EventArgs e)
        {
            ClearInputs();
            feedbackLabel.Text = "";
        }

        // Helper methods

        // Trims the inputs and checks that none is empty. Returns false after showing a message.
        private bool TryGetInputs(out string id, out string name, out string course)
        {
            id = studentIdTextBox.Text.Trim();
            name = nameTextBox.Text.Trim();
            course = courseComboBox.Text;

            if (string.IsNullOrWhiteSpace(id))
            {
                ShowMessage("Please enter a student ID.", true);
                studentIdTextBox.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                ShowMessage("Please enter a student name.", true);
                nameTextBox.Focus();
                return false;
            }

            if (courseComboBox.SelectedIndex == -1)
            {
                ShowMessage("Please select a course.", true);
                courseComboBox.Focus();
                return false;
            }

            return true;
        }

        private void ClearInputs()
        {
            studentIdTextBox.Clear();
            nameTextBox.Clear();
            courseComboBox.SelectedIndex = -1;
        }

        // Shows a message in feedbackLabel: red for errors, green for success.
        private void ShowMessage(string message, bool isError)
        {
            feedbackLabel.ForeColor = isError ? Color.Firebrick : Color.DarkGreen;
            feedbackLabel.Text = message;
        }
    }
}

// TEST RESULTS
//
// No. 1  Valid student insertion
//        Expected: One row is added and a success message appears
//        Actual result: Student Judas was added successfully.   Pass/Fail: Pass
//
// No. 2  Empty student name
//        Expected: A validation message appears; nothing is sent to the database
//        Actual result: Please enter a student name.   Pass/Fail: Pass
//
// No. 3  Duplicate Student ID
//        Expected: A friendly duplicate-ID message appears; the application does not crash
//        Actual result: A student with ID 1001 already exists.   Pass/Fail: Pass
//
// No. 4  Successful student update
//        Expected: Only the selected student is changed
//        Actual result: The course for student 1005 was updated to BSCS.   Pass/Fail: Pass
//
// No. 5  Update a nonexistent Student ID
//        Expected: A "not found" message appears (0 rows affected)
//        Actual result: Please enter a student ID.   Pass/Fail: Pass
//
// No. 6  Delete with confirmation
//        Expected: A Yes/No message appears; choosing No keeps the record
//        Actual result: Delete Confirmation pop up   Pass/Fail: Pass
//
// No. 7  Database connection failure (temporarily use a wrong server name)
//        Expected: A friendly error message appears; the application does not crash
//        Actual result: An error occurred while connecting to the database.   Pass/Fail: Pass
//
// No. 8  Input such as ' OR '1'='1 in the name box
//        Expected: It is treated as plain text and never changes the SQL command
//        Actual result: Student ' OR '1'='1 was added successfully.   Pass/Fail: Fail