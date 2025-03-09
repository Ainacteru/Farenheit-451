using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace Fahrenheit451API.Controllers
{
    [ApiController]
    [Route("api/respond")]
    public class FahrenheitController : ControllerBase
    {
        // Path to the folder containing the text files
        private readonly string _textFileDirectory = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).FullName, "TextFiles");
        
        private static bool limited = true;
        private static bool access = false;

        private static string status = "";
        private static int author = 0;

        private static readonly string[] books = {
            "Gulliver's Travels",
            "On the Origin of Species", 
            "The World as Will and Representation",
            "Relativity: The Special and the General Theory",
            "The Philosophy of Civilization",
            "The Clouds",
            "The Story of My Experiments with Truth",
            "Dhammapada",
            "Analects",
            "Nightmare Abbey",
            "The Declaration of Independence",
            "The Gettysburg Address",
            "Common Sense",
            "The Prince",
            "The Bible"
        };

        private static readonly string[] authors = {
            "Jonathan Swift", "Charles Darwin", "Schopenhauer", "Einstein", "Albert Schweitzer",
            "Aristophanes", "Mahatma Gandhi", "Gautama Buddha", "Confucius", "Thomas Love Peacock",
            "Thomas Jefferson", "Lincoln", "Tom Paine", "Machiavelli", "Christ"
        };

        [HttpPost]
        public IActionResult Respond([FromBody] UserInput input)
        {
            int step = HttpContext.Session.GetInt32("step") ?? 0;

            string response = ProcessInput(input.Text, ref step);

            HttpContext.Session.SetInt32("step", step);  // Store updated step

            return Ok(new { response });
        }

        private string ProcessInput(string answer, ref int step)
        {
            string input = answer.ToLower();
            if (!access) 
            {
                switch (step) 
                {
                    case 0:
                        if (input == "continue") 
                        {
                            step++;
                            return "Please enter your name:";
                        }
                        return "Access Denied.";

                    case 1:
                        if (CheckForName(input)) 
                        {
                            step++;
                            return "Please enter your assigned title:";
                        }
                        else if (input == "ari") {return "ew";}
                        
                        return "Invalid name. Try again.";

                    case 2:
                        if (CheckForBook(input)) 
                        {
                            step++;
                            access = true;
                            return "Access granted. You can access 1 book(s).   Type 'help' for a list of commands";
                        }
                        return "Incorrect title. Try again.";
                    default:
                        return "";

                }
            }
            else if (limited) {
                
                switch (input)
                {
                    case "help":
                        return GetHelp();
                    case "get permission":
                        return PermissionRiddle();
                    case "books":
                        return AvailableBooks();
                    default:
                        if (isFullPermissionGranted(input)) {
                            return "Full Access Granted";
                        }
                        return "Not a valid command. Type a 'help' for a list of commands";
                }
            }
            else {
                switch (input) {
                    case "help":
                        return GetHelp();
                    case "books":
                        return AvailableBooks();
                    default:
                        if (input.StartsWith("open ") )
                        {
                            // Remove the "open " prefix and match the remaining part with book titles
                            string bookTitle = input.Substring(5);  // Removes the "open " part
                            return OpenBook(bookTitle);
                        }
                        return "Not a valid command. Type a 'help' for a list of commands"; 
                }
            }
        }

        private string OpenBook(string input) {
            // Make sure the input is sanitized and used correctly in the file path
            string filePath = Path.Combine(_textFileDirectory, input + ".txt");

            //return $"Looking for file: {filePath}\nExists: {System.IO.File.Exists(filePath)}"; 

            if (limited && (filePath == books[author])) {
                return System.IO.File.ReadAllText(filePath);
            }
            // Check if the file exists
            else if (System.IO.File.Exists(filePath))
            {
                // Read and return the content of the file
                return System.IO.File.ReadAllText(filePath);
            }

            // If file does not exist, return null
            return "Book not found in database";
        }

        private string AvailableBooks(){
            if (limited) {
                return "You have access to 1 book(s):\n" + books[author];
            }
            return "you have access to 14 book(s):\n" + string.Join("\n", books);
        }

        private string PermissionRiddle()
        {
            status = "getting permission";
            return "I rise from the ashes, reborn again, where books are a symbol of hope, when all seems lost, I'm reborn, no matter what";
        }

        private bool isFullPermissionGranted(string input) {
            if (input == "phoenix" && status == "getting permission") {
                limited = false;
                status = "";
                return true;
            }
            status = "";
            return false;
        }

        private string GetHelp()
        {
            string additionalStuff;
            if (limited)
            {
                additionalStuff = "get permission - Request additional access\n";
            }
            else {
                additionalStuff = "";
            }

            return "Available commands:\n" + 
                        "help - Opens this menu\n" +
                        "books - Lists all books available to you\n" +
                        "open {book} - Shows you a section book saved in our database\n" +
                        additionalStuff;
        }

        private bool CheckForName(string input)
        {
            for (int i = 0; i < authors.Length; i++)
            {
                if (input == authors[i].ToLower())
                {
                    author = i;
                    return true;
                }
            }
            return false;
        }

        private bool CheckForBook(string input)
        {
            if (input == books[author].ToLower()) {
                return true;
            }
            else {
                return false;
            }
        }


        
    }
    public class UserInput
    {
        public required string Text { get; set; }
    }
}
