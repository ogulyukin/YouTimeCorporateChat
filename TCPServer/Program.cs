using System;

namespace TCPServer
{
    class Program
    {
        static void PrintMessages(string msg)
        {
            Console.WriteLine(msg);
        }

        static void Main(string[] args)
        {
            Starter starter = new();
            starter.StartServer();
        }
    }
}
