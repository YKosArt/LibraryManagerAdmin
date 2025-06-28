using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

class LibraryManager
{
    static Dictionary<string, bool> libraryBooks = new Dictionary<string, bool>();
    static Dictionary<string, List<string>> userBooks = new Dictionary<string, List<string>>();
    static Dictionary<string, string> userPasswords = new Dictionary<string, string>();
    const int maxLibraryCapacity = 5;
    const int maxBorrowedBooks = 3;
    const string usersFilePath = "users.json";
    const string booksFilePath = "books.json";

    // Пароль адміністратора
    static string adminPassword = "admin123";

    static void Main()
    {
        LoadUsers();
        LoadBooks();

        while (true) // Основний цикл програми
        {
            Console.WriteLine("Are you an admin or a user? (admin/user) or type 'exit' to quit:");
            string userRole = Console.ReadLine()?.Trim().ToLower();

            if (userRole == "exit")
            {
                Console.WriteLine("Exiting program. Goodbye!");
                SaveUsers();
                SaveBooks();
                return;
            }

            if (userRole == "admin")
            {
                if (!AuthenticateAdmin())
                {
                    Console.WriteLine("Authentication failed. Returning to role selection.");
                    continue;
                }

                while (true)
                {
                    Console.WriteLine("Admin menu: Would you like to add, remove, display, view stats, or return to role selection? (add/remove/display/stats/back)");
                    string adminAction = Console.ReadLine()?.Trim().ToLower();

                    switch (adminAction)
                    {
                        case "add":
                            AddBook();
                            break;
                        case "remove":
                            RemoveBook();
                            break;
                        case "display":
                            DisplayBooks();
                            break;
                        case "stats":
                            DisplayUserStats(); // Виклик нового методу
                            break;
                        case "back":
                            Console.WriteLine("Returning to role selection...");
                            return;
                        default:
                            Console.WriteLine("Invalid action. Please try again.");
                            break;
                    }
                }
            }
            else if (userRole == "user")
            {
                Console.WriteLine("Do you want to register or login? (register/login/back)");
                string action = Console.ReadLine()?.Trim().ToLower();

                if (action == "back")
                {
                    Console.WriteLine("Returning to role selection...");
                    continue;
                }

                if (action == "register")
                {
                    RegisterUser();
                }
                else if (action == "login")
                {
                    LoginUser();
                }
                else
                {
                    Console.WriteLine("Invalid action. Returning to role selection.");
                }
            }
            else
            {
                Console.WriteLine("Invalid role. Please try again.");
            }
        }
    }

    static bool AuthenticateAdmin()
    {
        Console.WriteLine("Enter admin password:");
        string inputPassword = Console.ReadLine();

        if (inputPassword == adminPassword)
        {
            Console.WriteLine("Authentication successful. Welcome, Admin!");
            return true;
        }
        else
        {
            Console.WriteLine("Incorrect password.");
            return false;
        }
    }

    static void RegisterUser()
    {
        Console.WriteLine("Enter a username to register:");
        string username = Console.ReadLine();

        if (userBooks.ContainsKey(username))
        {
            Console.WriteLine("This username is already taken. Try another one.");
        }
        else
        {
            Console.WriteLine("Enter a password for your account:");
            string password = Console.ReadLine();

            userBooks[username] = new List<string>();
            userPasswords[username] = password;

            Console.WriteLine($"User '{username}' registered successfully!");
            SaveUsers(); // Зберігаємо користувачів і паролі
        }
    }

    static void LoginUser()
    {
        Console.WriteLine("Enter your username:");
        string username = Console.ReadLine();

        if (userBooks.ContainsKey(username))
        {
            Console.WriteLine("Enter your password:");
            string password = Console.ReadLine();

            if (userPasswords.ContainsKey(username) && userPasswords[username] == password)
            {
                Console.WriteLine($"Welcome back, {username}!");
                UserMenu(username);
            }
            else
            {
                Console.WriteLine("Incorrect password. Please try again.");
            }
        }
        else
        {
            Console.WriteLine("User not found. Please register first.");
        }
    }

    static void UserMenu(string username)
    {
        while (true)
        {
            Console.WriteLine("Would you like to search, borrow, return, display, go back to role selection, or exit? (search/borrow/return/display/back/exit)");
            string userAction = Console.ReadLine()?.Trim().ToLower();

            switch (userAction)
            {
                case "search":
                    SearchBook();
                    break;
                case "borrow":
                    BorrowBook(username);
                    break;
                case "return":
                    ReturnBook(username);
                    break;
                case "display":
                    DisplayBooks();
                    break;
                case "back":
                    Console.WriteLine("Returning to role selection...");
                    return; // Повернення до вибору ролі
                case "exit":
                    Console.WriteLine("Exiting program. Goodbye!");
                    SaveUsers();
                    SaveBooks();
                    Environment.Exit(0); // Завершення програми
                    break;
                default:
                    Console.WriteLine("Invalid action. Please try again.");
                    break;
            }
        }
    }

    static void AddBook()
    {
        if (libraryBooks.Count >= maxLibraryCapacity)
        {
            Console.WriteLine("The library is full. No more books can be added.");
            return;
        }

        Console.WriteLine("Enter the title of the book to add:");
        string bookTitleToAdd = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(bookTitleToAdd))
        {
            Console.WriteLine("Book title cannot be empty.");
            return;
        }

        if (libraryBooks.ContainsKey(bookTitleToAdd))
        {
            Console.WriteLine($"The book '{bookTitleToAdd}' already exists in the library.");
            return;
        }

