using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Senior_Backend_Task_1
{
    internal class DiscountClient
    {
        public static async Task TestClient()
        {
            try
            {
                // Connect to the server
                TcpClient client = new TcpClient("127.0.0.1", 8888);
                NetworkStream stream = client.GetStream();
                Console.WriteLine("Connected to server.");

                bool loop = true;
                while (loop)
                {
                    Console.WriteLine("Choose an option:");
                    Console.WriteLine("1. GENERATE");
                    Console.WriteLine("2. USE");
                    Console.WriteLine("3. Exit");

                    string? userInput = Console.ReadLine();

                    switch (userInput)
                    {
                        case "1":
                            await SendRequest(stream, "GENERATE");// Regenerate Discount Code
                            // Receive responses
                            string response = await ReceiveResponse(stream);
                            Console.WriteLine("Response 1: " + response);
                            break;

                        case "2":
                            Console.Write("Check your Discount Code: ");
                            string? discountCode = Console.ReadLine();

                            await SendRequest(stream, $"USE {discountCode}"); // Try to use a discount code
                            // Receive responses
                            string response1 = await ReceiveResponse(stream);
                            Console.WriteLine("Response 1: " + response1);
                            break;

                        default:
                            // Close connection
                            stream.Close();
                            client.Close();
                            loop = false;
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception: {e}");
            }
        }

        private static async Task SendRequest(NetworkStream stream, string request)
        {
            byte[] requestData = Encoding.ASCII.GetBytes(request);
            await stream.WriteAsync(requestData, 0, requestData.Length);
            Console.WriteLine("Sent request: " + request);
        }

        private static async Task<string> ReceiveResponse(NetworkStream stream)
        {
            byte[] buffer = new byte[1024];
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            return Encoding.ASCII.GetString(buffer, 0, bytesRead);
        }
    }
}
