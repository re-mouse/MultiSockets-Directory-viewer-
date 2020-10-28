using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace ServerV
{
    class BytesManipulator
    {
		public static int RecieveInt(Socket client)
		{
			byte[] bufferLength = new byte[4];
			int readed = client.Receive(bufferLength, 0, bufferLength.Length, SocketFlags.None);
			int returnInt = BitConverter.ToInt32(bufferLength);
			return (returnInt);
		}
		public static long RecieveLong(Socket client)
		{
			byte[] bufferLong = new byte[8];
			client.Receive(bufferLong, 0, bufferLong.Length, SocketFlags.None);
			long returnLong = BitConverter.ToInt32(bufferLong);
			return (returnLong);
		}
		public static string RecieveString(Socket client)
		{
			int length = RecieveInt(client);
			byte[] gettedStringBuffer = new byte[length];
			int readed = client.Receive(gettedStringBuffer, 0, gettedStringBuffer.Length, SocketFlags.None);
			string returnString = Encoding.UTF8.GetString(gettedStringBuffer);
			return (returnString);
		}
		public static void SendString(Socket client, string toSend)
		{
			byte[] sendBytes = Encoding.UTF8.GetBytes(toSend);
			SendInt(client, sendBytes.Length);
			client.Send(sendBytes, 0, sendBytes.Length, SocketFlags.None);
		}
		public static void SendInt(Socket client, int intToSend)
		{
			byte[] sendLength = BitConverter.GetBytes(intToSend);
			client.Send(sendLength, 0, 4, SocketFlags.None);
		}
		public static void SendLong(Socket client, long longToSend)
		{
			byte[] sendLength = BitConverter.GetBytes(longToSend);
			client.Send(sendLength, 0, 8, SocketFlags.None);
		}
		public static void RecieveFile(Socket client, string path)
		{
			string fileName = RecieveString(client);

			long lengthFileBytes = RecieveLong(client);

			byte[] gettedFileBuffer = new byte[lengthFileBytes];
			byte[] sumFileBuffer = new byte[lengthFileBytes];
			int reciv = 0;
			int recivTemp = reciv;
			while (reciv < lengthFileBytes)
			{
				int currentRecieved = client.Receive(gettedFileBuffer, 0, gettedFileBuffer.Length, SocketFlags.None);
				reciv += currentRecieved;
				Array.Copy(gettedFileBuffer, 0, sumFileBuffer, recivTemp, currentRecieved);
				recivTemp = reciv;
			}
			File.WriteAllBytes(path + fileName, sumFileBuffer);

			Console.WriteLine($"Downloaded {fileName} at {path + fileName}");
		}
	}
}
