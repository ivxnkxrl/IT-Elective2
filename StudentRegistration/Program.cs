using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace StudentRegistration
{
    // =====================
    // Part 2: Classes (OOP)
    // =====================
    // Base class that represents any person.
    public class Person
    {
        public string Name { get; set; }

        public Person(string name)
        {
            Name = name;
        }

        // Virtual so derived classes (like Student) can override it.
        public virtual string GetSummary()
        {
            return "Person: " + Name;
        }
    }

    // Derived class that represents a student.
    public class Student : Person
    {
        // Encapsulation: private setter
        public string StudentID { get; private set; }

        public string Course { get; set; }

        // Passes the name to the base class constructor with : base(name).
        public Student(string studentId, string name, string course)
            : base(name)
        {
            StudentID = studentId;
            Course = course;
        }

        // Override the base summary.
        public override string GetSummary()
        {
            return "Saved! Student ID: " + StudentID +
                   " | Name: " + Name +
                   " | Course: " + Course;
        }
    }

    // ===================
    // Part 1: Form Design
    // ===================
    public class MainForm : Form
    {
        // Controls (the (Name) of each control is set exactly as required)
        private GroupBox studentGroupBox;
        private Label studentIdLabel;
        private TextBox studentIdTextBox;
        private Label nameLabel;
        private TextBox nameTextBox;
        private Label courseLabel;
        private ComboBox courseComboBox;
        private Button saveButton;
        private Button clearButton;
        private Label feedbackLabel;

        // In-memory storage for the students.
        private readonly List<Student> students = new List<Student>();

        public MainForm()
        {
            InitializeFormControls();
        }

        // Form layout: consistent alignment and spacing.
        private void InitializeFormControls()
        {
            // Form settings
            Text = "Student Registration";
            Font = new Font("Cascadia Code", 10F);
            ClientSize = new Size(440, 330);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;

            // Group box
            studentGroupBox = new GroupBox
            {
                Name = "studentGroupBox",
                Text = "Student Information",
                Location = new Point(15, 15),
                Size = new Size(410, 170)
            };

            // Row 1: Student ID
            studentIdLabel = new Label
            {
                Name = "studentIdLabel",
                Text = "Student ID:",
                Location = new Point(15, 37),
                AutoSize = true
            };
            studentIdTextBox = new TextBox
            {
                Name = "studentIdTextBox",
                Location = new Point(130, 34),
                Size = new Size(260, 27)
            };

            // Row 2: Student Name
            nameLabel = new Label
            {
                Name = "nameLabel",
                Text = "Student Name:",
                Location = new Point(15, 82),
                AutoSize = true
            };
            nameTextBox = new TextBox
            {
                Name = "nameTextBox",
                Location = new Point(130, 79),
                Size = new Size(260, 27)
            };

            // Row 3: Course
            courseLabel = new Label
            {
                Name = "courseLabel",
                Text = "Course:",
                Location = new Point(15, 127),
                AutoSize = true
            };
            courseComboBox = new ComboBox
            {
                Name = "courseComboBox",
                Location = new Point(130, 124),
                Size = new Size(260, 27),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            courseComboBox.Items.AddRange(new object[] { "BSIT", "BSCS", "BSIS" });

            studentGroupBox.Controls.AddRange(new Control[]
            {
                studentIdLabel, studentIdTextBox,
                nameLabel, nameTextBox,
                courseLabel, courseComboBox
            });

            // Buttons
            saveButton = new Button
            {
                Name = "saveButton",
                Text = "Save",
                Location = new Point(225, 200),
                Size = new Size(95, 34)
            };
            clearButton = new Button
            {
                Name = "clearButton",
                Text = "Clear",
                Location = new Point(330, 200),
                Size = new Size(95, 34)
            };

            // Feedback label
            feedbackLabel = new Label
            {
                Name = "feedbackLabel",
                Text = "",
                Location = new Point(15, 250),
                Size = new Size(410, 65),
                AutoSize = false
            };

            Controls.AddRange(new Control[]
            {
                studentGroupBox, saveButton, clearButton, feedbackLabel
            });

            // Pressing Enter triggers Save.
            AcceptButton = saveButton;

            // Part 3: connect the event handlers to the Click events
            saveButton.Click += saveButton_Click;
            clearButton.Click += clearButton_Click;
        }

        // ====================================================
        // Part 3 + Part 4: Event handlers and input validation
        // ====================================================

        private void saveButton_Click(object sender, EventArgs e)
        {
            // Read and trim the text from the input controls.
            string studentId = studentIdTextBox.Text.Trim();
            string studentName = nameTextBox.Text.Trim();

            // Validation: Student ID must not be empty or only spaces.
            if (string.IsNullOrWhiteSpace(studentId))
            {
                ShowError("Please enter a student ID.");
                studentIdTextBox.Focus();
                return;
            }

            // Validation: Student Name must not be empty or only spaces.
            if (string.IsNullOrWhiteSpace(studentName))
            {
                ShowError("Please enter a student name.");
                nameTextBox.Focus();
                return;
            }

            // Validation: a course must be selected (-1 means nothing selected).
            if (courseComboBox.SelectedIndex == -1)
            {
                ShowError("Please select a course.");
                courseComboBox.Focus();
                return;
            }

            string course = courseComboBox.Text;

            // Create the Student object and keep it in memory only.
            Student student = new Student(studentId, studentName, course);
            students.Add(student);

            // Show the summary as feedback to the user.
            feedbackLabel.ForeColor = Color.DarkGreen;
            feedbackLabel.Text = student.GetSummary();
        }

        private void clearButton_Click(object sender, EventArgs e)
        {
            // Empty the text boxes, reset the course selection, clear feedback.
            studentIdTextBox.Clear();
            nameTextBox.Clear();
            courseComboBox.SelectedIndex = -1;
            feedbackLabel.Text = "";

            // Put the cursor back on the first field for the next entry.
            studentIdTextBox.Focus();
        }

        // Displays an error message in red so it is easy to notice.
        private void ShowError(string message)
        {
            feedbackLabel.ForeColor = Color.Firebrick;
            feedbackLabel.Text = message;
        }
    }

    // ===========
    // Entry point
    // ===========

    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}

