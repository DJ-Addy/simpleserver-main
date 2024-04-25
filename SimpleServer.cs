// SimpleServer based on code by Can Güney Aksakalli
// MIT License - Copyright (c) 2016 Can Güney Aksakalli
// https://aksakalli.github.io/2014/02/24/simple-http-server-with-csparp.html
// modifications by Jaime Spacco

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Web;
using System.Text.Json;
using System.ComponentModel.DataAnnotations;


/// <summary>
/// Interface for simple servlets.
/// 
/// </summary>
interface IServlet {
    void ProcessRequest(HttpListenerContext context);
}
/// <summary>
/// BookHandler: Servlet that reads a JSON file and returns a random book
/// as an HTML table with one row.
/// TODO: search for specific books by author or title or whatever
/// </summary>


class BookHandler : IServlet {
    private List<Book> books;
    private void SendHtmlResponse(HttpListenerContext context, string htmlContent)
    {
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(htmlContent);
        context.Response.ContentType = "text/html";
        context.Response.ContentLength64 = bytes.Length;
        context.Response.AddHeader("Date", DateTime.Now.ToString("r"));
        context.Response.AddHeader("Last-Modified", DateTime.Now.ToString("r"));
        context.Response.StatusCode = (int)HttpStatusCode.OK;
        context.Response.OutputStream.Write(bytes, 0, bytes.Length);
        context.Response.OutputStream.Flush();
    }
    public BookHandler()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        string text = File.ReadAllText(@"json/books.json");
        books = JsonSerializer.Deserialize<List<Book>>(text, options);
    }


    public void ProcessRequest(HttpListenerContext context) {
        // we want to use case-insensitive matching for the JSON properties
        // the json files use lowercae letters, but we want to use uppercase in our C# code

        if(!context.Request.QueryString.AllKeys.Contains("cmd"))
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return;
        }
        string cmd = context.Request.QueryString["cmd"];
        if(cmd.Equals("list"))
        {
            //list books s to e from the JSON file
            //s and e are query parameters
            int start= Int32.Parse(context.Request.QueryString["s"]);
            int end= Int32.Parse(context.Request.QueryString["e"]);
            List<Book> sublist = books.GetRange(start, end-start+1);
            if(start<0 || end<0 || start>end || end>books.Count)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return;
            }
            string response = $@" 
        <H1>Book Servlet</H1>
        <h2>Search for Books</h2>
        <form method=""get"" action=""books"">
            <input type=""number"" name=""s"" placeholder=""Start"" />
            <input type=""number"" name=""e"" placeholder=""End"" />
            <input type=""cmd"" value=""list"" />
            <input type=""submit"" value=""Search"" />
        </form>
        <table border=1>
        <tr>
            <th>Title</th>
            <th>Author</th>
            <th>Short Description</th>
            <th>Thumbnail</th>
        </tr>
    ";
        foreach(Book booked in sublist){
            string authors = string.Join(",<br> ", booked.Authors);
            response+= $@"
        <tr>
            <td>{booked.Title}</td>
            <td>{authors}</td>
            <td>{booked.ShortDescription}</td>
           <td><img src='{booked.ThumbnailUrl}'/></td>
        </tr>
        ";
        }
        response+="</table>";
        SendHtmlResponse(context, response);
        
        }else if(cmd.Equals("random"))
        {    
           //return a random book from the JSON file//
           Random random = new Random();
            int index = random.Next(books.Count);
             Book booku = books[index];
             string authory = string.Join(",<br> ", booku.Authors);
             string randoms = $@"
          <table border=1>
         <tr>
            <th>Title</th>
           <th>Author</th>
        <th>Short Description</th>
           <th>Thumbnail</th>

         </tr>

         <tr>

    <td>{booku.Title}</td>
    <td>{authory}</td>
     <td>{booku.ShortDescription}</td>
   <td><img src='{booku.ThumbnailUrl}'/></td>
    </tr>
     </table>
    ";
        SendHtmlResponse(context, randoms);
      }else{
            string response = $@"
            <H1>ERROR 404.</H1>
            <h2>you stand in a barrier between worlds my fellow internet person</h2>
            <h3>I am but a humble Java programmer </h3>";
             SendHtmlResponse(context, response);

            }
      }
        
    
    }

