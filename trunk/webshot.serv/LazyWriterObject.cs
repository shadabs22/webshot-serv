using System;
using System.IO;
using System.Web;
using System.Text;
using System.Threading;
using System.Collections;

namespace T.W3SVC
{
    class LazyWriterObject
    {
        private Queue WriteQueue;
        private FileStream RawStream;
        private DateTime FileDateTime;
        private Thread Monitoring;
        private Thread Flushing;
        private ManualResetEvent MustStop;

        //
        // ������� ��� ������ CSV-������
        //
        private string Directory;

        public LazyWriterObject(string Directory)
        {
            //
            // ��������� ��� ���������� ��� ������ CSV-������
            //
            this.Directory = Directory;

            //
            // �������������� ������� ������ ��� ������ � ����
            //
            WriteQueue = Queue.Synchronized(new Queue(1000000));
        }

        public void Start(object Argument)
        {
            //
            // �������� ����� ������� ��������
            //
            MustStop = (ManualResetEvent)Argument;

            //
            // ��������� ��������� ���������������
            //
            Monitoring = new Thread(Monitor);

            try
            {
                Monitoring.Start();
            }
            catch (Exception E)
            {
                lock (Console.Out)
                {
                    Console.WriteLine("{0}: Warning: unable to start LazyWriter monitoring thread. A message that describes the reason for this is below.", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    Console.WriteLine();
                    Console.WriteLine(E.ToString());
                    Console.WriteLine();
                    Console.WriteLine();
                }
            }

            //
            // ��������� ��������� ������ ���������� � ����
            //
            Flushing = new Thread(Flush);

            try
            {
                Flushing.Start();
            }
            catch (Exception E)
            {
                lock (Console.Out)
                {
                    Console.WriteLine("{0}: Error: unable to start LazyWriter thread. A message that describes the reason for this is below.", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    Console.WriteLine();
                    Console.WriteLine(E.ToString());
                    Console.WriteLine();
                    Console.WriteLine();
                }
            }
        }

        public bool IsAlive()
        {
            if (Monitoring.IsAlive | Flushing.IsAlive)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool Write(string Data)
        {
            if (WriteQueue.Count > 1000000)
            {
                //
                // ��� ���������� ������� ������� (��� ����� �������� ��-�� ���������� �����) �������� ����������� ������
                //
                return false;
            }

            WriteQueue.Enqueue(Data);

            return true;
        }

        public void Monitor()
        {
            //
            // ������� ���������� ��� �����������
            //
            while (MustStop.WaitOne(60000, false) == false)
            {
                Console.WriteLine("{0}: Information: current write queue length is {1}.", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), WriteQueue.Count);
            }
        }

        private void Flush()
        {
            //
            // ����� ��� �������� ������ � ������������������ ������
            //
            byte[] Buffer = new byte[4097];

            //
            // �������� ��������� ��� ������ ������
            //
            Encoding MyEncoding = Encoding.GetEncoding(1251);

            while (MustStop.WaitOne(0, false) == false)
            {
                if (RawStream == null)
                {
                    //
                    // ������������� ������� ����� �����
                    //
                    FileDateTime = DateTime.Now;

                    //
                    // ������� ������� ����
                    //
                    try
                    {
                        RawStream = new FileStream(Directory + FileDateTime.ToString("yyyyMMddHH") + ".CSV", FileMode.Append, FileAccess.Write, FileShare.Read, 4096, FileOptions.WriteThrough);
                    }
                    catch (ArgumentOutOfRangeException E)
                    {
                        Console.WriteLine("{0}: Warning: unable to open file ({1}) with specified mode, access and share.", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), Directory + FileDateTime.ToString("yyyyMMddHH") + ".CSV");
                    }
                    catch (ArgumentException E)
                    {
                        Console.WriteLine("{0}: Warning: unable to open file ({1}) because it is an empty string, contains only white space, contains one or more invalid characters or refers to a non-file device in a non-NTFS environment.", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), Directory + FileDateTime.ToString("yyyyMMddHH") + ".CSV");
                    }
                    catch (NotSupportedException E)
                    {
                        Console.WriteLine("{0}: Warning: unable to open file ({1}) because contains one or more invalid characters or refers to a non-file device in a non-NTFS environment.", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), Directory + FileDateTime.ToString("yyyyMMddHH") + ".CSV");
                    }
                    catch (System.Security.SecurityException E)
                    {
                        Console.WriteLine("{0}: Warning: unable to open file ({1}) due to the lack of required permissions.", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), Directory + FileDateTime.ToString("yyyyMMddHH") + ".CSV");
                    }
                    catch (DirectoryNotFoundException E)
                    {
                        Console.WriteLine("{0}: Warning: the specified path ({1}) is invalid, such as being on an unmapped drive.", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), Directory + FileDateTime.ToString("yyyyMMddHH") + ".CSV");
                    }
                    catch (UnauthorizedAccessException E)
                    {
                        Console.WriteLine("{0}: Warning: the access requested is not permitted by the operating system for the specified path ({1}), such as file or directory is set for read-only access.", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), Directory + FileDateTime.ToString("yyyyMMddHH") + ".CSV");
                    }
                    catch (PathTooLongException E)
                    {
                        Console.WriteLine("{0}: Warning: the specified path ({1}) exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters.", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), Directory + FileDateTime.ToString("yyyyMMddHH") + ".CSV");
                    }
                    catch (IOException E)
                    {
                        Console.WriteLine("{0}: Warning: an I/O error occurs when opening file ({1}). The stream has been closed.", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), Directory + FileDateTime.ToString("yyyyMMddHH") + ".CSV");
                    }
                    catch (Exception E)
                    {
                        //
                        // ���������� ��������� ��������� �� ������
                        //
                        lock (Console.Out)
                        {
                            Console.WriteLine("{0}: Error: an unhandled exception occurs when writing to file ({1}). A message that describes the reason for this is below.", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), Directory + FileDateTime.ToString("yyyyMMddHH") + ".CSV");
                            Console.WriteLine();
                            Console.WriteLine(E.ToString());
                            Console.WriteLine();
                            Console.WriteLine();
                        }
                    }

                    if (RawStream == null)
                    {
                        //
                        // ������� ����������� ��������
                        //
                        MustStop.WaitOne(15000, false);
                    }
                }
                else
                {
                    //
                    // ���������, �� ���� �� ������� ��� �����
                    //
                    if (FileDateTime.ToString("yyyyMMddHH") != DateTime.Now.ToString("yyyyMMddHH"))
                    {
                        //
                        // ��������� ����
                        //
                        RawStream.Close();

                        // ����������� �������
                        try
                        {
                            RawStream.Dispose();
                        }
                        finally
                        {
                            RawStream = null;
                        }
                    }
                    else
                    {
                        if (WriteQueue.Count > 0)
                        {
                            //
                            // ��������� ������ �� �������
                            //
                            string Value;

                            lock (WriteQueue.SyncRoot)
                            {
                                Value = WriteQueue.Dequeue().ToString();
                            }

                            //
                            // ��������� ������ � ����� ������, ���������� ������ ������ ��� �������������
                            //
                            if (Value.Length > Buffer.Length)
                            {
                                Buffer = MyEncoding.GetBytes(Value);
                            }
                            else
                            {
                                MyEncoding.GetBytes(Value, 0, Value.Length, Buffer, 0);
                            }

                            try
                            {
                                RawStream.Write(Buffer, 0, Value.Length);
                            }
                            catch (IOException E)
                            {
                                Console.WriteLine("{0}: Error: an I/O error occurs when writing to file ({1}). Another thread may have caused an unexpected change in the position of the operating system's file handle.", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), Directory + FileDateTime.ToString("yyyyMMddHH") + ".CSV");

                                //
                                // ���������� ������ ������� � �������
                                //
                                WriteQueue.Enqueue(Value);

                                //
                                // ���� ��������� �����, ����� �� �������� ���-����
                                //
                                MustStop.WaitOne(5000, false);
                            }
                            catch (ObjectDisposedException E)
                            {
                                Console.WriteLine("{0}: Warning: unable to write to file ({1}) because underlying stream is closed.", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), Directory + FileDateTime.ToString("yyyyMMddHH") + ".CSV");

                                //
                                // ���������� ������ ������� � �������
                                //
                                WriteQueue.Enqueue(Value);

                                //
                                // ���� ��������� �����, ����� �� �������� ���-����
                                //
                                MustStop.WaitOne(5000, false);
                            }
                            catch (Exception E)
                            {
                                //
                                // ���������� ��������� ��������� �� ������
                                //
                                lock (Console.Out)
                                {
                                    Console.WriteLine("{0}: Warning: an unhandled exception occurs when writing to file ({1}). A message that describes the reason for this is below.", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), Directory + FileDateTime.ToString("yyyyMMddHH") + ".CSV");
                                    Console.WriteLine();
                                    Console.WriteLine(E.ToString());
                                    Console.WriteLine();
                                    Console.WriteLine();
                                }

                                //
                                // ���������� ������ ������� � �������
                                //
                                WriteQueue.Enqueue(Value);

                                //
                                // ���� ��������� �����, ����� �� �������� ���-����
                                //
                                MustStop.WaitOne(5000, false);
                            }

                            //
                            // ���������� ������ �� ����
                            //
                            RawStream.Flush();
                        }
                        else
                        {
                            //
                            // ������� ��������� ������ � �������
                            //
                            MustStop.WaitOne(100, false);
                        }
                    }
                }
            }
        }
    }
}