using System;
using Common;
using NetMQ;
using NetMQ.Sockets;

namespace NetCoreServer
{
    public class InventoryNotificationPublisher : IDisposable
    {
        private readonly PublisherSocket _pubSocket;
        private readonly string _address;

        public InventoryNotificationPublisher(string address = "tcp://*:5556")
        {
            _address = address;
            _pubSocket = new PublisherSocket();
            _pubSocket.Bind(_address);
        }

        public void PublishInventoryChanged(InventoryChangedMessage message)
        {
            // Serialize message as JSON
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(message);
            _pubSocket.SendMoreFrame("InventoryChanged").SendFrame(json);
        }

        public void Dispose()
        {
            _pubSocket?.Dispose();
        }
    }
}
