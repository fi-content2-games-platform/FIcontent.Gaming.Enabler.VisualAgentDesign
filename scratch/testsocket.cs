using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class SynchronousSocketClient {
	
	protected static byte[] ReceiveAll(Socket socket, int count)
	{
		byte[] buffer = new byte[count];
		int done = 0;
		int left = count;
		while (left > 0)
		{
			int recvCount = socket.Receive(buffer,done,left,SocketFlags.None);
			done += recvCount;
			left -= recvCount;
		}
		return buffer;
	}
	
	protected static ushort ReceiveUInt16LE(Socket socket)
	{
		byte[] buffer = ReceiveAll(socket, 2);
		if (!BitConverter.IsLittleEndian)
			Array.Reverse(buffer); 
		return BitConverter.ToUInt16(buffer, 0);
	}
	
	/*protected static ushort[] ReceiveUInt16ArrayLE(Socket socket, int len)
	{
		ushort[] array = new ushort[len];
		for (int i=0; i<len; ++i)
			array[i] = ReceiveUInt16LE(socket);
		return array;
	}*/

	public static void StartClient()
	{
		// Connect to a remote device.
		try
		{
			// Establish the remote endpoint for the socket.
			// This example uses port 33333 on the local computer.
			IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
			IPAddress ipAddress = ipHostInfo.AddressList[0];
			IPEndPoint remoteEP = new IPEndPoint(ipAddress,33333);

			// Create a TCP/IP  socket.
			Socket socket = new Socket(AddressFamily.InterNetwork, 
				SocketType.Stream, ProtocolType.Tcp );

			// Connect the socket to the remote endpoint. Catch any errors.
			try
			{
				socket.Connect(remoteEP);

				Console.WriteLine("Socket connected to {0}", socket.RemoteEndPoint.ToString());
				
				// note: the connected status is only updated on read, so most likely this program will not realise the server disconnected
				while (socket.Connected)
				{
					// look if there is some pending data
					bool isData = socket.Poll(0,SelectMode.SelectRead);
					if (isData)
					{
						// receive an Aseba message
						ushort len = ReceiveUInt16LE(socket);
						ushort source = ReceiveUInt16LE(socket);
						ushort type = ReceiveUInt16LE(socket);
						byte[] payload = ReceiveAll(socket, len);
						Console.WriteLine(String.Format("Received message from {0} of type 0x{1:X4}, size {2} : {3}", source, type, len, String.Join(", ", payload)));
					}
					
					// TODO: move the above into a Unity update step and discard sleep
					Thread.Sleep(100);
				}
				
				// Release the socket.
				socket.Shutdown(SocketShutdown.Both);
				socket.Close();
				
			} catch (ArgumentNullException ane) {
				Console.WriteLine("ArgumentNullException : {0}",ane.ToString());
			} catch (SocketException se) {
				Console.WriteLine("SocketException : {0}",se.ToString());
			} catch (Exception e) {
				Console.WriteLine("Unexpected exception : {0}", e.ToString());
			}
		}
		catch (Exception e)
		{
			Console.WriteLine( e.ToString());
		}
	}
	
	public static int Main(String[] args)
	{
		StartClient();
		return 0;
	}
}