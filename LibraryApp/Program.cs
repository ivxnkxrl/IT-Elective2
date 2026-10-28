using System;

// Book Class represents a physical book in the library
class Book
{
    public string Title { get; set; }
    public string Author { get; set; }
    public string Isbn { get; set; }
    public bool IsAvailable { get; private set; }

    public Book(string title, string author, string isbn)
    {
        Title = title;
        Author = author;
        Isbn = isbn;
        IsAvailable = true;
    }

    // Method to borrow the book
    public void Borrow()
    {
        if (IsAvailable)
        {
            IsAvailable = false;
            Console.WriteLine($"\"{Title}\" has been borrowed.");
        }
        else
        {
            Console.WriteLine($"Sorry, '{Title}' is currently not available.");
        }
    }

    // Method to return the book
    public void Return()
    {
        if (!IsAvailable)
        {
            IsAvailable = true;
            Console.WriteLine($"\"{Title}\" has been returned.");
        }
        else
        {
            Console.WriteLine($"\"{Title}\" was not borrowed.");
        }
    }

    // Method to check out the book
    public virtual void DisplayInfo()
    {
        Console.WriteLine($"Title: {Title} | Author: {Author} | ISBN: {Isbn} | Available: {IsAvailable}");
    }
}

// EBook Class represents an electronic book in the library
class EBook : Book
{
    public double FileSizeMb { get; set; }
    public EBook(string title, string author, string isbn, double fileSizeMb)
        : base(title, author, isbn)
    {
        FileSizeMb = fileSizeMb;
    }

    // Override the DisplayInfo method to include file size
    public override void DisplayInfo()
    {
        Console.WriteLine($"Title: {Title} | Author: {Author} | ISBN: {Isbn} | Available: {IsAvailable} | File Size (MB): {FileSizeMb}");
    }
}

// Main Class Program
class Program
{
    static void Main(string[] args)
    {
        // Creating 2 Book objects and 1 EBook object
        Book book1 = new Book("Python Basics", "Ivan Amoroso", "111");
        Book book2 = new Book("JavaScript Basics", "Erik Trautman", "222");
        EBook ebook1 = new EBook("Programming Fundamentals", "David Malan", "333", 1.5);

        // Displaying information of the books
        Console.WriteLine("Library Books Information:");
        book1.DisplayInfo();
        book2.DisplayInfo();
        ebook1.DisplayInfo();

        // Borrowing a book
        Console.WriteLine("\nBorrowing a book...");
        book1.Borrow(); // Borrowing the first book
        book1.Borrow(); // Attempt to borrow again
        book1.Return(); // Returning the book
        book1.DisplayInfo(); // Display updated availability

        // Polymorphism demonstration
        Console.WriteLine("\nPolymorphism Demonstration:");
        Book libraryItem = new EBook("Data Structures", "Rene Decartes", "444", 2.0);
        libraryItem.DisplayInfo();
    }
}