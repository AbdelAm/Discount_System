using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Senior_Backend_Task_1_Server
{
    internal class DiscountServer
    {
        private const int PORT = 8888;
        private const string StorageFilePath = "discount_codes.txt";
        private static HashSet<string> discountCodes = new HashSet<string>();

        public static async Task InitServer()
        {
            TcpListener? server = null;
            try
            {
                IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
                server = new TcpListener(ipAddress, PORT);
                server.Start();
                Console.WriteLine($"Server started, listening on port {PORT}...");
                LoadDiscountCodes();
                while (true)
                {
                    TcpClient client = await server.AcceptTcpClientAsync();
                    Console.WriteLine("Connected!");

                    _ = HandleClientAsync(client);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception: {e}");
            }
            finally
            {
                // Stop listening for new clients
                server?.Stop();
            }
        }
        private static void LoadDiscountCodes()
        {
            if (File.Exists(StorageFilePath))
            {
                LoadCodesFromFile();
                Console.WriteLine($"Loaded {discountCodes.Count} discount codes from file.");
            } else
            {
                bool generated = GenerateDiscountCodes();
                string message = generated ? "Discount Codes are Generated Successfully" : "There was a problem with generating discount codes";
                Console.WriteLine(message);
            }
        }
        private static bool LoadCodesFromFile()
        {
            string[] storedCodes = File.ReadAllLines(StorageFilePath);
            discountCodes = new HashSet<string>(storedCodes);
            return true;
        }
        private static bool SaveCodesToFile()
        {
            File.WriteAllLines(StorageFilePath, discountCodes);
            return true;
        }
        private static async Task HandleClientAsync(TcpClient client)
        {
            try
            {
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[1024];
                int bytesRead;

                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    string requestData = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"Received request from {((IPEndPoint)client.Client.RemoteEndPoint).Address}: {requestData}");

                    // Process client request and generate response
                    string response = ProcessRequest(requestData);

                    // Send response back to client
                    byte[] responseData = Encoding.ASCII.GetBytes(response);
                    await stream.WriteAsync(responseData, 0, responseData.Length);
                    Console.WriteLine($"Sent response to {((IPEndPoint)client.Client.RemoteEndPoint).Address}: {response}");
                }
                // Close client connection
                client.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception: {e}");
            }
          
        }

        private static string ProcessRequest(string request)
        {
            string[] parts = request.Split(' ');

            if (parts.Length == 1 && parts[0] == "GENERATE")
            {
                bool generated = GenerateDiscountCodes();
                return generated ? "Discount Codes are Regenerated Successfully" : "There was a problem with generating discount codes";
            }
            else if (parts.Length == 2 && parts[0] == "USE")
            {
                string code = parts[1];
                byte result = UseDiscountCode(code);
                return result == 1 ? "Your Discount Code is Valid" : "Your Discount Code is not Valid";
            }
            else
            {
                return "INVALID_REQUEST";
            }
        }

        private static bool GenerateDiscountCodes()
        {
            Random random = new Random();
            for (int i = 0; i < 1000; i++)
            {
                string code = GenerateRandomCode(random);
                discountCodes.Add(code);
            }
            return SaveCodesToFile();
            
        }

        private static string GenerateRandomCode(Random random)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            StringBuilder code = new StringBuilder();
            int length = random.Next(7, 8);
            for (int i = 0; i < length; i++)
            {
                code.Append(chars[random.Next(chars.Length)]);
            }
            return code.ToString();
        }

        private static byte UseDiscountCode(string code)
        {
            if (discountCodes.Contains(code))
            {
                discountCodes.Remove(code);
                if (discountCodes.Count != 0)
                {
                    SaveCodesToFile(); // Save codes to file after usage
                } else
                {
                    File.Delete(StorageFilePath);
                }
                return 1; // Success
            }
            else
            {
                return 0; // Failure
            }
        }
    }
}
