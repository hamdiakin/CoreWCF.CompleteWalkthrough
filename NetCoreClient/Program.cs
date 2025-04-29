using Common;
using System.ServiceModel;

namespace NetCoreClient
{
    public class Program
    {
        public const int HTTP_PORT = 8088;
        public const int HTTPS_PORT = 8443;
        public const int NETTCP_PORT = 8089;

        static async Task Main(string[] args)
        {
            Console.Title = "WCF .Net Core Client";

            //// Add this before making any HTTPS calls - only for development environments!
            //System.Net.ServicePointManager.ServerCertificateValidationCallback =
            //    ((sender, certificate, chain, sslPolicyErrors) => true);

            try
            {
                await CallBasicHttpBindingAsync($"http://localhost:{HTTP_PORT}");
                await CallBasicHttpBindingAsync($"https://localhost:{HTTPS_PORT}");
                await CallWsHttpBindingAsync($"http://localhost:{HTTP_PORT}");
                //await CallWsHttpBindingAsync($"https://localhost:{HTTPS_PORT}");
                await CallNetTcpBindingAsync($"net.tcp://localhost:{NETTCP_PORT}");

                Console.WriteLine("Press any key to exit");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine(ex.ToString());
                Console.ReadKey();
            }
        }

        private static async Task CallBasicHttpBindingAsync(string hostAddr)
        {
            Console.WriteLine($"Calling service with BasicHttpBinding at {hostAddr}");

            var binding = new BasicHttpBinding(IsHttps(hostAddr) ? BasicHttpSecurityMode.Transport : BasicHttpSecurityMode.None);
            binding.OpenTimeout = TimeSpan.FromMinutes(1);
            binding.CloseTimeout = TimeSpan.FromMinutes(1);
            binding.SendTimeout = TimeSpan.FromMinutes(10);
            binding.ReceiveTimeout = TimeSpan.FromMinutes(10);

            var endpointAddress = new EndpointAddress($"{hostAddr}/EchoService/basichttp");

            using (var factory = new ChannelFactory<IEchoService>(binding, endpointAddress))
            {
                IEchoService client = factory.CreateChannel();

                using (var channel = client as IClientChannel)
                {
                    try
                    {
                        channel?.Open();
                        var result = await Task.FromResult(client.Echo("Hello World via BasicHttp!"));
                        Console.WriteLine($"Response: {result}");
                    }
                    finally
                    {
                        if (channel?.State == CommunicationState.Opened)
                        {
                            channel?.Close();
                        }
                        else
                        {
                            channel?.Abort();
                        }
                    }
                }
            }
        }

        private static async Task CallWsHttpBindingAsync(string hostAddr)
        {
            Console.WriteLine($"Calling service with WSHttpBinding at {hostAddr}");

            var binding = new WSHttpBinding(IsHttps(hostAddr) ? SecurityMode.Transport : SecurityMode.None);

            // Configure the binding for HTTPS
            if (IsHttps(hostAddr))
            {
                binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
                binding.Security.Message.ClientCredentialType = MessageCredentialType.None;
                binding.Security.Message.NegotiateServiceCredential = false;
            }

            binding.OpenTimeout = TimeSpan.FromMinutes(1);
            binding.CloseTimeout = TimeSpan.FromMinutes(1);
            binding.SendTimeout = TimeSpan.FromMinutes(10);
            binding.ReceiveTimeout = TimeSpan.FromMinutes(10);

            // Make sure the case matches exactly what's on the server
            var endpointAddress = new EndpointAddress($"{hostAddr}/EchoService/wsHttp");

            try
            {
                using (var factory = new ChannelFactory<IEchoService>(binding, endpointAddress))
                {
                    IEchoService client = factory.CreateChannel();

                    using (var channel = client as IClientChannel)
                    {
                        try
                        {
                            channel?.Open();
                            var result = await Task.FromResult(client.Echo("Hello World via WSHttp!"));
                            Console.WriteLine($"Response: {result}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error in channel: {ex.Message}");
                            channel?.Abort();
                            throw;
                        }
                        finally
                        {
                            if (channel?.State == CommunicationState.Opened)
                            {
                                channel?.Close();
                            }
                            else
                            {
                                channel?.Abort();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WSHttpBinding failed: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
            }
        }

        private static async Task CallNetTcpBindingAsync(string hostAddr)
        {
            Console.WriteLine($"Calling service with NetTcpBinding at {hostAddr}");

            var binding = new NetTcpBinding();
            binding.OpenTimeout = TimeSpan.FromMinutes(1);
            binding.CloseTimeout = TimeSpan.FromMinutes(1);
            binding.SendTimeout = TimeSpan.FromMinutes(10);
            binding.ReceiveTimeout = TimeSpan.FromMinutes(10);

            var endpointAddress = new EndpointAddress($"{hostAddr}/netTcp");

            using (var factory = new ChannelFactory<IEchoService>(binding, endpointAddress))
            {
                IEchoService client = factory.CreateChannel();

                using (var channel = client as IClientChannel)
                {
                    try
                    {
                        channel?.Open();
                        var result = await Task.FromResult(client.Echo("Hello World via NetTcp!"));
                        Console.WriteLine($"Response: {result}");
                    }
                    finally
                    {
                        if (channel?.State == CommunicationState.Opened)
                        {
                            channel?.Close();
                        }
                        else
                        {
                            channel?.Abort();
                        }
                    }
                }
            }
        }

        private static bool IsHttps(string url)
        {
            return url.ToLower().StartsWith("https://");
        }
    }
}