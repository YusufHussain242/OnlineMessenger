using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using DTOs_OnlineMessenger2020;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Threading;

namespace Client_OnlineMessenger2020
{
    class Client
    {
        private static Socket serverSocket;

        static void Main(string[] args)
        {
            initialiseSocket();
            sendMessage();
        }

        private static void initialiseSocket()
        {
            Console.WriteLine("Enter Server IP:");
            string serverIP = Console.ReadLine();
            Console.WriteLine("Enter Port");
            int port = Int32.Parse(Console.ReadLine());

            Console.Write("Enter username: ");
            String username = Console.ReadLine();

            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Connect(new IPEndPoint(IPAddress.Parse(serverIP), port));
            serverSocket.Send(DTOtoByte(new DTOs(1, Encoding.UTF8.GetBytes(username))));

            Thread recieveThread = new Thread(() => recieveData(serverSocket));
            recieveThread.Start();
        }

        private static void sendMessage()
        {
            string messageData = Console.ReadLine();
            while (messageData != "")
            {
                serverSocket.Send(DTOtoByte(new DTOs(0, Encoding.UTF8.GetBytes(messageData))));
                messageData = Console.ReadLine();
            }

            serverSocket.Close();
        }

        public static byte[] DTOtoByte(DTOs dto)
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            MemoryStream memoryStream = new MemoryStream();
            binaryFormatter.Serialize(memoryStream, dto);
            return memoryStream.ToArray();
        }

        private static void recieveData(Socket serverSocket)
        {
            int bufferSize;
            ClientVariables variables = new ClientVariables();

            do
            {
                byte[] buffer = new byte[serverSocket.SendBufferSize];
                bufferSize = serverSocket.Receive(buffer);

                if (bufferSize > 0)
                {
                    DTOs dto = BytetoDTO(buffer, bufferSize);
                    variables.handleData(dto.data, dto.variableIndex);
                }
                else
                    break;

            } while (bufferSize > 0);
            Console.WriteLine("Lost Connection To Server");
        }

        private static DTOs BytetoDTO(byte[] buffer, int bufferSize)
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            MemoryStream memoryStream = new MemoryStream();
            memoryStream.Write(buffer, 0, bufferSize);
            memoryStream.Seek(0, SeekOrigin.Begin);
            object tempObj = (object)binaryFormatter.Deserialize(memoryStream);
            return (DTOs)tempObj;
        }
    }

    class ClientVariables
    {
        private string username = "Test: ";
        private string message;

        public void handleData(byte[] data, int variableIndex)
        {
            switch (variableIndex)
            {
                case 0:
                    message = Encoding.UTF8.GetString(data);
                    Console.WriteLine(username + ": " + message);
                    break;

                case 1:
                    username = Encoding.UTF8.GetString(data);
                    break;
            }
        }

        private static byte[] DTOtoByte(DTOs dto)
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            MemoryStream memoryStream = new MemoryStream();
            binaryFormatter.Serialize(memoryStream, dto);
            return memoryStream.ToArray();
        }
    }
}
