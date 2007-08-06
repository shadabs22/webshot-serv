using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Specialized;
using System.Web;
using System.Drawing;
using System.Drawing.Imaging;
using GetSiteThumbnail;

namespace T.Serv
{
    class HttpProcessor
    {
        private static int threads = 0;
        private Socket s;
        private NetworkStream ns;
        private StreamReader sr;
        private StreamWriter sw;
        private string method;
        private string url;
        private string protocol;
        private Hashtable headers;
        private string request;
        private bool keepAlive = false;
        private int numRequests = 0;
        private bool verbose = HttpServer.verbose;
        private byte[] bytes = new byte[4096];
        private FileInfo docRootFile;

        private static ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }
            return null;
        }

        /**
         * Each HTTP processor object handles one client.  If Keep-Alive is enabled then this
         * object will be reused for subsequent requests until the client breaks keep-alive.
         * This usually happens when it times out.  Because this could easily lead to a DoS
         * attack, we keep track of the number of open processors and only allow 100 to be
         * persistent active at any one time.  Additionally, we do not allow more than 500
         * outstanding requests.
         */

        public HttpProcessor(string docRoot, Socket s)
        {
            this.s = s;
            docRootFile = new FileInfo(docRoot);
            headers = new Hashtable();
        }

        /**
         * This is the main method of each thread of HTTP processing.  We pass this method
         * to the thread constructor when starting a new connection.
         */
        public void process()
        {
            try
            {
                try
                {

                    // Increment the number of current connections
                    Interlocked.Increment(ref threads);
                    // Bundle up our sockets nice and tight in various streams
                    ns = new NetworkStream(s, FileAccess.ReadWrite);
                    // It looks like these streams buffer
                    sr = new StreamReader(ns);
                    sw = new StreamWriter(ns);
                    // Parse the request, if that succeeds, read the headers, if that
                    // succeeds, then write the given URL to the stream, if possible.
                    while (parseRequest())
                    {
                        if (readHeaders())
                        {
                            // This makes sure we don't have too many persistent connections and also
                            // checks to see if the client can maintain keep-alive, if so then we will
                            // keep this http processor around to process again.
                            if (threads <= 100 && "Keep-Alive".Equals(headers["Connection"])) keepAlive = true;

                            NameValueCollection QueryString = HttpUtility.ParseQueryString(url.Replace("/?", ""));

                            if (!QueryString.HasKeys())
                            {
                                writeFailure();
                                return;
                            };

                            Console.WriteLine("[{0}] GET: {2}, IP: {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), s.RemoteEndPoint, QueryString.ToString());

                            int w = 240; //h = output height 
                            int h = 180; //w = output width 
                            long q = 90L; //q = quality level (30-70%) 
                            int val;

                            if (int.TryParse(QueryString["w"], out val)) w = val;
                            if (int.TryParse(QueryString["h"], out val)) h = val;
                            //if (long.TryParse(QueryString["q"], out val)) q = val;

                            string wUrl = "http://" + QueryString["url"];
                            string fileName = "./web-shots/" + QueryString["url"].Replace("/", "_").Replace("\\", "_") + "_" + w.ToString() + "x" + h.ToString() + ".png";

                            FileInfo file = new FileInfo(fileName);
                            TimeSpan span = DateTime.Now - file.CreationTime;

                            if (!file.Exists || span.TotalDays > 7)
                            {
                                WebPageBitmap webBitmap = new WebPageBitmap(wUrl, w, h, false);

                                if (webBitmap.isReady)
                                {
                                    Directory.CreateDirectory(Path.GetDirectoryName(fileName));
                                    webBitmap.SaveThumbnail(fileName, q);

                                    Console.WriteLine("[{0}] Saved: {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), fileName);
                                }
                                else
                                {
                                    fileName = "./window.png";
                                }
                            }

                            //flush jpeg to user
                            writeFile(fileName);

                            // If keep alive is not active then we want to close down the streams
                            // and shutdown the socket
                            if (!keepAlive)
                            {
                                ns.Close();
                                s.Shutdown(SocketShutdown.Both);
                                break;
                            }
                        }
                    }
               }
               catch (Exception e)
               {
                  Console.WriteLine("[{0}] Error: {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), e.Message + e.StackTrace);
               }
            }
            finally
            {
                // Always decrement the number of connections
                Interlocked.Decrement(ref threads);
            }
        }

        public bool parseRequest()
        {
            // The number of requests handled by this persistent connection
            numRequests++;
            // Here is where we ensure that we are not overloaded
            if (threads > 500)
            {
                writeError(502, "Server temporarily overloaded");
                return false;
            }
            // FIXME: This could conceivably used to DoS us if we never finish reading the
            // line and they never hang up.  We could set the socket options to limit
            // the amount of time before reading a request.
            try
            {
                request = null;
                request = sr.ReadLine();
            }
            catch (IOException)
            {
            }
            // If the request line is null, then the other end has hung up on us.  A well
            // behaved client will do this after 15-60 seconds of inactivity.
            if (request == null)
            {
                if (verbose)
                {
                    Console.WriteLine("Keep-alive broken after " + numRequests + " requests");
                }
                return false;
            }
            // HTTP request lines are of the form:
            // [METHOD] [Encoded URL] HTTP/1.?
            string[] tokens = request.Split(new char[] { ' ' });
            if (tokens.Length != 3)
            {
                writeError(400, "Bad request");
                return false;
            }
            // We currently only handle GET requests
            method = tokens[0];
            if (!method.Equals("GET"))
            {
                writeError(501, method + " not implemented");
                return false;
            }
            url = tokens[1];
            // Only accept valid urls
            if (!url.StartsWith("/"))
            {
                writeError(400, "Bad URL");
                return false;
            }
            // Decode all encoded parts of the URL using the built in URI processing class
            int i = 0;
            while ((i = url.IndexOf("%", i)) != -1)
            {
                url = url.Substring(0, i) + Uri.HexUnescape(url, ref i) + url.Substring(i);
            }
            // Lets just make sure we are using HTTP, thats about all I care about
            protocol = tokens[2];
            if (!protocol.StartsWith("HTTP/"))
            {
                writeError(400, "Bad protocol: " + protocol);
            }
            return true;
        }
        public bool readHeaders()
        {
            string line;
            string name = null;
            // The headers end with either a socket close (!) or an empty line
            while ((line = sr.ReadLine()) != null && line != "")
            {
                // If the value begins with a space or a hard tab then this
                // is an extension of the value of the previous header and
                // should be appended
                if (name != null && Char.IsWhiteSpace(line[0]))
                {
                    headers[name] += line;
                    continue;
                }
                // Headers consist of [NAME]: [VALUE] + possible extension lines
                int firstColon = line.IndexOf(":");
                if (firstColon != -1)
                {
                    name = line.Substring(0, firstColon);
                    String value = line.Substring(firstColon + 1).Trim();
                    if (verbose) Console.WriteLine(name + ": " + value);
                    headers[name] = value;
                }
                else
                {
                    writeError(400, "Bad header: " + line);
                    return false;
                }
            }
            return line != null;
        }

        public void writeFile(string filename)
        {
            try
            {
                // Open the file
                FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
                // Write the content length and the success header to the stream
                long left = fs.Length;

                writeSuccess(left);
                // Copy the contents of the file to the stream, ensure that we never write
                // more than the content length we specified.  Just in case the file somehow
                // changes out from under us, although I don't know if that is possible.
                BufferedStream bs = new BufferedStream(fs);
                int read;
                while (left > 0 && (read = bs.Read(bytes, 0, (int)Math.Min(left, bytes.Length))) != 0)
                {
                    ns.Write(bytes, 0, read);
                    left -= read;
                }
                ns.Flush();
                bs.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("[{0}] Error: {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), e.Message + e.StackTrace);
            }
        }


        /**
         * These write out the various HTTP responses that are possible with this
         * very simple web server.
         */

        public void writeSuccess(long length)
        {
            writeResult(200, "OK", length);
        }

        public void writeFailure()
        {
            writeError(404, "File not found");
        }

        public void writeForbidden()
        {
            writeError(403, "Forbidden");
        }

        public void writeError(int status, string message)
        {
            string output = "<h1>HTTP/1.0 " + status + " " + message + "</h1>";
            writeResult(status, message, (long)output.Length);
            sw.Write(output);
            sw.Flush();
        }

        public void writeResult(int status, string message, long length)
        {
            if (verbose) Console.WriteLine(request + " " + status + " " + numRequests);
            sw.Write("HTTP/1.0 " + status + " " + message + "\r\n");
            sw.Write("Content-Length: " + length + "\r\n");
            sw.Write("Content-Type: image/jpeg\r\n");
            if (keepAlive)
            {
                sw.Write("Connection: Keep-Alive\r\n");
            }
            else
            {
                sw.Write("Connection: close\r\n");
            }
            sw.Write("\r\n");
            sw.Flush();
        }
    }

    public class HttpServer
    {

        // ============================================================
        // Data

        public static bool verbose = false;
        private int port;
        private string docRoot;

        // ============================================================
        // Constructor

        public HttpServer(string docRoot, int port)
        {
            this.docRoot = docRoot;
            this.port = port;
        }

        // ============================================================
        // Listener

        public void listen()
        {
            IPHostEntry ipEntry = Dns.GetHostByName(Dns.GetHostName());
            IPAddress[] addr = ipEntry.AddressList;
            
            // Create a new server socket, set up all the endpoints, bind the socket and then listen
            Socket listener = new Socket(0, SocketType.Stream, ProtocolType.Tcp);

            IPAddress ipaddress = addr[0];
            IPEndPoint endpoint = new IPEndPoint(ipaddress, port);

            listener.Bind(endpoint);
            listener.Blocking = true;
            listener.Listen(-1);

            Console.WriteLine("[{0}] Socket listening, ip: {2}, port: {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), port, addr[0]);

            while (true)
            {
                try
                {
                    // Accept a new connection from the net, blocking till one comes in
                    Socket s = listener.Accept();
                    // Create a new processor for this request
                    HttpProcessor processor = new HttpProcessor(docRoot, s);
                    // Dispatch that processor in its own thread
                    Thread thread = new Thread(new ThreadStart(processor.process));
                    thread.SetApartmentState(ApartmentState.STA);
                    thread.Start();
                }
                catch (NullReferenceException)
                {
                    // Don't even ask me why they throw this exception when this happens
                    Console.WriteLine("Accept failed.  Another process might be bound to port " + port);
                }
            }
        }

        // ============================================================
        // Main

        private static void usage()
        {
            Console.WriteLine("Usage: httpd [-port port] [-docroot path] [-verbose] [/?]");
        }
        
        // Process all the command line parameters, create the listener, and dispatch it. We
        // could have just started listening in the main method, but its a little cleaner like this.
        public static int Main(String[] args)
        {
            HttpServer httpServer;
            int port = 777;
            string docRoot = ".";
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "/?":
                        usage();
                        return 1;
                    case "-port":
                        if (i < args.Length - 1)
                        {
                            port = int.Parse(args[++i]);
                        }
                        else
                        {
                            usage();
                        }
                        break;
                    case "-docroot":
                        if (i < args.Length - 1)
                        {
                            docRoot = args[++i];
                        }
                        else
                        {
                            usage();
                        }
                        break;
                    case "-verbose":
                        verbose = true;
                        break;
                    default:
                        usage();
                        return 1;
                }
            }

            httpServer = new HttpServer(docRoot, port);
            Thread thread = new Thread(new ThreadStart(httpServer.listen));
            thread.Start();
            return 0;
        }
    }
}
