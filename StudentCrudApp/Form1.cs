using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SqlClient;
using Microsoft.Data.SqlClient;

namespace StudentCrudApp
{
    public partial class Form1 : Form
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

        // Database connection goes here.
        private readonly StudentRepository repository = new StudentRepository();

        public Form1()
        {
            BuildForm();
            Load += Form1_Load;
        }

        // Part 1: Form Design
        private void BuildForm()
        {
            Text = "Student Management";
            Font = new Font("Cascadia Code", 10);
            ClientSize = new Size(620, 560);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;

            // Input group
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

            // Buttons
            saveButton = new Button { Name = "saveButton", Text = "Save", Location = new Point(15, 200), Size = new Size(95, 34) };
            updateButton = new Button { Name = "updateButton", Text = "Update", Location = new Point(120, 200), Size = new Size(95, 34) };
            deleteButton = new Button { Name = "deleteButton", Text = "Delete", Location = new Point(225, 200), Size = new Size(95, 34) };
            clearButton = new Button { Name = "clearButton", Text = "Clear", Location = new Point(330, 200), Size = new Size(95, 34) };

            // GridView
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

            // Connect event handlers
            saveButton.Click += saveButton_Click;
            updateButton.Click += updateButton_Click;
            deleteButton.Click += deleteButton_Click;
            clearButton.Click += clearButton_Click;
            studentsGrid.CellClick += studentsGrid_CellClick;
        }

        // READ: show all students
        private void Form1_Load(object sender, EventArgs e)
        {
            LoadStudents();
        }

        // Gets the latest rows from the database
        private void LoadStudents()
        {
            try
            {
                DataTable table = repository.GetAll();
                studentsGrid.DataSource = table;
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.Message);
                ShowMessage("Could not load students. Check that SchoolDb exists and the connection string is correct.", true);
            }
        }

        // When row is clicked, copy the values to the input fields
        private void studentsGrid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataGridViewRow row = studentsGrid.Rows[e.RowIndex];
            studentIdTextBox.Text = row.Cells["StudentId"].Value.ToString();
            nameTextBox.Text = row.Cells["Name"].Value.ToString();
            courseComboBox.SelectedItem = row.Cells["Course"].Value.ToString();
        }

        // Create: save new student
        private void saveButton_Click(object sender, EventArgs e)
        {
            if (!TryGetInputs(out string id, out string name, out string course)) return;

            try
            {
                repository.Insert(id, name, course);
                LoadStudents();
                ClearInputs();
                ShowMessage("Student " + name + " was added successfully.", false);
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                // Primary key violation
                ShowMessage("A student with ID " + id + " already exists. Use a different ID.", true);
            }
            catch (SqlException)
            {
                ShowMessage("The student could not be saved. Please try again.", true);
            }
        }

        // Update: update existing student
        private void updateButton_Click(object sender, EventArgs e)
        {
            if (!TryGetInputs(out string id, out string name, out string course)) return;

            try
            {
                int rowsAffected = repository.Update(id, name, course);

                if (rowsAffected == 0)
                {
                    ShowMessage("No student with ID " + id + " was found, so nothing was updated.", true);
                    return;
                }

                LoadStudents();
                ShowMessage("Student " + id + " was updated successfully.", false);
            }
            catch (SqlException)
            {
                ShowMessage("The student could not be updated. Please try again.", true);
            }
        }

        // Delete: delete existing student
        private void deleteButton_Click(object sender, EventArgs e)
        {
            string id = studentIdTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(id))
            {
                ShowMessage("Please select a student from the grid to delete.", true);
                return;
            }

            // Ask for confirmation
            DialogResult answer = MessageBox.Show("Delete this student?", "Confirm", MessageBoxButtons.YesNo);
            if (answer != DialogResult.Yes) return;

            try
            {
                int rowsAffected = repository.Delete(id);

                if (rowsAffected == 0)
                {
                    ShowMessage("No student with ID " + id + " was found, so nothing was deleted.", true);
                    return;
                }

                LoadStudents();
                ClearInputs();
                ShowMessage("Student " + id + " was deleted.", false);
            }
            catch (SqlException)
            {
                ShowMessage("The student could not be deleted. Please try again.", true);
            }
        }

        // Clear: clear input fields
        private void clearButton_Click(object sender, EventArgs e)
        {
            ClearInputs();
            feedbackLabel.Text = "";
        }

        // Helper: get inputs and validate
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

        // Empty the input fields
        private void ClearInputs()
        {
            studentIdTextBox.Clear();
            nameTextBox.Clear();
            courseComboBox.SelectedIndex = -1;
        }

        // Shows a message in feedbackLabel
        private void ShowMessage(string message, bool isError)
        {
            feedbackLabel.ForeColor = isError ? Color.Firebrick : Color.DarkGreen;
            feedbackLabel.Text = message;
        }
    }
}