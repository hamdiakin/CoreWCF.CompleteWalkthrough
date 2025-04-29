namespace NetCoreServer
{
    public static class AppConfig
    {
        public const int HttpPort = 8088;
        public const int HttpsPort = 8443;
        public const int NetTcpPort = 8089;
        public const string HostInWsdl = "localhost";

        // Service paths
        public const string EchoServicePath = "/EchoService";
        public const string BasicHttpEndpoint = "/basichttp";
        public const string WsHttpEndpoint = "/wsHttp";
        public const string NetTcpEndpoint = "/netTcp";
    }
}