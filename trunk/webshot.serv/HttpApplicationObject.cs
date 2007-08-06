using System;
using System.IO;
using System.Web;
using System.Text;
using System.Threading;
using System.Collections;



namespace T.W3SVC
{
    class StaticFileHandlerObject
    {
       // public using System.Web.HttpRequest
        public Int32 Hits;
        public Int32 Misses;
        private Hashtable StaticFileCache;
        private FileSystemWatcher FSW;

        public StaticFileHandlerObject()
        {
            StaticFileCache = new Hashtable(StringComparer.InvariantCultureIgnoreCase);

            //
            // Запускаем процедуру отслеживания изменений файла
            //
            FSW = new FileSystemWatcher(Directory.GetCurrentDirectory());

            //
            // Счетчики попаданий/промахов в кэш
            //
            Hits = 0;
            Misses = 0;
        }

        public void Transfer(string LocalPath, HttpRequestObject HttpRequest, HttpResponseObject HttpResponse)
        {
            //
            // Проверяем, существует ли запрошенный файл в кэше
            //
            string Filename = null;

            try
            {
                Filename = Path.GetFullPath(string.Concat(Directory.GetCurrentDirectory(), LocalPath));
            }
            catch (Exception E)
            {
                Console.WriteLine("{0}: Warning: Illegal characters in path ({1}).", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), LocalPath);
            }

