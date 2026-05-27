using System;
using System.Text;
using System.Threading;

namespace RDPKeepAlive
{
    internal static class Program
    {
        private const string MutexName = "RDPKeepAliveMutex";
        private const int Interval = 60;
        private static bool _verbose;

        public static void Main(string[] args)
        {
            using (Mutex mutex = new Mutex(false, MutexName))
            {
                if (!mutex.WaitOne(0))
                {
                    Console.WriteLine("2nd instance");
                    ExitGracefully();
                }

                if (args.Length > 0 && args[0].Equals("-v"))
                {
                    _verbose = true;
                }

                Console.OutputEncoding = Encoding.UTF8;

                Console.CancelKeyPress += OnCancelKeyPress;

                Console.WriteLine("RDPKeepAlive - Zafer Balkan, (c) 2025");
                Console.WriteLine("Simulating RDP activity.");
                Console.WriteLine("Press CTRL+C to stop...");
                Console.WriteLine();

                while (true)
                {
                    bool previousValue = false;

                    for (int i = 0; i < Interval; i++)
                    {
                        Client client;
                        bool isFound = KeepAlive.TryGetRDPClient(out client);

                        if (!isFound)
                        {
                            Console.WriteLine("No RDP client found. Exiting...");
                            ExitGracefully();
                        }

                        if (!previousValue)
                        {
                            previousValue = isFound;

                            if (_verbose)
                            {
                                Console.WriteLine(string.Format("{0:o} - Found RDP client.", DateTime.Now));
                                Console.WriteLine(string.Format("\t* Window: {0}", client.WindowTitle));
                                Console.WriteLine(string.Format("\t* Class : {0}", client.ClassName));
                            }

                            KeepAlive.Execute();

                            if (_verbose)
                            {
                                Console.WriteLine(string.Format("{0:o} - Mouse movement is sent.", DateTime.Now));
                            }
                        }

                        Thread.Sleep(1000);
                    }
                }
            }
        }

        private static void ExitGracefully()
        {
            Console.WriteLine("RDPKeepAlive terminated gracefully.");
            Environment.Exit(0);
        }

        private static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            ExitGracefully();
        }
    }
}