        libraryBooks.Add(bookTitleToAdd, false); // Додаємо книгу як доступну
        Console.WriteLine($"Book '{bookTitleToAdd}' added to the library.");
        SaveBooks(); // Зберігаємо книги
    }

    static void RemoveBook()
    {
        if (libraryBooks.Count == 0)
        {
            Console.WriteLine("The library is empty. No books to remove.");
            return;
        }

        Console.WriteLine("Enter the title of the book to remove:");
        string bookTitleToRemove = Console.ReadLine();

        if (libraryBooks.ContainsKey(bookTitleToRemove))
        {
            if (libraryBooks[bookTitleToRemove])
            {
                Console.WriteLine($"The book '{bookTitleToRemove}' is currently borrowed and cannot be removed.");
            }
            else
            {
                libraryBooks.Remove(bookTitleToRemove);
                Console.WriteLine($"Book '{bookTitleToRemove}' removed from the library.");
                SaveBooks(); // Зберігаємо книги
            }
        }
        else
        {
            Console.WriteLine("Book not found.");
        }
    }

    static void SearchBook()
    {
        Console.WriteLine("Enter the title of the book to search:");
        string bookTitleToSearch = Console.ReadLine();
        if (libraryBooks.ContainsKey(bookTitleToSearch))
        {
            bool isBorrowed = libraryBooks[bookTitleToSearch];
            if (isBorrowed)
            {
                Console.WriteLine($"Book '{bookTitleToSearch}' is currently borrowed.");
            }
            else
            {
                Console.WriteLine($"Book '{bookTitleToSearch}' is available in the library.");
            }
        }
        else
        {
            Console.WriteLine($"Book '{bookTitleToSearch}' is not in the library.");
        }
    }

    static void BorrowBook(string username)
    {
        if (libraryBooks.Count == 0)
        {
            Console.WriteLine("The library is empty. No books to borrow.");
            return;
        }

        if (userBooks[username].Count >= maxBorrowedBooks)
        {
            Console.WriteLine("You have reached the maximum number of borrowed books.");
            return;
        }

        Console.WriteLine("Enter the title of the book to borrow:");
        string bookTitleToBorrow = Console.ReadLine();

        if (libraryBooks.ContainsKey(bookTitleToBorrow))
        {
            if (libraryBooks[bookTitleToBorrow])
            {
                Console.WriteLine($"The book '{bookTitleToBorrow}' is already borrowed.");
            }
            else
            {
                libraryBooks[bookTitleToBorrow] = true; // Позначаємо книгу як позичену
                userBooks[username].Add(bookTitleToBorrow);
                Console.WriteLine($"Book '{bookTitleToBorrow}' borrowed successfully.");
            }
        }
        else
        {
            Console.WriteLine("Book not found in the library.");
        }
    }

    static void ReturnBook(string username)
    {
        Console.WriteLine("Enter the title of the book to return:");
        string bookTitleToReturn = Console.ReadLine();

        if (userBooks[username].Contains(bookTitleToReturn))
        {
            if (libraryBooks.ContainsKey(bookTitleToReturn) && libraryBooks[bookTitleToReturn])
            {
                libraryBooks[bookTitleToReturn] = false; // Позначаємо книгу як доступну
                userBooks[username].Remove(bookTitleToReturn);
                Console.WriteLine($"Book '{bookTitleToReturn}' returned successfully.");
            }
            else
            {
                Console.WriteLine("This book is not currently borrowed.");
            }
        }
        else
        {
            Console.WriteLine("You have not borrowed this book.");
        }
    }

    static void DisplayBooks()
    {
        Console.WriteLine("Library books:");
        if (libraryBooks.Count == 0)
        {
            Console.WriteLine("No books in the library.");
        }
        else
        {
            foreach (var book in libraryBooks)
            {
                string status = book.Value ? "[Borrowed]" : "[Available]";
                Console.WriteLine($"{book.Key} {status}");
            }
        }
    }

    static void DisplayUserStats()
    {
        Console.WriteLine($"Total registered users: {userBooks.Count}");

        Console.WriteLine("Borrowed books and their borrowers:");
        bool hasBorrowedBooks = false;

        foreach (var user in userBooks)
        {
            foreach (var book in user.Value)
            {
                Console.WriteLine($"Book: '{book}' is borrowed by User: '{user.Key}'");
                hasBorrowedBooks = true;
            }
        }

        if (!hasBorrowedBooks)
        {
            Console.WriteLine("No books are currently borrowed.");
        }
    }

    static void LoadUsers()
    {
        if (File.Exists(usersFilePath))
        {
            string json = File.ReadAllText(usersFilePath);
            var data = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json) ?? new Dictionary<string, List<string>>();
            userBooks = data;

            if (File.Exists("passwords.json"))
            {
                string passwordJson = File.ReadAllText("passwords.json");
                userPasswords = JsonSerializer.Deserialize<Dictionary<string, string>>(passwordJson) ?? new Dictionary<string, string>();
            }
        }
    }

    static void SaveUsers()
    {
        string json = JsonSerializer.Serialize(userBooks, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(usersFilePath, json);

        string passwordJson = JsonSerializer.Serialize(userPasswords, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText("passwords.json", passwordJson);
    }

    static void LoadBooks()
    {
        if (File.Exists(booksFilePath))
        {
            string json = File.ReadAllText(booksFilePath);
            libraryBooks = JsonSerializer.Deserialize<Dictionary<string, bool>>(json) ?? new Dictionary<string, bool>();
        }
    }

    static void SaveBooks()
    {
        string json = JsonSerializer.Serialize(libraryBooks, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(booksFilePath, json);
    }
}