class HomeHandler : IServlet {
    private Dictionary<string, string> _servletMappings;

    public HomeHandler(Dictionary<string, string> servletMappings) {
        _servletMappings = servletMappings;
    }

    private void SendHtmlResponse(HttpListenerContext context, string htmlContent) {
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(htmlContent);
        context.Response.ContentType = "text/html";
        context.Response.ContentLength64 = bytes.Length;
        context.Response.AddHeader("Date", DateTime.Now.ToString("r"));
        context.Response.AddHeader("Last-Modified", DateTime.Now.ToString("r"));
        context.Response.StatusCode = (int)HttpStatusCode.OK;
        context.Response.OutputStream.Write(bytes, 0, bytes.Length);
        context.Response.OutputStream.Flush();
    }

    public void ProcessRequest(HttpListenerContext context) {
        string response = $@"<h1>Welcome to the Home Page</h1>";

        // Generate redirect buttons for each servlet mapping
        response += $@"<ul>
        <H1>Welcome to the Home Page</H1>
    <ul>
        <li><a href=""foo"">Foo Servlet</a></li>
        <li><a href=""books"">Book Servlet</a></li>
        <li>
            Filter Servlet
            <form method=""get"" action=""filters"">
                <input type=""text"" name=""filter"" placeholder=""Search..."" />
                <input type=""number"" name=""s"" placeholder=""Start"" />
                <input type=""number"" name=""e"" placeholder=""End"" />
                <input type=""submit"" value=""Search"" />
            </form>
        </li>
    </ul>";
        response += "</ul>";

        SendHtmlResponse(context, response);
    }
}
class BookFilterServlet : IServlet {
    private List<Book> books;

    public BookFilterServlet() {
        var options = new JsonSerializerOptions {
            PropertyNameCaseInsensitive = true
        };

        string text = File.ReadAllText(@"json/books.json");
        books = JsonSerializer.Deserialize<List<Book>>(text, options);
    }

    public void ProcessRequest(HttpListenerContext context) {
        if (!context.Request.QueryString.AllKeys.Contains("filter")) {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return;
        }

        string filter = context.Request.QueryString["filter"].ToLower();
         
            // search for books that match the filter
            //has a search bar that filters books by author or title
            string response = $@"
            <form method='get'>
            <input type='text' name='filter' value='{filter}' placeholder='Search books...'>
            <input type='submit' value='Search'>
            </form>
            <table border=1>
            <tr>
            <th>Title</th>
            <th>Author</th>
            <th>Short Description</th>
            <th>Thumbnail</th>
            </tr>
        ";
        foreach(Book book in books){
                 string authors = string.Join(",<br> ", book.Authors);
            if(authors.ToLower().Contains(filter)||book.Title.ToLower().Contains(filter))
            {
            Console.WriteLine("found a book with the author");

            response+= $@"
            <tr>
            <td>{book.Title}</td>
            <td>{authors}</td>
            <td>{book.ShortDescription}</td>
           <td><img src='{book.ThumbnailUrl}'/></td>
        </tr>
        ";


            }


    }
      response+="</table>";
        SendHtmlResponse(context, response);

    }


    

    //write a function that takes a string and returns a list of books that match the filter
   
