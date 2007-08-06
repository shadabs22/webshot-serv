using System;
using System.IO;
using System.Collections;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace T.W3SVC
{
    //
    // Обработчик запроса
    //
    public delegate void HttpHandler(object Sender, HttpConnectionObject HttpConnection);

    public enum HttpConnectionState
    {
        Accept,
        Receive,
        Idle,
        Close
    }

    public class HttpConnectionObject
    {
        public object SyncRoot;
        public HttpConnectionState State;
        public Socket Socket;
        public HttpRequestObject Request;
        public HttpResponseObject Response;

        public HttpConnectionObject()
        {
            Request = new HttpRequestObject();
            Response = new HttpResponseObject();

            //
            // Создаем объект для синхронизации доступа
            //
            SyncRoot = new object();

            //
            // Переводим соединение в закрытое состояние
            //
            State = HttpConnectionState.Close;
        }

        public void Close()
        {
            if (State != HttpConnectionState.Close)
            {
                try
                {
                    try
                    {
                        Socket.Shutdown(SocketShutdown.Both);
                    }
                    finally
                    {
                        Socket.Close();
                    }
                }
                catch (ObjectDisposedException E)
                {
                }
                //
                // Клиент уже отключился
                //
                catch (SocketException E)
                {
                    Console.WriteLine("{0}: Warning: unable to shutdown connection with {1} due to socket error {2}. Refer to the Windows Sockets version 2 API error code documentation in the MSDN library for a detailed description of the error.", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), Request.UserHostAddress, E.ErrorCode);
                }

                Request.Clear();
                Response.Clear();

                State = HttpConnectionState.Close;
            }
        }
    }

    public class HttpRequestObject
    {
        public byte[] Buffer = new byte[8193];
        public Int32 Length;
        public string UserHostAddress;
        public string HttpMethod;
        public string RawUrl;
        public string HttpVersion;

        public Hashtable Headers;
        public Uri Url;

        private Int32 Position;
        private Int32 Cr;
        private Int32 Lf;

        public HttpRequestObject()
        {
            Headers = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
            this.Clear();
        }

        public void Clear()
        {
            Position = 0;
            Cr = 0;
            Lf = 0;
            Length = 0;
            HttpMethod = "";
            RawUrl = "";
            HttpVersion = "";
            UserHostAddress = "";
            Url = null;
            Headers.Clear();
        }

        public bool IsAvailable
        {
            get
            {
                while (Position < Length)
                {
                    if (Buffer(Position) == 13)
                    {
                        Cr = Cr + 1;
                    }
                    else if (Buffer(Position) == 10)
                    {
                        Lf = Lf + 1;
                    }
                    else
                    {
                        Cr = 0;
                        Lf = 0;
                    }

                    if (Cr == 2 | Lf == 2)
                    {
                        return true;
                    }

                    Position = Position + 1;
                }

                return false;
            }
        }
    }

    public class HttpResponseObject
    {
        public string Status;
        public string ContentType;
        public string Location;
        public string Cookie;
        public string P3P;

        private MemoryStream OutputStream;
        private MemoryStream ContentStream;

        public HttpResponseObject()
        {
            //
            // Выделяем память для тела ответа
            //
            OutputStream = new MemoryStream(16384);
            ContentStream = new MemoryStream(10240);

            //
            // Приводим структуры в начальное состояние
            //
            this.Clear();
        }

        public byte[] GetBuffer
        {
            get { return OutputStream.GetBuffer; }
        }

        public Int32 TotalBytes
        {
            get { return OutputStream.Length; }
        }

        public Int32 Length
        {
            get { return ContentStream.Length; }
        }

        public void Clear()
        {
            OutputStream.SetLength(0);
            ContentStream.SetLength(0);
            Status = "500 Internal Server Error";
            ContentType = "text/plain";
            Location = "";
            Cookie = "";
            P3P = "";
        }

        public void Close()
        {
            //
            // Подготавливаем заголовок ответа
            //
            StringBuilder Header = new StringBuilder(1024);

            Header.Append("HTTP/1.1 ");
            Header.Append(Status);
            Header.Append(Constants.vbCrLf);
            Header.Append("Content-Length: ");
            Header.Append(Length);
            Header.Append(Constants.vbCrLf);
            Header.Append("Content-Type: ");
            Header.Append(ContentType);
            Header.Append(Constants.vbCrLf);
            Header.Append("Connection: Close");
            Header.Append(Constants.vbCrLf);
            Header.Append("Date: ");
            Header.Append(DateTime.Now.AddHours(-3).AddMinutes(-5).ToString("r"));
            Header.Append(Constants.vbCrLf);
            Header.Append("Expires: Wed, 17 Sep 1980 00:00:00 GMT");
            Header.Append(Constants.vbCrLf);
            Header.Append("Pragma: no-cache");
            Header.Append(Constants.vbCrLf);

            if (Location != "")
            {
                Header.Append("Location: ");
                Header.Append(Location);
                Header.Append(Constants.vbCrLf);
            }

            if (Cookie != "")
            {
                Header.Append("Set-Cookie: ");
                Header.Append(Cookie);
                Header.Append(Constants.vbCrLf);
            }

            if (P3P != "")
            {
                Header.Append("P3P: ");
                Header.Append(P3P);
                Header.Append(Constants.vbCrLf);
            }

            Header.Append(Constants.vbCrLf);

            //
            // Прицепляем заголовок ответа в начало
            //
            byte[] Buffer = Encoding.Default.GetBytes(Header.ToString);

            OutputStream.Write(Buffer, 0, Buffer.Length);

            //
            // Копируем тело ответа
            //
            if (Length > 0)
            {
                OutputStream.Write(ContentStream.GetBuffer, 0, Convert.ToInt32(ContentStream.Length));
            }
        }

        public void Redirect(string Url)
        {
            Status = "302 Object Moved";
            Location = Url;
        }

        public void Write(string Text)
        {
            byte[] Buffer = Encoding.Default.GetBytes(Text);

            FileSystem.Write(Buffer);
        }

        public void Write(byte[] Buffer)
        {
            ContentStream.Write(Buffer, 0, Buffer.Length);
        }
    }

    class W3SVC
    {
        //
        // Основная процедура приема запросов
        //
        private Thread Accepting;

        // Очередь обработки запросов
        private HttpConnectionObject[] Connections = new HttpConnectionObject[1001];

        //
        //
        //
        private AsyncCallback ReceiveCallback;

        //
        //
        //
        private AsyncCallback AcceptCallback;

        private char[] RowsDelimiter = { Strings.Chr(13), Strings.Chr(10) };

        public event HttpHandler HandleHttpRequest;

        private Socket ServerSocket;

        public W3SVC()
        {
            //
            // Сокет приема запросов
            //
            ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //
            // Запускаем процедуру приема запросов
            //
            Accepting = new Thread(Service);

            //
            // Инициализируем массив соединений
            //
            for (Int32 i = 0; i <= Connections.Length - 1; i++)
            {
                Connections(i) = new HttpConnectionObject();
            }

            //
            // Регистрируем процедуру асинхронного приема соединений
            //
            AcceptCallback = new AsyncCallback(OnAccept);

            //
            // Регистрируем процедуру асинхронного приема данных
            //
            ReceiveCallback = new AsyncCallback(OnReceive);
        }

        public void Start()
        {
            try
            {
                Accepting.Start();
            }
            catch (Exception E)
            {
                lock (Console.Out)
                {
                    Console.WriteLine("{0}: Error: unable to start service. A message that describes the reason for this is below.", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    Console.WriteLine();
                    Console.WriteLine(E.ToString);
                    Console.WriteLine();
                    Console.WriteLine();
                }
            }
        }

        public void Bind(string Address, Int32 Port)
        {
            try
            {
                if (Address == "*")
                {
                    ServerSocket.Bind(new IPEndPoint(IPAddress.Any, Port));

                    Console.WriteLine("{0}: Information: server listening on {2} port of every IP address.", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), Address, Port);
                }
                else
                {
                    ServerSocket.Bind(new IPEndPoint(IPAddress.Parse(Address), Port));

                    Console.WriteLine("{0}: Information: server listening on {1}:{2}.", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), Address, Port);
                }
            }
            catch (ArgumentNullException E)
            {
                Console.WriteLine("{0}: Error: server address not specified. Check configuration options.", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            }
            catch (FormatException E)
            {
                Console.WriteLine("{0}: Error: server address is not in corrent format. Check configuration options.", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            }
        }

        private void Service()
        {
            //
            // Устанавливаем параметры серверного сокета
            //
            ServerSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
            ServerSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, 1);
            ServerSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, 0);
            ServerSocket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, 0);

            if (ServerSocket.IsBound)
            {
                try
                {
                    ServerSocket.Listen(5000);

                    //
                    // Выводим информацию о состоянии сервера
                    //
                    Console.WriteLine("{0}: Information: server process identifier is {1}.", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), System.Diagnostics.Process.GetCurrentProcess.Id);

                    //
                    // Принимаем соединение
                    //
                    //ServerSocket.BeginAccept(Connections(0).Request.Buffer.Length, AcceptCallback, 0)
                    //ServerSocket.BeginAccept(100, AcceptCallback, 0)
                    Connections(0).State = HttpConnectionState.Accept;

                    ServerSocket.BeginAccept(AcceptCallback, 0);

                    //
                    // TODO: Заглушка останова сервера
                    //
                    ManualResetEvent MustStop = new ManualResetEvent(false);

                    while (MustStop.WaitOne(60000, false) == false)
                    {
                        //
                        // Подсчитываем количество свободных соединений
                        //
                        Int32 Accept = 0;
                        Int32 Receive = 0;
                        Int32 Idle = 0;
                        Int32 Close = 0;

                        for (Int32 i = 0; i <= Connections.Length - 1; i++)
                        {
                            switch (Connections[i].State)
                            {
                                case HttpConnectionState.Accept:
                                    Accept = Accept + 1;
                                    break;

                                case HttpConnectionState.Receive:
                                    Receive = Receive + 1;
                                    break;

                                case HttpConnectionState.Idle:
                                    Idle = Idle + 1;
                                    break;

                                case HttpConnectionState.Close:
                                    Close = Close + 1;
                                    break;
                            }
                        }

                        Console.WriteLine("{0}: Information: {1} connection is in accept state, {2} in receive state, {3} idle, {4} closed.", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), Accept, Receive, Idle, Close);
                    }
                }
                catch (SocketException E)
                {
                    lock (Console.Out)
                    {
                        Console.WriteLine("{0}: Error: an error ({1}) occurred when attempting to access the socket. A message that describes the reason for this is below.", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), E.ErrorCode);
                        Console.WriteLine();
                        Console.WriteLine(E.ToString);
                        Console.WriteLine();
                        Console.WriteLine();
                    }
                }
                finally
                {
                    ServerSocket.Close();
                }
            }
            else
            {
                Console.WriteLine("{0}: Error: server is not bound to any IP address. Check configuraton file.", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            }
        }

        private void OnAccept(IAsyncResult AsyncResult)
        {
            Int32 i = (Int32)AsyncResult.AsyncState;
            HttpConnectionObject HttpConnection = Connections(i);

            //
            // Принимаем сокет
            //
            Connections[i].State = HttpConnectionState.Receive;

            try
            {
                //HttpConnection.Socket = ServerSocket.EndAccept(HttpConnection.Request.Buffer, HttpConnection.Request.Length, AsyncResult)
                HttpConnection.Socket = ServerSocket.EndAccept(AsyncResult);
            }
            catch (ObjectDisposedException E)
            {
                Connections[i].State = HttpConnectionState.Close;
            }
            catch (SocketException E)
            {
                Connections[i].State = HttpConnectionState.Close;
            }

            if (Connections[i].State == HttpConnectionState.Receive)
            {
                //
                // Определяем IP-адрес
                //
                HttpConnection.Request.UserHostAddress = ((IPEndPoint)HttpConnection.Socket.RemoteEndPoint).Address.ToString();

                //
                // Проверяем, достаточно ли данных для обработки запроса
                //
                //ProcessRequest(HttpConnection)

                //
                // Продолжаем прием данных, пока не будет достигнут конец запроса
                //
                try
                {
                    HttpConnection.Socket.BeginReceive(HttpConnection.Request.Buffer, HttpConnection.Request.Length, HttpConnection.Request.Buffer.Length - HttpConnection.Request.Length, SocketFlags.None, ReceiveCallback, HttpConnection);
                }
                catch (ObjectDisposedException E)
                {
                    HttpConnection.Close();
                }
                catch (SocketException E)
                {
                    Console.WriteLine("{0}: Warning: unable to receive data from {1} due to socket error {2}. Refer to the Windows Sockets version 2 API error code documentation in the MSDN library for a detailed description of the error.", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), HttpConnection.Request.UserHostAddress, E.ErrorCode);

                    HttpConnection.Close();
                }
            }

            //
            // Выбираем следующее свободное соединение в списке
            //
            do
            {
                i = (i + 1) % Connections.Length;

                lock (Connections[i].SyncRoot)
                {
                    switch (Connections[i].State)
                    {
                        case HttpConnectionState.Idle:
                            //
                            // Закрываем зависшие соединения
                            //
                            Console.WriteLine("{0}: Information: connection with {1} timed out and was foribly closed.", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), HttpConnection.Request.UserHostAddress);

                            Connections[i].Close();
                            break;

                        case HttpConnectionState.Receive:
                            Connections[i].State = HttpConnectionState.Idle;
                            break;

                        case HttpConnectionState.Close:
                            Connections[i].State = HttpConnectionState.Accept;
                            break;
                    }
                }
            }
            while (!(Connections[i].State == HttpConnectionState.Accept));

            //
            // Продолжаем прием соединений
            //
            ServerSocket.BeginAccept(AcceptCallback, i);
        }

        private void OnReceive(IAsyncResult AsyncResult)
        {
            //
            // Получаем контекст запроса
            //
            HttpConnectionObject HttpConnection = (HttpConnectionObject)AsyncResult.AsyncState;

            lock (HttpConnection.SyncRoot)
            {
                if (HttpConnection.State == HttpConnectionState.Close)
                {
                    //
                    // Соединение уже закрыто
                    //
                    return;
                }
                else
                {
                    //
                    // Изменяем состояние соединения для оповещения о том, что оно все еще активно
                    //
                    HttpConnection.State = HttpConnectionState.Receive;
                }
            }

            //
            // Считать принятые данные
            //
            Int32 BytesReceived = 0;

            try
            {
                BytesReceived = HttpConnection.Socket.EndReceive(AsyncResult);
            }
            catch (ObjectDisposedException E)
            {
            }
            //
            // Клиент уже отключился
            //
            catch (ArgumentNullException E)
            {
                Console.WriteLine("{0}: Warning: unable to receive data from {1}. AsyncResult is a null reference.", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), HttpConnection.Request.UserHostAddress);
            }
            catch (InvalidOperationException E)
            {
                Console.WriteLine("{0}: Warning: unable to receive data from {1}. AsyncResult was not returned by a call to the BeginReceive method.", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), HttpConnection.Request.UserHostAddress);
            }
            catch (ArgumentException E)
            {
                Console.WriteLine("{0}: Warning: unable to receive data from {1}. EndReceive was previously called for the asynchronous read.", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), HttpConnection.Request.UserHostAddress);
            }
            catch (SocketException E)
            {
                Console.WriteLine("{0}: Warning: unable to receive data from {1} due to socket error {2}. Refer to the Windows Sockets version 2 API error code documentation in the MSDN library for a detailed description of the error.", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), HttpConnection.Request.UserHostAddress, E.ErrorCode);
            }

            if (BytesReceived > 0)
            {
                HttpConnection.Request.Length = HttpConnection.Request.Length + BytesReceived;

                //
                // Обрабатываем запрос
                //
                ProcessRequest(HttpConnection);
            }
            else
            {
                HttpConnection.Close();
            }
        }

        private void ProcessRequest(HttpConnectionObject HttpConnection)
        {
            HttpRequestObject HttpRequest = HttpConnection.Request;
            HttpResponseObject HttpResponse = HttpConnection.Response;

            if (HttpRequest.Length < HttpRequest.Buffer.Length)
            {
                if (HttpRequest.IsAvailable)
                {
                    //
                    // Заголовок запроса принят, заканчиваем прием данных
                    //
                    try
                    {
                        HttpConnection.Socket.Shutdown(SocketShutdown.Receive);
                    }
                    catch (ObjectDisposedException E)
                    {
                    }
                    //
                    // Клиент уже отключился
                    //
                    catch (SocketException E)
                    {
                        Console.WriteLine("{0}: Warning: unable to shutdown connection with {1} due to socket error {2}. Refer to the Windows Sockets version 2 API error code documentation in the MSDN library for a detailed description of the error.", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), HttpRequest.UserHostAddress, E.ErrorCode);
                    }

                    //
                    // Переводим принятые данные в строку
                    //
                    string Received = Encoding.Default.GetString(HttpRequest.Buffer, 0, HttpRequest.Length);

                    Int32 EOL = Received.IndexOfAny(RowsDelimiter);

                    //
                    // Разбираем заголовок запроса
                    //
                    string[] Heading = Received.Substring(0, EOL).Split(" ");

                    if (Heading.Length == 3)
                    {
                        HttpRequest.HttpMethod = Heading[0];
                        HttpRequest.RawUrl = Heading[1];
                        HttpRequest.HttpVersion = Heading[2];
                    }

                    if (HttpRequest.HttpVersion != "HTTP/0.9" & HttpRequest.HttpVersion != "HTTP/1.0" & HttpRequest.HttpVersion != "HTTP/1.1")
                    {
                        HttpResponse.Status = "505 HTTP Version Not Supported";
                        HttpResponse.ContentType = "text/plain";
                        HttpResponse.Write("The server does not support, or refuses to support, the HTTP protocol version that was used in the request message.");
                        HttpResponse.Write(Constants.vbCrLf);
                        HttpResponse.Write(Constants.vbCrLf);
                        HttpResponse.Write("This server support only 0.9, 1.0 and 1.1 HTTP versions.");
                    }
                    else if (HttpRequest.HttpMethod != "GET")
                    {
                        HttpResponse.Status = "501 Not Implemented";
                        HttpResponse.ContentType = "text/plain";
                        HttpResponse.Write("Method is unrecognized or not implemented by the server.");
                    }
                    else
                    {
                        Int32 SOL = Received.IndexOfAny(RowsDelimiter, EOL + 1);

                        while (SOL > -1)
                        {
                            EOL = Received.IndexOfAny(RowsDelimiter, SOL + 1);

                            if (EOL - SOL > 1)
                            {
                                string Line = Received.Substring(SOL + 1, EOL - SOL - 1);
                                Int32 Semicolon = Line.IndexOf(":");

                                if (Semicolon > 0)
                                {
                                    string Name = Line.Substring(0, Semicolon);

                                    //
                                    // UNDONE: Преобразовываем синонимы названий к стандартному виду
                                    //
                                    if (string.Compare(Name, "Cookies", StringComparison.InvariantCultureIgnoreCase) == 0)
                                    {
                                        Name = "Cookie";
                                    }
                                    else if (string.Compare(Name, "Referrer", StringComparison.InvariantCultureIgnoreCase) == 0)
                                    {
                                        Name = "Referer";
                                    }

                                    string Value;

                                    if (Semicolon + 1 < Line.Length)
                                    {
                                        Value = Line.Substring(Semicolon + 1).Trim;
                                    }
                                    else
                                    {
                                        Value = "";
                                    }

                                    HttpRequest.Headers.Item[Name] = Value;
                                }
                            }

                            SOL = Received.IndexOfAny(RowsDelimiter, EOL + 1);
                        }

                        try
                        {
                            if (HttpRequest.Headers.Item("host") == null)
                            {
                                if (HttpRequest.RawUrl.StartsWith("http"))
                                {
                                    HttpRequest.Url = new Uri(HttpRequest.RawUrl);
                                }
                                else
                                {
                                    HttpRequest.Url = new Uri("http://" + HttpRequest.RawUrl);
                                }
                            }
                            else
                            {
                                HttpRequest.Url = new Uri("http://" + HttpRequest.Headers.Item("host") + HttpRequest.RawUrl);
                            }
                        }
                        catch
                        {
                            HttpResponse.Status = "400 Bad Request";
                            HttpResponse.ContentType = "text/plain";
                            HttpResponse.Write("The exact resource identified by an Internet request cannot be determined by examining both the Request-URI and the Host header field.");
                        }

                        if (HttpResponse.Length == 0)
                        {
                            //
                            // Сигнализируем о возможности обработать запрос приложением
                            //
                            if (HandleHttpRequest != null)
                            {
                                HandleHttpRequest(this, HttpConnection);
                            }
                        }
                    }
                }
                else
                {
                    //
                    // Продолжаем прием данных, пока не будет достигнут конец запроса
                    //
                    try
                    {
                        HttpConnection.Socket.BeginReceive(HttpRequest.Buffer, HttpRequest.Length, HttpRequest.Buffer.Length - HttpRequest.Length, SocketFlags.None, ReceiveCallback, HttpConnection);
                    }
                    catch (ObjectDisposedException E)
                    {
                        HttpConnection.Close();
                    }
                    catch (SocketException E)
                    {
                        Console.WriteLine("{0}: Warning: unable to receive data from {1} due to socket error {2}. Refer to the Windows Sockets version 2 API error code documentation in the MSDN library for a detailed description of the error.", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), HttpRequest.UserHostAddress, E.ErrorCode);

                        HttpConnection.Close();
                    }
                }
            }
            else
            {
                //
                // Входной буфер переполнен
                //
                HttpResponse.Status = "414 Request-URI Too Long";
                HttpResponse.ContentType = "text/plain";
                HttpResponse.Write("The server is refusing to service the request because the Request-URI is longer than the server is willing to interpret.");
            }

            if (HttpResponse.Length > 0 | HttpResponse.Location.Length > 0)
            {
                //
                // Заканчиваем обработку запроса
                //
                HttpResponse.Close();

                //
                // Направляем ответ посетилелю
                //
                try
                {
                    HttpConnection.Socket.Send(HttpResponse.GetBuffer, 0, Convert.ToInt32(HttpResponse.TotalBytes), SocketFlags.None);
                }
                catch (ObjectDisposedException E)
                {
                }
                //
                // Клиент уже отключился
                //
                catch (ArgumentNullException E)
                {
                    Console.WriteLine("{0}: Warning: unable to send data to {1}. Buffer is a null reference.", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), HttpRequest.UserHostAddress);
                }
                catch (ArgumentOutOfRangeException E)
                {
                    Console.WriteLine("{0}: Warning: unable to send data to {1}.", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), HttpRequest.UserHostAddress);
                }
                catch (SocketException E)
                {
                    Console.WriteLine("{0}: Warning: unable to send data to {1} due to socket error {2}. Refer to the Windows Sockets version 2 API error code documentation in the MSDN library for a detailed description of the error.", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), HttpRequest.UserHostAddress, E.ErrorCode);
                }
                finally
                {
                    HttpConnection.Close();
                }
            }
        }
    }

    class UI
    {
        public static void Main(string[] Arguments)
        {
            string XmlFileName = "W3SVC.XML";

            if (File.Exists(XmlFileName))
            {
                FileStream XmlFileStream = null;

                try
                {
                    XmlFileStream = File.Open(XmlFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                }
                catch (Exception E)
                {
                    lock (Console.Out)
                    {
                        Console.WriteLine("{0}: Unable to open configuration file ({1}). A message that describes the reason for this is below.", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), XmlFileName);
                        Console.WriteLine(E.ToString);
                        Console.WriteLine();
                    }
                }

                if (XmlFileStream == null)
                {
                }
                else
                {
                    System.Xml.XmlDocument Configuration = new System.Xml.XmlDocument();

                    try
                    {
                        Configuration.Load(XmlFileStream);

                        //
                        // Флажок необходимости останова сервиса
                        //
                        ManualResetEvent MustStop = new ManualResetEvent(false);

                        //
                        // Инициализируем веб-сервис приема и обработки запросов
                        //
                        W3SVC W3 = new W3SVC();

                        //
                        // Директория для записи CSV-файлов
                        //
                        foreach (System.Xml.XmlElement Node in Configuration.SelectNodes("/Root/Application/Directory"))
                        {
                            //
                            // Инициализируем приложение обработки запросов
                            //
                            HttpApplicationObject HttpApplication = new HttpApplicationObject(Node.GetAttribute("Path"));

                            // Запускаем его
                            try
                            {
                                HttpApplication.Start(MustStop);
                            }
                            catch (Exception E)
                            {
                                lock (Console.Out)
                                {
                                    Console.WriteLine("{0}: Error: unable to start HTTP application. A message that describes the reason for this is below.", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                    Console.WriteLine();
                                    Console.WriteLine(E.ToString);
                                    Console.WriteLine();
                                    Console.WriteLine();
                                }
                            }

                            // Подключаем обработчик событий
                            W3.HandleHttpRequest += HttpApplication.HttpHandler;
                        }

                        //
                        // Адреса, по которым должен отвечать веб-сервер
                        //
                        foreach (System.Xml.XmlElement Node in Configuration.SelectNodes("/Root/W3/IPEndPoint"))
                        {
                            Int32 Port;

                            if (Int32.TryParse(Node.GetAttribute("Port"), Port))
                            {
                                string Address = Node.GetAttribute("Address");

                                if (Address == null)
                                {
                                }
                                else
                                {
                                    W3.Bind(Address, Port);
                                }
                            }
                        }

                        W3.Start();
                    }
                    catch (System.Xml.XmlException E)
                    {
                        lock (Console.Out)
                        {
                            Console.WriteLine("{0}: Unable to load configuration file ({1}). A message that describes the reason for this is below.", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), XmlFileName);
                            Console.WriteLine(E.ToString);
                            Console.WriteLine();
                        }
                    }

                    XmlFileStream.Close();
                }
            }
            else
            {
                Console.WriteLine("{0}: Configuration file ({1}) does not exists or access denied.", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), XmlFileName);
            }
        }
    }
}