            if (Filename == null)
            {
                Transfer("0.gif", HttpRequest, HttpResponse);
            }
            else
            {
                //
                // Content-Type
                //
                switch (Path.GetExtension(Filename).ToLower())
                {
                    case ".gif":
                        HttpResponse.ContentType = "image/gif";
                        break;

                    case ".png":
                        HttpResponse.ContentType = "image/png";
                        break;

                    case ".jpeg":
                    case ".jpg":
                    case ".jpe":
                        HttpResponse.ContentType = "image/jpeg";
                        break;

                    case ".ico":
                        HttpResponse.ContentType = "image/x-icon";
                        break;

                    case ".htm":
                    case ".html":
                        HttpResponse.ContentType = "text/html";
                        break;

                    default:
                        HttpResponse.Status = "404 Not Found";
                        HttpResponse.ContentType = "text/plain";
                        HttpResponse.Write("The server has not found anything matching the Request-URI. No indication is given of whether the condition is temporary or permanent.");
                        break;
                }

                //
                // Буфер для считывания данных
                //
                if (StaticFileCache.Contains(Filename))
                {
                    //
                    // Увеличиваем статистику попаданий в кэш
                    //
                    Interlocked.Increment(ref Hits);

                    HttpResponse.Status = "200 OK";
                    HttpResponse.Write(StaticFileCache[Filename].ToString());
                }
                else
                {
                    //
                    // Увеличиваем статистику промахов в кэш
                    //
                    Interlocked.Increment(ref Misses);

                    if (File.Exists(Filename))
                    {
                        try
                        {
                            using (FileStream FSO = File.Open(Filename, FileMode.Open, FileAccess.Read, FileShare.Read))
                            {
                                try
                                {
                                    //
                                    // При попытке обращения к файлу большому файлу выдать сообщение об ошибке
                                    //
                                    if (FSO.Length > 8192)
                                    {
                                        HttpResponse.Status = "413 Request Entity Too Large";
                                        HttpResponse.ContentType = "text/plain";
                                        HttpResponse.Write("The server is refusing to process a request because the request entity is larger than the server is able to process.");
                                    }
                                    else
                                    {
                                        byte[] Buffer = new byte[FSO.Length + 1];

                                        try
                                        {
                                            FSO.Read(Buffer, 0, FSO.Length);
                                        }
                                        catch (IOException E)
                                        {
                                            //
                                            // Логировать ошибку доступа к файлу
                                            //
                                            lock (Console.Out)
                                            {
                                                Console.WriteLine("{0}: Warning: an I/O exception occurs while reading file ({1}). A message that describes the reason for this is below.", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), Filename);
                                                Console.WriteLine();
                                                Console.WriteLine(E.ToString);
                                                Console.WriteLine();
                                                Console.WriteLine();
                                            }
                                        }
                                        catch (Exception E)
                                        {
                                            //
                                            // Логировать ошибку доступа к файлу
                                            //
                                            lock (Console.Out)
                                            {
                                                Console.WriteLine("{0}: Warning: an unhandled exception occurs while reading file ({1}). A message that describes the reason for this is below.", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), Filename);
                                                Console.WriteLine();
                                                Console.WriteLine(E.ToString);
                                                Console.WriteLine();
                                                Console.WriteLine();
                                            }
                                        }

                                        if (Buffer == null)
                                        {
                                            //
                                            // TODO: Вывести картинку с ошибкой
                                            //
                                            HttpResponse.Status = "403 Forbidden";
                                            HttpResponse.ContentType = "text/plain";
                                            HttpResponse.Write("The server understood the request, but is refusing to fulfill it. Authorization will not help and the request SHOULD NOT be repeated.");
                                        }
                                        else
                                        {
                                            //
                                            // FIXME: При большом количестве файлов в кэша возможно исчерпание памяти, поэтому необходимо сделать LRU-удаление данных
                                            //

                                            //
                                            // Добавляем файл в кэш
                                            //
                                            StaticFileCache.Item(Filename) = Buffer;

                                            //
                                            // Запускаем процедуру отслеживания изменений файлов, при необходимости
                                            //
                                            if (FSW.EnableRaisingEvents == false)
                                            {
                                                FSW.NotifyFilter = NotifyFilters.Security | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;

                                                //
                                                // Отслеживать изменения в подкаталогах
                                                //
                                                FSW.IncludeSubdirectories = true;

                                                //
                                                // Устанавливаем размер буфера регистрации изменений
                                                //
                                                FSW.InternalBufferSize = 8192;

                                                //
                                                // Добавляем процедуры слежения
                                                //
                                                FSW.Changed += FileChange;
                                                FSW.Deleted += FileChange;
                                                FSW.Renamed += FileRename;
                                                FSW.Error += InternalBufferOverflows;

                                                //
                                                // Запускаем отслеживание
                                                //
                                                FSW.EnableRaisingEvents = true;
                                            }

                                            HttpResponse.Status = "200 OK";
                                            HttpResponse.Write(Buffer);
                                        }
                                    }
                                }
                                finally
                                {
                                    FSO.Close();
                                }
                            }
                        }
                        catch (Exception E)
                        {
                            //
                            // TODO: Логировать ошибку доступа к файлу
                            //
                            lock (Console.Out)
                            {
                                Console.WriteLine("{0}: Warning: unable to open requested file ({1}) for reading. A message that describes the reason for this is below.", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), Filename);
                                Console.WriteLine();
                                Console.WriteLine(E.ToString);
                                Console.WriteLine();
                                Console.WriteLine();
                            }
                        }
                    }
                    else
                    {
                        HttpResponse.Redirect("http://www.kmindex.ru" + HttpRequest.Url.PathAndQuery);
                    }
                }
            }
        }

        private void FileChange(object Source, FileSystemEventArgs EventArgs)
        {
            //
            // Удаляем файл из кэша
            //
            StaticFileCache.Remove(EventArgs.FullPath);
        }

        private void FileRename(object Source, RenamedEventArgs EventArgs)
        {
            //
            // Удаляем файл из кэша
            //
            StaticFileCache.Remove(EventArgs.OldFullPath);
        }

        private void InternalBufferOverflows(object Source, ErrorEventArgs EventArgs)
        {
            //
            // FIXME: Возможно, необходима очистка кэша
            //
            Console.WriteLine("{0}: Error: internal buffer overflows. Component lose track of changes in the directory, and it will only provide blanket notification.", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        }
    }

    class HttpApplicationObject
    {
        private Random GUID = new Random();
        private LazyWriterObject LazyWriter;
        private ManualResetEvent MustStop;

        //
        // Объект управления отправкой статичных файлов
        //
        public StaticFileHandlerObject StaticFileHandler;

        public HttpApplicationObject(string Directory)
        {
            //
            // Инициализируем объект записи данных
            //
            LazyWriter = new LazyWriterObject(Directory);

            //
            // Инициализируем объект отправки статичных файлов
            //
            StaticFileHandler = new StaticFileHandlerObject();
        }

        public void Start(object Argument)
        {
            //
            // Получаем адрес сигнала останова
            //
            MustStop = (ManualResetEvent)Argument;

            //
            // Запускаем процедуру записи сырых данных в файл
            //
            LazyWriter.Start(MustStop);
        }

        public void HttpHandler(object Sender, HttpConnectionObject HttpConnection)
        {
            HttpRequestObject HttpRequest = HttpConnection.Request;
            HttpResponseObject HttpResponse = HttpConnection.Response;

            if (HttpRequest.Url.Host.StartsWith("counting") | HttpRequest.Url.AbsolutePath == "/c/")
            {
                Count(HttpRequest, HttpResponse);
            }

            if (HttpResponse.Length == 0)
            {
                //
                // Определяем тип содержимого по расширению запрашиваемого файла
                //
                switch (HttpRequest.Url.AbsolutePath)
                {
                    case "/":
                        if (HttpRequest.Url.Host.StartsWith("counting"))
                        {
                            StaticFileHandler.Transfer("/0.gif", HttpRequest, HttpResponse);
                        }
                        else
                        {
                            HttpResponse.Redirect("http://www.kmindex.ru" + HttpRequest.Url.PathAndQuery);
                        }

                        break;

                    case "/c/":
                    case "/c/index.asp":
                        StaticFileHandler.Transfer("/0.gif", HttpRequest, HttpResponse);
                        break;

                    case "/1.gif":
                        StaticFileHandler.Transfer("/1.gif", HttpRequest, HttpResponse);
                        break;

                    case "/32.gif":
                        StaticFileHandler.Transfer("/32.gif", HttpRequest, HttpResponse);
                        break;

                    case "/p/":
                        if (HttpRequest.Url.Query.IndexOf("&p=32") > -1)
                        {
                            StaticFileHandler.Transfer("/32.gif", HttpRequest, HttpResponse);
                        }
                        else
                        {
                            StaticFileHandler.Transfer("/1.gif", HttpRequest, HttpResponse);
                        }

                        break;

                    default:
                        //
                        // Выдаем запрошеный файл
                        //
                        StaticFileHandler.Transfer(HttpRequest.Url.LocalPath, HttpRequest, HttpResponse);
                        break;
                }
            }
        }

        private void Count(HttpRequestObject HttpRequest, HttpResponseObject HttpResponse)
        {
            //
            // Получаем параметры запроса
            //
            Int32 UserID = 0;
            string Referer = "";

            foreach (string NameAndValue in HttpRequest.Url.Query.TrimStart("?").Split("&"))
            {
                if (NameAndValue.IndexOf("=") > 0)
                {
                    string Name = NameAndValue.Substring(0, NameAndValue.IndexOf("="));

                    if (string.Compare(Name, "uid", StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        Int32.TryParse(NameAndValue.Substring(NameAndValue.IndexOf("=") + 1), UserID);
                    }
                    else if (string.Compare(Name, "id", StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        Int32 CID;

                        if (Int32.TryParse(NameAndValue.Substring(NameAndValue.IndexOf("=") + 1), CID))
                        {
                            UserID = CID / 10;
                        }
                    }
                    else if (string.Compare(Name, "r", StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        Referer = HttpUtility.UrlDecode(NameAndValue.Substring(NameAndValue.IndexOf("=") + 1));
                    }
                }
            }

            if (UserID == 0)
            {
                StaticFileHandler.Transfer("/change-version.gif", HttpRequest, HttpResponse);
            }
            else
            {
                //
                // Ищем идентификатор пользователя
                //
                Int32 UID = 0;
                System.Guid UNIQUEIDENTIFIER = System.Guid.Empty;

                if (HttpRequest.Headers.Contains("Cookie"))
                {
                    string[] Cookies = HttpRequest.Headers.Item("Cookie").Split(";");

                    for (Int32 i = 0; i <= Cookies.Length - 1; i++)
                    {
                        string Cookie = Cookies(i).Trim;

                        Int32 j = Cookie.IndexOf("=");

                        if (j > -1)
                        {
                            if (string.Compare(Cookie.Substring(0, j), "U", StringComparison.InvariantCultureIgnoreCase) == 0)
                            {
                                if (Int32.TryParse(Cookie.Substring(j + 1), UID) == false)
                                {
                                    Console.WriteLine("{0}: EXCONVERSION: User identifier is invalid ({1}).", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "{" + Cookie.Substring(j + 1) + "}");
                                }

                                //
                                // FIXME: Проблема с -1
                                //
                                if (UID == -1)
                                {
                                    UID = 0;
                                }
                            }
                            else if (string.Compare(Cookie.Substring(0, j), "GUID", StringComparison.InvariantCultureIgnoreCase) == 0)
                            {
                                try
                                {
                                    UNIQUEIDENTIFIER = new System.Guid(Cookie.Substring(j + 1));
                                }
                                catch (FormatException E)
                                {
                                    Console.WriteLine("{0}: EXCONVERSION: Guid format is invalid ({1}).", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "{" + Cookie.Substring(j + 1) + "}");
                                }
                                catch (OverflowException E)
                                {
                                    Console.WriteLine("{0}: EXCONVERSION: Guid format is invalid ({1}).", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "{" + Cookie.Substring(j + 1) + "}");
                                }
                            }
                        }
                    }
                }
                else
                {
                    UID = 0;
                }

                if (UID == 0)
                {
                    //
                    // Присвоить посетителю новый идентификатор
                    //
                    UID = GUID.Next(Int32.MinValue, Int32.MaxValue);

                    HttpResponse.Cookie = "U=" + UID + "; expires=Wednesday, 01-Jan-2020 00:00:00 GMT; domain=.kmindex.ru; path=/";
                    HttpResponse.P3P = "CP=UNI";
                }

                if (UNIQUEIDENTIFIER.Equals(System.Guid.Empty))
                {
                    UNIQUEIDENTIFIER = System.Guid.NewGuid;

                    HttpResponse.Cookie = "GUID=" + UNIQUEIDENTIFIER.ToString("D") + "; expires=Wednesday, 01-Jan-2020 00:00:00 GMT; domain=.kmindex.ru; path=/";
                    HttpResponse.P3P = "CP=UNI";
                }

                //
                // User-Agent
                //
                string UserAgent;

                if (HttpRequest.Headers.Contains("User-Agent"))
                {
                    UserAgent = HttpRequest.Headers.Item("User-Agent");
                }
                else
                {
                    UserAgent = "";
                }

                //
                // Записываем строку в лог-файл
                //
                StringBuilder Record = new StringBuilder(8192);

                Record.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                Record.Append(Strings.Chr(0));
                Record.Append(UserID);
                Record.Append(Strings.Chr(0));
                Record.Append(HttpRequest.UserHostAddress);
                Record.Append(Strings.Chr(0));
                Record.Append(UID);
                Record.Append(Strings.Chr(0));
                Record.Append(UNIQUEIDENTIFIER.ToString("D"));
                Record.Append(Strings.Chr(0));

                if (HttpRequest.Headers.Contains("referer"))
                {
                    Record.Append(SafeValue(HttpRequest.Headers.Item("referer")));
                }

                Record.Append(Strings.Chr(0));
                Record.Append(SafeValue(Referer));
                Record.Append(Strings.Chr(0));
                Record.Append(SafeValue(UserAgent));
                Record.Append(Strings.Chr(13));
                Record.Append(Strings.Chr(10));

                LazyWriter.Write(Record.ToString);
            }
        }

        private string SafeValue(string Value)
        {
            //
            // Убираем ненужные символы из строки
            //
            return Value.Replace(Strings.Chr(13), " ").Replace(Strings.Chr(10), " ").Replace(Strings.Chr(0), " ");
        }
    }
}