    private void SendHtmlResponse(HttpListenerContext context, string htmlContent) {
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(htmlContent);
        context.Response.ContentType = "text/html";
        context.Response.ContentLength64 = bytes.Length;
        context.Response.AddHeader("Date", DateTime.Now.ToString("r"));
        context.Response.AddHeader("Last-Modified", DateTime.Now.ToString("r"));
        context.Response.StatusCode = (int)HttpStatusCode.OK;
        context.Response.OutputStream.Write(bytes, 0, bytes.Length);
        context.Response.OutputStream.Flush();
    }

}
/// <summary>
/// FooHandler: Servlet that returns a simple HTML page.
/// </summary>
class FooHandler : IServlet {
    private void SendHtmlResponse(HttpListenerContext context, string htmlContent)
    {
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(htmlContent);
        context.Response.ContentType = "text/html";
        context.Response.ContentLength64 = bytes.Length;
        context.Response.AddHeader("Date", DateTime.Now.ToString("r"));
        context.Response.AddHeader("Last-Modified", DateTime.Now.ToString("r"));
        context.Response.StatusCode = (int)HttpStatusCode.OK;
        context.Response.OutputStream.Write(bytes, 0, bytes.Length);
        context.Response.OutputStream.Flush();
    }

    public void ProcessRequest(HttpListenerContext context) {
        string response = $@"
            <H1>This is a Servlet Test.</H1>
            <h2>Servlets are a Java thing; there is probably a .NET equivlanet but I don't know it</h2>
            <h3>I am but a humble Java programmer who wrote some Servlets in the 2000s</h3>
            <p>Request path: {context.Request.Url.AbsolutePath}</p>
";
        foreach ( String s in context.Request.QueryString.AllKeys )
            response += $"<p>{s} -> {context.Request.QueryString[s]}</p>\n";

        SendHtmlResponse(context, response);
    }
}


class SimpleHTTPServer
{
    // bind servlets to a path
    // for example, this means that /foo will be handled by an instance of FooHandler
    // TODO: put these mappings into a configuration file
    private static IDictionary<string, IServlet> _servlets = new Dictionary<string, IServlet>() {
        {"home", new HomeHandler(new Dictionary<string, string> {
        {"foo", "Foo Servlet"},
        {"books", "Book Servlet"},
        {"filters", "Filter Servlet"}
    })},{"foo", new FooHandler()},
        {"books", new BookHandler()},
        {"filters", new BookFilterServlet()},
    };

    // list of default index files
    // if the client requests a directory (e.g. http://localhost:8080/), 
    // we will look for one of these files
    private string[] _indexFiles;
    
    // map extensions to MIME types
    // TODO: put this into a configuration file
    private IDictionary<string, string> _mimeTypeMappings;

    // instance variables
    private Thread _serverThread;
    private string _rootDirectory;
    private HttpListener _listener;
    private int _port;
    private int _numRequests = 0;
    private bool _done = false;
    private Dictionary<string, int> pathRequested = new Dictionary<string, int>();
    private int _404RequestCount = 0;

    public int Port
    {
        get { return _port; }
        private set { }
    }

public int NumRequests
    {
        get { return _numRequests; }
        private set {_numRequests = value; }
    }

public Dictionary<string, int> PathsRequested
    {
        get { return pathRequested; }
        private set { }
    }

    public int Num404Requests
    {
        get { return _404RequestCount; }
        private set {_404RequestCount = value; }
    }
    /// <summary>
    /// Construct server with given port.
    /// </summary>
    /// <param name="path">Directory path to serve.</param>
    /// <param name="port">Port of the server.</param>
    public SimpleHTTPServer(string path, int port, string configFileName)
    {
        this.Initialize(path, port,configFileName);
    }

    /// <summary>
    /// Construct server with any open port.
    /// </summary>
    /// <param name="path">Directory path to serve.</param>
    public SimpleHTTPServer(string path, string configFileName)
    {
        //get an empty port
        TcpListener l = new TcpListener(IPAddress.Loopback, 0);
        l.Start();
        int port = ((IPEndPoint)l.LocalEndpoint).Port;
        l.Stop();
        this.Initialize(path, port, configFileName);
    }

    /// <summary>
    /// Stop server and dispose all functions.
    /// </summary>
    public void Stop()
    {
        _done = true;
        _listener.Close();
    }

