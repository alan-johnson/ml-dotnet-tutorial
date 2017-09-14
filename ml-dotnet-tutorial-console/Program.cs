using System;
using System.Configuration;
using System.Threading.Tasks;
using System.Xml;

// add in MarkLogic C# api
using MarkLogic.REST;

namespace mldotnettutorialconsole
{
    class MainClass
    {
		public static void Main(string[] args)
        {
            Console.WriteLine("MarkLogic C# API Demo.");
            Console.WriteLine("Begin Read, Write and Search Tests.");

			/* Read the MarkLogic property settings from 
			 *  the app.config file.
			 * host - Name or IP address of MarkLogic Server
			 * port - port number of the REST Instance Application Server
			 * username - user with at least rest-writer role
			 * password - password for the user
			 * realm - by default, MarkLogic uses the string "public"
			 */
            var host = ConfigurationManager.AppSettings["host"];
			var port = ConfigurationManager.AppSettings["port"];
			var username = ConfigurationManager.AppSettings["username"];
			var password = ConfigurationManager.AppSettings["password"];
			var realm = ConfigurationManager.AppSettings["realm"];

			// Create a DatabaseClient object. These objects represent
			//  long-lived connections to MarkLogic databases.
			DatabaseClient dbClient = DatabaseClientFactory.NewClient(host, port, username, password, realm, AuthType.Digest);

            var cmd = string.Empty;
            do
            {
                PrintMenu();
                cmd = Console.ReadLine();
                cmd = cmd.ToLower();

                switch (cmd)
                {
                    case "1":
                        WriteToDatabase(dbClient);
                        break;
                    case "2":
                        ReadFromDatabase(dbClient);
                        break;
                    case "3":
                        SearchDatabase(dbClient);
                        break;
                    case "4":
                        PrintHelp();
                        break;
                    default:
                        break;
                }

            } while (cmd != "exit");

            /*
            // Test reading an XML document synchronously
            XmlDocument doc = ReadXMLTest(dbClient);
            Console.WriteLine("Document read.");
            Console.WriteLine(doc.InnerXml);

            // Test writing the XML document to 
            //  a new URI (MarkLogic Insert operation)
            WriteXMLTest(dbClient, "/doc1b.xml", doc);
            Console.WriteLine("document /doc1b.xml written to database.");

            var results = SearchTest(dbClient, "dogs");
			Console.WriteLine("Search results follows.");
            Console.WriteLine(results);
            */

			// Uncomment to read document asynchronously
			/*
            var task = TestAsync();
			task.Wait();
            */

			// Tell the DatabaseClient we are done with it
			//  and release any connections and resources
			dbClient.Release();
		}

		static void ReadFromDatabase(DatabaseClient dbClient)
        {
            Console.Write("Document URI: ");
			var uri = Console.ReadLine();

			// Create a DocumentManager to act as our interface for
			//  reading and writing to/from the database.
			DocumentManager mgr = dbClient.NewDocumentManager();

            // Use the DocumentManager object to read a document 
            // with the uri of "/doc1.xml" from the "Documents" database.
            string mimetype = string.Empty;
            string content = string.Empty;

            GenericDocument doc = mgr.Read(uri);
            mimetype = doc.GetMimetype();
            content = doc.GetContent();

			Console.WriteLine(" ");
			Console.WriteLine("---Results----------");
            Console.Write("Mime type: ");
            Console.WriteLine(mimetype);
            Console.Write("Content: ");
            Console.WriteLine(content);

		}

		static void WriteToDatabase(DatabaseClient dbClient)
		{
            Console.Write("Document path and filename to store: ");
			string filename = Console.ReadLine();
			if (filename.Length == 0)
			{
				Console.WriteLine(("No file specified."));
				return;
			}
			string content = System.IO.File.ReadAllText(filename);

            Console.Write("Document URI: ");
			var uri = Console.ReadLine();
            if (uri.Length == 0)
            {
                Console.WriteLine(("You must supply a document URI to save a document to the MarkLogic database."));
                return;
            }

			// Create a DocumentManager to act as our interface for
			//  reading and writing to/from the database.
			DocumentManager mgr = dbClient.NewDocumentManager();

            // Use the DocumentManager object to read a document 
            // with the uri of "/doc1.xml" from the "Documents" database.
            GenericDocument doc = new GenericDocument();
            doc.SetMimetype(string.Empty);
			doc.SetContent(content);

			var results = mgr.Write(uri, doc);

			Console.WriteLine(" ");
			Console.WriteLine("--------------------------------");
			Console.Write("Write results: ");
			Console.WriteLine(results);
		}

