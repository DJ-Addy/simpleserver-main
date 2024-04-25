using System.Text.Json;

static void TestJSON() {
    var options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };
    string text = File.ReadAllText("config.json");
    //write a line that deserializes the text from a config.json file into a <Config>(text, options) variable:
    var config = JsonSerializer.Deserialize<Config>(text, options);

    Console.WriteLine($"MimeTypes: {config.MimeTypes["html"]}");
    Console.WriteLine($"indexFiles: {config.indexFiles[0]}");
}
static void TestJSON2() {
    var options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    string text = File.ReadAllText(@"json/books.json");
    var books = JsonSerializer.Deserialize<List<Book>>(text, options);

    Book book = books[4];
    Console.WriteLine($"title: {book.Title}");
    Console.WriteLine($"authors: {book.Authors[0]}");
}

static void TestServer() {
    SimpleHTTPServer server = new SimpleHTTPServer("files", 8080, "config.json");
    string helpMessage = @"Server started. You can try the following commands:
    help- show this help
    stop - stop the server
    numreqs - show the number of requests
    paths - show the paths for each request
    #404 - show the number of 404 requests
";

     Console.WriteLine(helpMessage);

    while (true)
    {
         Console.Write("> ");
        Console.WriteLine(@"Server started. You can try the following commands:
stop - stop the server
");
        // read line from console;
        String command = Console.ReadLine();
        if (command.Equals("stop"))
        {
            server.Stop();
            break;
        } 
        else if (command.Equals("help"))
        {
            Console.WriteLine(helpMessage);
        } 
        else if (command.Equals("numreqs"))
        {
            Console.WriteLine($"Number of requests: {server.NumRequests}");
        }
         else if (command.Equals("paths"))
        {
            foreach (var pathtraveled in server.PathsRequested)
            {
            
                Console.WriteLine($"{pathtraveled.Key}: {pathtraveled.Value}");
            }
        }else if(command.Equals("#404"))
        {
            Console.WriteLine($"Number of 404 requests: {server.Num404Requests}");
        }
        else
        {
            Console.WriteLine($"Unknown command: {command}");
        }
    }
}

//TestJSON();
TestServer();