    private void Listen()
    {
        _listener = new HttpListener();
        _listener.Prefixes.Add("http://*:" + _port.ToString() + "/");
        _listener.Start();
        while (!_done)
        {
            Console.WriteLine("Waiting for connection...");
            try
            {
                HttpListenerContext context = _listener.GetContext();
                NumRequests++;
                Process(context);
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        Console.WriteLine("Server stopped!");
    }

    /// <summary>
    /// Process an incoming HTTP request with the given context.
    /// </summary>
    /// <param name="context"></param>
    private void Process(HttpListenerContext context)
    {
        string filename = context.Request.Url.AbsolutePath;
        //keep track of how many times a path is requested
        //include the leading slash in the path
        
        pathRequested[filename] = pathRequested.GetValueOrDefault(filename, 0) + 1;
        filename = filename.Substring(1);
        Console.WriteLine($"{filename} is the path");

        // check if the path is mapped to a servlet
        if (_servlets.ContainsKey(filename))
        {
            _servlets[filename].ProcessRequest(context);
            return;
        }

        // if the path is empty (i.e. http://blah:8080/ which yields hte path /)
        // look for a default index filename
        if (string.IsNullOrEmpty(filename))
        {
            foreach (string indexFile in _indexFiles)
            {
                if (File.Exists(Path.Combine(_rootDirectory, indexFile)))
                {
                    filename = indexFile;
                    break;
                }
            }
        }

        // search for the file in the root directory
        // this means we are serving the file, if we can find it
        filename = Path.Combine(_rootDirectory, filename);

        if (File.Exists(filename))
        {
            try
            {
                Stream input = new FileStream(filename, FileMode.Open);
                
                //Adding permanent http response headers
                string mime;
                context.Response.ContentType = _mimeTypeMappings.TryGetValue(Path.GetExtension(filename), out mime) ? mime : "application/octet-stream";
                context.Response.ContentLength64 = input.Length;
                context.Response.AddHeader("Date", DateTime.Now.ToString("r"));
                context.Response.AddHeader("Last-Modified", System.IO.File.GetLastWriteTime(filename).ToString("r"));

                byte[] buffer = new byte[1024 * 16];
                int nbytes;
                while ((nbytes = input.Read(buffer, 0, buffer.Length)) > 0)
                    context.Response.OutputStream.Write(buffer, 0, nbytes);
                input.Close();
                
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                context.Response.OutputStream.Flush();
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }

        }
        else
        {
            string custom404Response = @"
        <html>
        <head><title>ERROR 404- Not Found</title></head>
        <body>
            <h1>ERROR 404- Not Found</h1>
            <h3>you stand in a barrier between worlds my fellow internet person</h3>
            <h3>I am but a humble C# programmer </h3>
            <p>The requested page could not be found.</p>
        </body>
        </html>";
            Num404Requests++;
        SendHtmlResponse(context, custom404Response);

        }
        
        context.Response.OutputStream.Close();
    }

     private void SendHtmlResponse(HttpListenerContext context, string htmlContent)
    {
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(htmlContent);
        context.Response.ContentType = "text/html";
        context.Response.ContentLength64 = bytes.Length;
        context.Response.AddHeader("Date", DateTime.Now.ToString("r"));
        context.Response.AddHeader("Last-Modified", DateTime.Now.ToString("r"));
        context.Response.StatusCode = (int)HttpStatusCode.OK;
        context.Response.OutputStream.Write(bytes, 0, bytes.Length);
        context.Response.OutputStream.Flush();
    }

    /// <summary>
    /// Initializes the server by setting up a listener thread on the given port
    /// </summary>
    /// <param name="path">the path of the root directory to serve files</param>
    /// <param name="port">the port to listen for connections</param>
    /// <param name="configFileName">the name of the configuration file</param>
    private void Initialize(string path, int port, string configFileName)
    {
        this._rootDirectory = path;
        this._port = port;
    //read the config file
     var options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };
    string text = File.ReadAllText("config.json");
    //write a line that deserializes the text from a config.json file into a <Config>(text, options) variable:
    var config = JsonSerializer.Deserialize<Config>(text, options);
    _mimeTypeMappings=config.MimeTypes;
    _indexFiles=config.indexFiles.ToArray();

        _serverThread = new Thread(this.Listen);
        _serverThread.Start();
    }


}
