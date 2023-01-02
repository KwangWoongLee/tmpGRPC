using Grpc.Core;
using System;

namespace GrpcService
{
    class Program
    {
        const int Port = 5001;
        public static void Main(string[] args)
        {
            try
            {
                Server server = new Server
                {
                    Services = { MatchService.BindService(new MatchImpl()) },
                    Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
                };
                server.Start();


                Console.WriteLine("Accounts server listening on port " + Port);
                Console.WriteLine("Press any key to stop the server...");
                Console.ReadKey();


                server.ShutdownAsync().Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception encountered: {ex}");
            }        }
    }
}