		static void SearchDatabase(DatabaseClient dbClient)
		{
			Console.Write("Enter a search term: ");
			var query = Console.ReadLine();
			if (query.Length == 0)
			{
				Console.WriteLine(("You must enter a search term to search the MarkLogic database."));
				return;
			}

			// Create a QueryManager to act as our interface for
			//  searching the database.
			QueryManager mgr = dbClient.NewQueryManager();

			// Search results are retuned in a SearchResult object.
            // 
			SearchResult searchResult = mgr.Search(query);
			
            Console.WriteLine(" ");
            Console.WriteLine("--------------------------------");
			Console.Write("Search total: ");
			Console.WriteLine(searchResult.GetTotalResults()); // total results found
			Console.Write("Search results per page: ");
			Console.WriteLine(searchResult.GetPageLength()); // number of results per page
			Console.Write("Starting at item: ");
			Console.WriteLine(searchResult.GetStart()); // starting result number
			Console.WriteLine("Search Results:");

            // Each search result is returned in a MatchDocSummary object.
            //  GetMatchResults() returns a list of these, if any.
            foreach (MatchDocSummary result in searchResult.GetMatchResults())
            {
				Console.WriteLine(" ");
				Console.WriteLine("---Result " + result.GetIndex() + "---------");

                Console.Write("URI: ");
                Console.WriteLine(result.GetUri());

				Console.Write("Relevance Score: ");
				Console.WriteLine(result.GetScore());

				Console.Write("Mimetype: ");
                Console.WriteLine(result.GetMimetype());

				Console.Write("Text: ");
                Console.WriteLine(result.GetFirstSnippetText());

            }

			// return all search results as a string
			// string results = searchResult.ToString();
        
        }

        static XmlDocument ReadXMLTest(DatabaseClient dbClient)
		{
		    // Create a DocumentManager to act as our interface for
            //  reading and writing to/from the database.
            XmlDocumentManager mgr = dbClient.NewXmlDocumentManager();

			// Use the DocumentManager object to read a document 
            // with the uri of "/doc1.xml" from the "Documents" database.

            XmlDocument content = mgr.Read("/doc1.xml");

            return content;
		}

		static void WriteXMLTest(DatabaseClient dbClient, string uri, XmlDocument content)
		{
			// Create a DocumentManager to act as our interface for
			//  reading and writing to/from the database.
			XmlDocumentManager mgr = dbClient.NewXmlDocumentManager();

			// Use the DocumentManager object to Write the 
			// document back to the database with a new uri.
			mgr.Write(uri, content);

            return;
		}

		static string SearchTest(DatabaseClient dbClient, string query)
		{
			// Create a QueryManager to act as our interface for
			//  searching the database.
			QueryManager mgr = dbClient.NewQueryManager();

            // Use the DocumentManager object to Write the 
            // document back to the database with a new uri.
            string results = System.String.Empty;

			//results = mgr.SearchRaw(query, 1, 10, "xml");
            SearchResult result = mgr.Search(query);
            results = result.ToString();

			return results;
		}

		static async Task TestAsync()
		{
            var host = ConfigurationManager.AppSettings["host"];
            var port = ConfigurationManager.AppSettings["port"];
			var username = ConfigurationManager.AppSettings["username"];
			var password = ConfigurationManager.AppSettings["password"];
			var realm = ConfigurationManager.AppSettings["realm"];

            DatabaseClient dbClient = DatabaseClientFactory.NewClient(host, port, username, password, realm, AuthType.Digest);
			
            DocumentManager mgr = dbClient.NewDocumentManager();

			var r = await mgr.ReadAsync("/doc1.xml");

			Console.WriteLine(r);
			
            dbClient.Release();
		}
		
        static void PrintMenu()
        {
			Console.WriteLine(" ");
			Console.WriteLine("|-----------------------------------------------------|");
            Console.WriteLine("| Select from the following options then press ENTER. |");
            Console.WriteLine("|-----------------------------------------------------|");
            Console.WriteLine("  1. Load document.");
            Console.WriteLine("  2. Read document.");
            Console.WriteLine("  3. Search a term.");
			Console.WriteLine("  4. Help.");
			Console.WriteLine("  Type 'exit' to quit.");
			Console.Write("==> ");
		}

		static void PrintHelp()
		{
			Console.WriteLine(" ");
            Console.WriteLine("  1. Load - Type 1 then ENTER to load an XML, JSON or Text file to the MarkLogic Documents database.");
			Console.WriteLine("  2. Read - Type 2 then ENTER to read an XML, JSON or Text file from the MarkLogic Documents database.");
			Console.WriteLine("  3. Enter a search term then press ENTER to search the MarkLogic Documents database. Results are returned as text snippets followed by the entire MarkLogic search response.");
		}
    }
}
