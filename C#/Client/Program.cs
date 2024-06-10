// A C# program for Client
//#define IP_MTU_DISCOVER

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client {

class Program {

// Main Method
static void Main(string[] args)
{
	ExecuteClient(args);
}

// ExecuteClient() Method
static void ExecuteClient(string[] args)
{
	string SERVER = "150.xxx.xxx.xx";
	int PORT = 8080;
	int BUFFER_SIZE = 1024;
	int MSG_SIZE = 6000;
	bool DONT_FRAGMENT = true;

	if (args.Length >= 1) {
		SERVER = args[0];
	}
	if (args.Length >= 2) {
		PORT = Int32.Parse(args[1]);
	}
	if (args.Length >= 3) {
		DONT_FRAGMENT = Boolean.Parse(args[2]);
	}	

	try {
		
		// Establish the remote endpoint for the socket.
		IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
        IPAddress ipAddr = System.Net.IPAddress.Parse(SERVER);
		IPEndPoint localEndPoint = new IPEndPoint(ipAddr, PORT);

		// Creation TCP/IP Socket using 
		// Socket Class Constructor
		Socket sender = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

		try {
			
			// Connect Socket to the remote 
			// endpoint using method Connect()
			sender.Connect(localEndPoint);

            // DontFragment
            sender.DontFragment = DONT_FRAGMENT;
			if (sender.DontFragment) {
				Console.WriteLine("DontFragment(DF) is enabled");
			}
            //sender.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.DontFragment, 1);

			// We print EndPoint information 
			// that we are connected
			Console.WriteLine("Socket connected to {0} ", sender.RemoteEndPoint.ToString());

			// Creation of message that
			// we will send to Server
			//byte[] messageSent = Encoding.ASCII.GetBytes("Test Client<EOF>");

			string message = MSG_SIZE.ToString();
			string paddedMessage = message.PadRight(MSG_SIZE - "<EOF>".Length, '_');

			byte[] messageSent = Encoding.ASCII.GetBytes(paddedMessage + "<EOF>");
			int byteSent = sender.Send(messageSent);

			Console.WriteLine("Sent: {0} bytes", byteSent);
			Console.WriteLine(Encoding.Default.GetString(messageSent));

			// Data buffer
			byte[] messageReceived = new byte[BUFFER_SIZE];

			// We receive the message using 
			// the method Receive(). This 
			// method returns number of bytes
			// received, that we'll use to 
			// convert them to string
			int byteRecv = sender.Receive(messageReceived);
			Console.WriteLine("Message from Server: {0}", 
				Encoding.ASCII.GetString(messageReceived, 0, byteRecv));

			// Close Socket using 
			// the method Close()
			sender.Shutdown(SocketShutdown.Both);
			sender.Close();
		}
		
		// Manage of Socket's Exceptions
		catch (ArgumentNullException ane) {
			Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
		}
		
		catch (SocketException se) {
			Console.WriteLine("SocketException : {0}", se.ToString());
		}
		
		catch (Exception e) {
			Console.WriteLine("Unexpected exception : {0}", e.ToString());
		}
	}
	
	catch (Exception e) {
		Console.WriteLine(e.ToString());
	}
}

}
}
