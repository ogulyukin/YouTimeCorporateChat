using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.IO;

namespace ChatClient
{
    public class ServerConnector
    { 
        private TcpClient client;
        SslStream sslStream;
        private static Hashtable _certificateErrors = new Hashtable();

        public string ConnectToServer(string ip, int port)
        {
            try
            {
                var ipServer = new IPEndPoint(IPAddress.Parse(ip), port);
                client = new();
                client.Connect(ipServer);
            }
            catch (Exception exc)
            {
                return exc.Message;
            }

            try
            {
                sslStream = new SslStream(client.GetStream(), true, ValidateServerCertificate, null);
                //sslStream = new SslStream(client.GetStream(), false);
                sslStream.AuthenticateAsClient("Host name");
            }catch(Exception exc)
            {
                client.Close();
                if (exc.InnerException != null)
                {
                    return $"Inner exception: {exc.InnerException.Message}";
                }
                return exc.Message;
            }

            return "Соединение установлено";
        }

        private bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        public string SendMessage(string message)
        {
            //connection.Send(Encoding.Unicode.GetBytes(message));
            sslStream.WriteAsync(Encoding.Unicode.GetBytes(message));

            var buffer = new byte[256];
            var data = new List<byte>();
            do
            {
                sslStream.Read(buffer);
                data.AddRange(buffer);
            } while (client.Available > 0);

            byte[] dataArray = data.ToArray();
                
            return Encoding.Unicode.GetString(dataArray, 0, dataArray.Length);
        }

        public bool isConnectedToServer()
        {
            return client != null && client.Connected;
        }

        private void CloseConnection()
        {
            client.Close();
        }

    }
}
