// A C# Program for Server
#define IP_MTU_DISCOVER

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
 
namespace Server {
 
class Program {
 
// Main Method
static void Main(string[] args)
{
    ExecuteServer(args);
}
 
public static void ExecuteServer(string[] args)
{
    int PORT = 8080;
    int BUFFER_SIZE = 1024;
    bool DONT_FRAGMENT = false;

    if (args.Length >= 1) {
        PORT = Int32.Parse(args[0]);
    }
    if (args.Length >= 2) {
        DONT_FRAGMENT = Boolean.Parse(args[1]);
    }	    

    // Establish the local endpoint for the socket.
    // Dns.GetHostName returns the name of the host running the application.
    IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
    IPAddress ipAddr = ipHost.AddressList[1];
    IPEndPoint localEndPoint = new IPEndPoint(ipAddr, PORT);

    // Creation TCP/IP Socket using 
    // Socket Class Constructor
    Socket listener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

    try {
        // Using Bind() method we associate a
        // network address to the Server Socket
        // All client that will connect to this 
        // Server Socket must know this network
        // Address
        listener.Bind(localEndPoint);

        // Using Listen() method we create 
        // the Client list that will want
        // to connect to Server
        listener.Listen(10);

        while (true) {

            Console.WriteLine("Listening on " + PORT);

            // Suspend while waiting for
            // incoming connection Using 
            // Accept() method the server 
            // will accept connection of client
            Socket clientSocket = listener.Accept();

            // DontFragment
            clientSocket.DontFragment = DONT_FRAGMENT;
            if (clientSocket.DontFragment) {
                Console.WriteLine("DontFragment(DF) is enabled");
            }

            // Data buffer
            byte[] bytes = new Byte[BUFFER_SIZE];
            string data = "";

            while (true) {

                int numByte = clientSocket.Receive(bytes);
                    
                data += Encoding.ASCII.GetString(bytes, 0, numByte);
                                            
                if (data.IndexOf("<EOF>") > -1)
                    break;
            }

            Console.WriteLine("[{0}] Received: {1} ", clientSocket.RemoteEndPoint, data);
            byte[] message = Encoding.ASCII.GetBytes(Encoding.ASCII.GetBytes(data).Length + " bytes received");            

            // Send a message to Client 
            // using Send() method
            clientSocket.Send(message);

            Console.WriteLine("[{0}] Sent: {1} ", clientSocket.RemoteEndPoint, Encoding.ASCII.GetString(message));

            // Close client Socket using the
            // Close() method. After closing,
            // we can use the closed Socket 
            // for a new Client Connection
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();

            Console.WriteLine("");
        }
    }
        
    catch (Exception e) {
        Console.WriteLine(e.ToString());
    }
}
}
}