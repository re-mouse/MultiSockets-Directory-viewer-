using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using static ServerV.BytesManipulator;

namespace ServerV
{
	class Server
	{
		private enum Command { Directory = 0, Download = 1, MakeDirectory = 2, Touch = 3}
		private const string defaultDownloadPath = "C:\\DownloadsTest\\";
		private static Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		private static List<Socket> clients = new List<Socket>();
		private const int port = 2000;
		static void Main(string[] args)
		{
			StartServer();
			try
			{
				StartLoop();
			}
			catch
			{
				
			}
		}
		private static void StartLoop()
		{
			while (true)
			{
				int id = ChooseAndShowId();
				Socket client = clients[id];
				while (true)
				{
					int commandtype = -1;
					string commandContext = "";
					string commandArgument;
					Console.Write("=========");
					Console.WriteLine($"Enter command:");
					while (commandtype == -1)
					{
						commandContext = Console.ReadLine();
						commandtype = ParseCommand(commandContext);
					}
					if (commandtype == -2)
						break;
					commandArgument = commandContext.Remove(0, commandContext.IndexOf(' ') + 1);
					SendInt(client, commandtype);
					switch (commandtype)  //ls показать директорию //dl скачать    mkdir  создать папку     touch закинуть свой файл туда-то 
					{
						case 0:
							{
								SendString(client, commandArgument);
								if (CheckForErrorsOrPrintIt(client) > 0)
								{
									Console.Clear();
									Console.WriteLine(RecieveString(client));
									Console.WriteLine("Last command succesfuly completed with code 1\n");
								}
							}
							break;
						case 1:
							{
								SendString(client, commandArgument);
								if (CheckForErrorsOrPrintIt(client) > 0)
								{
									Console.Clear();
									RecieveFile(client, defaultDownloadPath);
									Console.WriteLine("Last command succesfuly completed with code 1\n");
								}
							}
							break;
						case 2:
							{
								SendString(client, commandArgument);
								if (CheckForErrorsOrPrintIt(client) > 0)
								{
									Console.Clear();
									Console.WriteLine("Last command succesfuly completed with code 1\n");
								}
							}
							break;
						case 3:
							{
								string[] arguments = commandArgument.Split(' ');
								if (arguments.Length > 1 && File.Exists(defaultDownloadPath + arguments[1]))
								{
									SendString(client, arguments[0]);
									SendString(client, Path.GetFileName(defaultDownloadPath + arguments[1]));
									byte[] sendLength = BitConverter.GetBytes(new FileInfo(defaultDownloadPath + arguments[1]).Length);
									client.Send(sendLength, 0, 8, SocketFlags.None);
									client.SendFile(defaultDownloadPath + arguments[1]);
									if (CheckForErrorsOrPrintIt(client) > 0)
									{
										Console.Clear();
										Console.WriteLine("Last command succesfuly completed with code 1\n");
									}
								}
								else
									Console.WriteLine("Argument error or file does not exist");
							}
							break;
						case 4:
							{
								SendString(client, commandArgument);
								if (CheckForErrorsOrPrintIt(client) > 0)
								{
									Console.Clear();
									Console.WriteLine("Last command succesfuly completed with code 1\n");
								}
							}
							break;
						default:
							{
								Console.WriteLine("Unkown command type");
							}
							break;
					}
					System.Threading.Thread.Sleep(10);
				}
			}
		}
		private static int ParseCommand(string commandString) 
		{
			int commandId = -1;
			if (!commandString.Contains(' ') || commandString.Remove(0, commandString.IndexOf(' ') + 1).Length < 1)
			{
				Console.WriteLine("Empty argument command");
				return (-1);
			}
			string command = commandString.Remove(commandString.IndexOf(' '));
			switch (command)
			{
				case "snc":
					commandId = -2;
					break;
				case "ls":
					commandId = 0;
					break;

				case "dl":
					commandId = 1;
					break;

				case "mkdir":
					commandId = 2;
					break;

				case "touch":
					commandId = 3;
					break;

				case "kill":
					commandId = 4;
					break;

				default:
					Console.WriteLine("Command does not exist, select from ls / dl / mkdir / touch / snc (select another client)");
					break;
			}
			return (commandId);
		}
		private static int CheckForErrorsOrPrintIt(Socket client) 
		{
			int operationResult = RecieveInt(client);
			if (operationResult < 0) 
			{
				Console.WriteLine(RecieveString(client));
			}
			return (operationResult);
		}
		private static void StartServer()
		{
			Console.WriteLine("Starting server");
			socket.Bind(new IPEndPoint(IPAddress.Any, port));
			socket.Listen(10);
			socket.BeginAccept(AcceptClient, null);
			Console.WriteLine("Server started and can accept clients");
		}
		private static void AcceptClient(IAsyncResult AR) 
		{
			Socket client;
			try
			{
				client = socket.EndAccept(AR);
			}
			catch 
			{
				return;
			}
			if (!clients.Contains(client))
				clients.Add(client);
			Console.WriteLine($"Added {clients.Count} client to clients list");
			socket.BeginAccept(AcceptClient, null);
		}
		private static int ChooseAndShowId() 
		{
			for (int i = 0; i < clients.Count; i++)
			{
				if (clients[i].Connected)
				{
					IPEndPoint remoteIpEndPoint = clients[i].RemoteEndPoint as IPEndPoint;

					Console.WriteLine($"\nClient id {i} = {remoteIpEndPoint.Address} avalible");
				}
				else
				{
					Console.WriteLine($"\nClient id {i} disconnected");
					clients[i].Close();
					clients.Remove(clients[i]);
				}
			}
			int id = -1;
			while (id < 0 || id >= clients.Count)
			{
				Console.WriteLine("Select current id from avalible which operate with:");
				try
				{
					id = int.Parse(Console.ReadLine());
				}
                catch 
				{ 
					Console.WriteLine("Incorrect client id"); 
				}
			}
			return (id);
		}
	}
}