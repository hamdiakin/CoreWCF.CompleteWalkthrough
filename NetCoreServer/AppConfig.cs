namespace ZmqServer
{
    public static class AppConfig
    {
        public const int TcpPort = 8088;
        public const int SecureTcpPort = 8443;  // For future TLS implementation
        public const string TcpEndpoint = "tcp://*:8088";
        public const string InprocEndpoint = "inproc://echo-service";
        public const string IpcEndpoint = "ipc://echo-service.ipc";
        
        // Client connection endpoints
        public const string TcpClientEndpoint = "tcp://localhost:8088";
        public const string InprocClientEndpoint = "inproc://echo-service";
        public const string IpcClientEndpoint = "ipc://echo-service.ipc";
    }
}