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
    public class BatchClientReceive : IDisposable
    {
        private int _ClientId;
        private Random _Random;

        /// <summary>
        /// Processador de blocos de mensagens
        /// </summary>
        private ActionBlock<SBMessage[]> _ClientReceiveActionBatchBlock;

        /// <summary>
        /// Agrupador de mensagens em blocos
        /// </summary>
        private BatchBlock<SBMessage> _ClientReceiveBatchBlock = new BatchBlock<SBMessage>(10);

        /// <summary>
        /// Buffer de entrada de mensagens para o ClientReceive
        /// </summary>
        private BufferBlock<SBMessage> _ClientReceiveBufferBlock = new BufferBlock<SBMessage>();

        /// <summary>
        /// Timer para processar blocos de mensagens antes que o agrupador esteja completo
        /// </summary>
        private TransformBlock<SBMessage, SBMessage> _ClientReceiveTimeoutTransformBlock;

        private IDisposable _ClientReceiveActionBatchBlockLink;

        private IDisposable _ClientReceiveBatchBlockLink;

        private IDisposable _ClientReceiveTimeoutTransformBlockLink;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientId">A value just to differentiate each instance</param>
        public BatchClientReceive(int clientId)
        {
            _ClientId = clientId;
            _Random = new Random(_ClientId);

            _ClientReceiveActionBatchBlock = new ActionBlock<SBMessage[]>(sbMessages => MessagesHandler(sbMessages));

            _ClientReceiveActionBatchBlockLink = _ClientReceiveBatchBlock.LinkTo(_ClientReceiveActionBatchBlock);

            Timer triggerBatchTimer = new Timer((callback) => _ClientReceiveBatchBlock.TriggerBatch());

            _ClientReceiveTimeoutTransformBlock = new TransformBlock<SBMessage, SBMessage>((sbMessage) =>
            {
                triggerBatchTimer.Change(2000, Timeout.Infinite);

                return sbMessage;
            });

            _ClientReceiveBatchBlockLink = _ClientReceiveTimeoutTransformBlock.LinkTo(_ClientReceiveBatchBlock);

            _ClientReceiveTimeoutTransformBlockLink = _ClientReceiveBufferBlock.LinkTo(_ClientReceiveTimeoutTransformBlock);

            SBReceive.SBMessageBufferBlockList.Add(_ClientReceiveBufferBlock);
        }

        /// <summary>
        /// Emulates the messages processing in batches
        /// </summary>
        /// <param name="message"></param>
        private void MessagesHandler(SBMessage[] messages)
        {
            Console.WriteLine($"MessagesHandler - ClientId: {_ClientId} - messages.Length: {messages.Length}");

            foreach (SBMessage message in messages)
            {
                Console.WriteLine($"MessagesHandler - ClientId: {_ClientId} - SequenceNumber:{message.SequenceNumber} - Body:{message.Body}");
            }

            int wait = _Random.Next(1000 * _ClientId);

            Thread.Sleep(wait);

            foreach (SBMessage message in messages)
            {
                Console.WriteLine($"MessagesHandler - ClientId: {_ClientId} - Wait: {wait} - SequenceNumber:{message.SequenceNumber} - Body:{message.Body}");
            }
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

                    if (_ClientReceiveTimeoutTransformBlockLink != null)
                        _ClientReceiveTimeoutTransformBlockLink.Dispose();

                    if (_ClientReceiveBatchBlockLink != null)
                        _ClientReceiveBatchBlockLink.Dispose();

                    if (_ClientReceiveActionBatchBlockLink != null)
                        _ClientReceiveActionBatchBlockLink.Dispose();
                }

                disposedValue = true;
            }
        }

        ~BatchClientReceive()
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
