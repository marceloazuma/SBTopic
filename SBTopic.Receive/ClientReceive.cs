using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using SBTopic.Model;

namespace SBTopic.Receive
{
    /// <summary>
    /// This class emulates a window in a client app, like a ViewModel in MVVM.
    /// </summary>
    public class ClientReceive : IDisposable
    {
        private int _ClientId;
        private Random _Random;

        /// <summary>
        /// Processador de mensagens
        /// </summary>
        private ActionBlock<SBMessage> _ClientReceiveActionBlock;

        /// <summary>
        /// Buffer de entrada de mensagens para o ClientReceive
        /// </summary>
        private BufferBlock<SBMessage> _ClientReceiveBufferBlock = new BufferBlock<SBMessage>();

        private IDisposable _ClientReceiveActionBlockLink;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientId">A value just to differentiate each instance</param>
        public ClientReceive(int clientId)
        {
            _ClientId = clientId;
            _Random = new Random(_ClientId);

            _ClientReceiveActionBlock = new ActionBlock<SBMessage>(sbMessage => MessageHandler(sbMessage));

            _ClientReceiveActionBlockLink = _ClientReceiveBufferBlock.LinkTo(_ClientReceiveActionBlock);

            SBReceive.SBMessageBufferBlockList.Add(_ClientReceiveBufferBlock);
        }

        /// <summary>
        /// Emulates the messages processing
        /// </summary>
        /// <param name="message"></param>
        private void MessageHandler(SBMessage message)
        {
            Console.WriteLine($"MessageHandler - ClientId: {_ClientId} - SequenceNumber:{message.SequenceNumber} - Body:{message.Body}");

            int wait = _Random.Next(1000 * _ClientId);

            Thread.Sleep(wait);

            Console.WriteLine($"MessageHandler - ClientId: {_ClientId} - Wait: {wait} - SequenceNumber:{message.SequenceNumber} - Body:{message.Body}");
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (_ClientReceiveBufferBlock != null)
                        SBReceive.SBMessageBufferBlockList.Remove(_ClientReceiveBufferBlock);

                    if (_ClientReceiveActionBlockLink != null)
                        _ClientReceiveActionBlockLink.Dispose();
                }

                disposedValue = true;
            }
        }

        ~ClientReceive()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);

            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
