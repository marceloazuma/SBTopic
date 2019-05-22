using System;
using System.Collections.Concurrent;
using System.Threading;
using SBTopic.Model;

namespace SBTopic.Receive
{
    /// <summary>
    /// This class emulates a window in a client app, like a ViewModel in MVVM.
    /// </summary>
    public class ClientReceive
    {
        private int _ClientId;
        private Random _Random;

        /// <summary>
        /// This queue will have a copy of the messages received from the Service Bus, so they can be handled according the processing capacity.
        /// </summary>
        private ConcurrentQueue<SBMessage> _ClientReceiveQueue = new ConcurrentQueue<SBMessage>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientId">A value just to differentiate each instance</param>
        public ClientReceive(int clientId)
        {
            _ClientId = clientId;
            _Random = new Random(_ClientId);

            SBReceive.SBEvent += SBEventHandler;

            ThreadStart threadStart = new ThreadStart(Dispatcher);
            Thread bgThread = new Thread(threadStart);
            bgThread.Start();
        }

        /// <summary>
        /// Handle the event received from the SBReceive.Dispatcher()
        /// </summary>
        /// <param name="message"></param>
        public void SBEventHandler(SBMessage message)
        {
            SBMessage sbMessage = new SBMessage()
            {
                SequenceNumber = message.SequenceNumber,
                Body = message.Body
            };

            _ClientReceiveQueue.Enqueue(sbMessage);
        }

        /// <summary>
        /// Thread dispatcher to process the messages in the local queue
        /// </summary>
        private void Dispatcher()
        {
            while (!SBReceive.Stop)
            {
                bool dequeued = _ClientReceiveQueue.TryDequeue(out SBMessage message);

                if (dequeued)
                {
                    MessageHandler(message);
                }
                else
                {
                    Console.WriteLine($"Dispatcher().Sleep() - ClientId: {_ClientId}");
                    Thread.Sleep(100);
                }
            }
        }

        /// <summary>
        /// Emulates the messages processing
        /// </summary>
        /// <param name="message"></param>
        private void MessageHandler(SBMessage message)
        {
            Console.WriteLine($"SBEventHandler - ClientId: {_ClientId} - SequenceNumber:{message.SequenceNumber} - Body:{message.Body}");

            int wait = _Random.Next(1000 * _ClientId);

            Thread.Sleep(wait);

            Console.WriteLine($"SBEventHandler - ClientId: {_ClientId} - Wait: {wait} - SequenceNumber:{message.SequenceNumber} - Body:{message.Body}");
        }
    }
}
