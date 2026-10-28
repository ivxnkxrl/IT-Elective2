using System;

class Activity1
{
    static void Main(string[] args)
    {
        // Declaring variables
        decimal tuitionFee = 15000.00m;
        bool isPassed = true;
        Console.Write("Enter your name: ");
        string studentName = Console.ReadLine() ?? "";
        Console.Write("Enter your course: ");
        string studentCourse = Console.ReadLine() ?? "";
        Console.Write("Enter your year level: ");
        int yearLevel = int.Parse(Console.ReadLine() ?? "0");

        Console.Write("Enter quiz 1 score: ");
        double quiz1 = double.Parse(Console.ReadLine() ?? "0");
        Console.Write("Enter quiz 2 score: ");
        double quiz2 = double.Parse(Console.ReadLine() ?? "0");
        Console.Write("Enter quiz 3 score: ");
        double quiz3 = double.Parse(Console.ReadLine() ?? "0");

        // Displaying the student profile
        Console.WriteLine("\n===== STUDENT PROFILE =====");
        Console.WriteLine($"\nName: {studentName}");
        Console.WriteLine($"Course: {studentCourse}");
        Console.WriteLine($"Year Level: {yearLevel}");
        double average = (quiz1 + quiz2 + quiz3) / 3;
        Console.WriteLine($"Average: {average:F2}");
        Console.WriteLine($"Tuition Fee: {tuitionFee}");

        // Determine if the student passed or failed
        if (average >= 75.0)
        {
            isPassed = true;
        }
        else
        {
            isPassed = false;
        }
        Console.WriteLine($"Passed: {isPassed}");
    }
}