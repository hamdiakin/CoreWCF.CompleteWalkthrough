using System;
using System.Threading;
using System.Threading.Tasks;
using Common;
using NetMQ;
using NetMQ.Sockets;

namespace NetCoreClient
{
    public class InventoryNotificationSubscriber : IDisposable
    {
        private readonly SubscriberSocket _subSocket;
        private readonly string _address;
        private CancellationTokenSource _cts;
        private Task _listeningTask;

        public event Action<InventoryChangedMessage> OnInventoryChanged;

        public InventoryNotificationSubscriber(string address = "tcp://localhost:5556")
        {
            _address = address;
            _subSocket = new SubscriberSocket();
            _subSocket.Connect(_address);
        }

        public void Subscribe()
        {
            _subSocket.Subscribe("InventoryChanged");
            StartListening();
        }

        public void Unsubscribe()
        {
            _subSocket.Unsubscribe("InventoryChanged");
        }

        public void StartListening()
        {
            _cts = new CancellationTokenSource();
            _listeningTask = Task.Run(() => Listen(_cts.Token));
        }

        private void Listen(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var topic = _subSocket.ReceiveFrameString();
                    var json = _subSocket.ReceiveFrameString();
                    var message = Newtonsoft.Json.JsonConvert.DeserializeObject<InventoryChangedMessage>(json);
                    OnInventoryChanged?.Invoke(message);
                }
                catch (Exception ex)
                {
                    // Handle/log exception as needed
                }
            }
        }

        public void StopListening()
        {
            _cts?.Cancel();
            _listeningTask?.Wait();
        }

        public void Dispose()
        {
            StopListening();
            _subSocket?.Dispose();
        }
    }
}
