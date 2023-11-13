using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using DTOs_OnlineMessenger2020;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Server_OnlineMessenger2020
{
    class Server
    {
        private static Socket listnerSocket;
        public static int clientCounter = 0;
        public static List<Socket> clientSocketList = new List<Socket>();
        public static string lastMessage;

        static void Main(string[] args)
        {
            initialiseSocket();
            Console.WriteLine("listening");
            while (true)
                listen();
        }

        private static void initialiseSocket()
        {
            Console.WriteLine("Enter Server IP:");
            string serverIP = Console.ReadLine();
            Console.WriteLine("Enter Port");
            int port = Int32.Parse(Console.ReadLine());

            listnerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listnerSocket.Bind(new IPEndPoint(IPAddress.Parse(serverIP), port));
        }

        private static void listen()
        {
            listnerSocket.Listen(500);
            Socket clientSocket = listnerSocket.Accept();
            Console.WriteLine("Client Connected");

            clientSocketList.Add(clientSocket);

            clientCounter++;
            Thread clientThread = new Thread(() => recieveData(clientSocket,clientCounter));
            clientThread.Start();
        }

        private static void recieveData(Socket clientSocket, int clientNo)
        {
            int bufferSize;
            Variables variables = new Variables();

            do
            {
                byte[] buffer = new byte[clientSocket.SendBufferSize];
                bufferSize = clientSocket.Receive(buffer);

                if (bufferSize > 0)
                {
                    DTOs dto = BytetoDTO(buffer, bufferSize);
                    variables.handleData(dto.data, dto.variableIndex, clientSocket);
                }
                else
                    break;

            } while (bufferSize > 0);
            Console.WriteLine("Client Disconnected");
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

    class Variables
    {
        private string username;

        public void handleData(byte[] data, int variableIndex,Socket clientSocket)
        {
            switch (variableIndex)
            {
                case 0:
                    Server.lastMessage = Encoding.UTF8.GetString(data);
                    Console.WriteLine(username + ": " + Server.lastMessage);
                    piggybackData(1, Encoding.UTF8.GetBytes(username));
                    piggybackData(variableIndex, data, clientSocket);
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

        private static void piggybackData(int variableIndex, byte[] data, Socket clientSocket)
        {
            foreach (Socket socket in Server.clientSocketList)
            {
                if (socket != clientSocket)
                    socket.Send(DTOtoByte(new DTOs(variableIndex, data)));
            }
        }

        private static void piggybackData(int variableIndex, byte[] data)
        {
            foreach (Socket socket in Server.clientSocketList)
                socket.Send(DTOtoByte(new DTOs(variableIndex, data)));
        }
    }
}  