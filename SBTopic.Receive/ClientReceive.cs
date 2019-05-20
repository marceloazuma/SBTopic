using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using SBTopic.Model;

namespace SBTopic.Receive
{
    public class ClientReceive
    {
        private int _ClientId;
        private Random _Random;

        private ConcurrentQueue<SBMessage> _ClientReceiveQueue = new ConcurrentQueue<SBMessage>();

        public ClientReceive(int clientId)
        {
            this._ClientId = clientId;
            _Random = new Random(_ClientId);

            SBReceive.SBEvent += SBEventHandler;

            ThreadStart threadStart = new ThreadStart(Dispatcher);
            Thread bgThread = new Thread(threadStart);
            bgThread.Start();
        }

        public void SBEventHandler(SBMessage message)
        {
            SBMessage sbMessage = new SBMessage()
            {
                SequenceNumber = message.SequenceNumber,
                Body = message.Body
            };

            _ClientReceiveQueue.Enqueue(sbMessage);
        }

        private void Dispatcher()
        {
            while (!SBReceive.Stop)
            {
                bool dequeued = _ClientReceiveQueue.TryDequeue(out SBMessage message);

                if (dequeued)
                {
                    UpdateWindows(message);
                }
                else
                {
                    Console.WriteLine("Dispatcher().Sleep()");
                    Thread.Sleep(100);
                }
            }
        }

        private void UpdateWindows(SBMessage message)
        {
            Console.WriteLine($"SBEventHandler - ClientId: {_ClientId} - SequenceNumber:{message.SequenceNumber} - Body:{message.Body}");

            int wait = _Random.Next(1000 * _ClientId);

            Thread.Sleep(wait);

            Console.WriteLine($"SBEventHandler - ClientId: {_ClientId} - Wait: {wait} - SequenceNumber:{message.SequenceNumber} - Body:{message.Body}");
        }
    }
}
