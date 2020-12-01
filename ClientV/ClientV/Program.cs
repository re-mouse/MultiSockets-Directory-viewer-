using System;
using System.Net.Sockets;
using System.Text;
using System.IO;
using static Client.BytesManipulator;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Client
{
	class Client
	{
		private static Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		private static string ip = "127.0.0.1";
		private static string DEFAULT_PATH = "C:\\kavo";
		private static int port = 2000;
		private static string command;
		
		static void Main(string[] args)
		{
			ConnectServer();
			while (true)
			{
				try 
				{
				int commandId = RecieveInt(socket);
				command = RecieveString(socket);
				Console.WriteLine($"Got command: {command} and id = {commandId}");
				SendBytes(commandId);
				System.Threading.Thread.Sleep(20);
				}
				catch 
				{
					socket.Close(2);
					ConnectServer();
				}
			}
		}

		private static void ConnectServer()
		{
			while (!socket.Connected)
				try
				{
					socket.Connect(ip, port);
				}
				catch { }
		}
		private static void SetupProgram()
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				DEFAULT_PATH = "C:\\stradai";
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
				DEFAULT_PATH = "/home/";
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
				DEFAULT_PATH = "/User/$USER";
		}
		private static void SendBytes(int commandId) 
		{
			switch (commandId) 
			{
				case 0:
					if (Directory.Exists(command))
					{
						try
						{
							SendInt(socket, 1);
							SendDirectory(socket);
						}
						catch (SocketException a)
						{
							SendError("Sending dirrectory error: " + a.Message);
						}
					}
					else
						SendError("Directory does not exist");
					break;
				case 1:
					if (File.Exists(command))
					{
						try
						{
							SendInt(socket, 1);
							SendString(socket, Path.GetFileName(command));
							byte[] sendLength = BitConverter.GetBytes(new FileInfo(command).Length);
							socket.Send(sendLength, 0, 8, SocketFlags.None);
							socket.SendFile(command);
						}
						catch (SocketException a)
						{
							SendError("Sending file error: " + a.Message);
						}
					}
					else
						SendError("File does not exist");
					break;
				case 2:
					if (!Directory.Exists(command))
					{
						try
						{
							SendInt(socket, 1);
							Directory.CreateDirectory(command);
						}
						catch (SocketException a)
						{
							SendError("Sending file error: " + a.Message);
						}
					}
					else
						SendError("Directory already exist");
					break;
				case 3:
					try
					{
						command += "\\";
						if (!Directory.Exists(command))
							try
							{
								Directory.CreateDirectory(command);
							}
							catch 
							{
								
							}
						DownloadFile(socket, command);
						SendInt(socket, 1);
					}
					catch (SocketException a)
					{
						SendError("Recieving file error: " + a.Message);
					}
					break;
				case 4:
					try 
					{
						foreach (var process in Process.GetProcessesByName(command))
						{
							process.Kill();
						}
						SendInt(socket, 1);
					}
					catch (SocketException a)
					{
						SendError("Sending file error: " + a.Message);
					}
					break;
			} 
		}
		private static void SendDirectory(Socket server) 
		{
			string toSend = "";
			string[] fileEntries = Directory.GetFiles(command);
			foreach (string fileName in fileEntries)
			{
				string temp = fileName;
				temp += "\n";
				toSend += temp;
			}
			string[] subdirectoryEntries = Directory.GetDirectories(command);
			foreach (string dir in subdirectoryEntries)
			{
				string temp = dir;
				temp += "\n";
				toSend += temp;
			}
			SendString(socket, toSend);
		}

		private static void SendError(string error) 
		{
			SendInt(socket, -1);
			SendString(socket, "Error: " + error);
		}
	}
}
