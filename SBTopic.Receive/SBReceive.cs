using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Primitives;
using Newtonsoft.Json;
using SBTopic.Model;

namespace SBTopic.Receive
{
    public static class SBReceive
    {
        private static ISubscriptionClient _SubscriptionClient;

        private static ConcurrentQueue<SBMessage> _SBReceiveQueue = new ConcurrentQueue<SBMessage>();

        private static string _Destinatary { get; set; }

        private static SBSubscriptionConnectionData _SBSubscriptionConnectionData;

        public static bool Stop { get; set; } = false;

        public delegate void SBEventHandler(SBMessage message);

        // Declare the event.
        public static event SBEventHandler SBEvent;

        public static async Task Init(string Destinatary)
        {
            _Destinatary = Destinatary;

            _SBSubscriptionConnectionData = await GetSBSubscription(Destinatary);

            ClientAsync(_SBSubscriptionConnectionData);

            ThreadStart threadStart = new ThreadStart(Dispatcher);
            Thread bgThread = new Thread(threadStart);
            bgThread.Start();
        }

        private static void ClientAsync(SBSubscriptionConnectionData sbSubscriptionConnectionData)
        {
            TokenProvider clientTokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(sbSubscriptionConnectionData.SharedAccessSignatureToken);
            _SubscriptionClient = new SubscriptionClient(sbSubscriptionConnectionData.Endpoint, sbSubscriptionConnectionData.Topic, sbSubscriptionConnectionData.Subscription, clientTokenProvider);

            // Register subscription message handler and receive messages in a loop.
            RegisterOnMessageHandlerAndReceiveMessages();
        }

        private static async void RenewClientAsync(SBSubscriptionConnectionData sbSubscriptionConnectionData)
        {
            sbSubscriptionConnectionData = await GetSBSASToken(_Destinatary, sbSubscriptionConnectionData.Subscription);

            TokenProvider clientTokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(sbSubscriptionConnectionData.SharedAccessSignatureToken);
            _SubscriptionClient = new SubscriptionClient(sbSubscriptionConnectionData.Endpoint, sbSubscriptionConnectionData.Topic, sbSubscriptionConnectionData.Subscription, clientTokenProvider);

            // Register subscription message handler and receive messages in a loop.
            RegisterOnMessageHandlerAndReceiveMessages();
        }

        private static void RegisterOnMessageHandlerAndReceiveMessages()
        {
            // Configure the message handler options in terms of exception handling, number of concurrent messages to deliver, etc.
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                // Maximum number of concurrent calls to the callback ProcessMessagesAsync(), set to 1 for simplicity.
                // Set it according to how many messages the application wants to process in parallel.
                MaxConcurrentCalls = 1,

                // Indicates whether MessagePump should automatically complete the messages after returning from User Callback.
                // False below indicates the Complete will be handled by the User Callback as in `ProcessMessagesAsync` below.
                AutoComplete = false
            };

            // Register the function that processes messages.
            _SubscriptionClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
        }

        private static async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
            // Process the message.
            Console.WriteLine($"Received message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{Encoding.UTF8.GetString(message.Body)}");

            SBMessage sbMessage = new SBMessage()
            {
                SequenceNumber = message.SystemProperties.SequenceNumber,
                Body = Encoding.UTF8.GetString(message.Body)
            };

            _SBReceiveQueue.Enqueue(sbMessage);

            // Complete the message so that it is not received again.
            // This can be done only if the subscriptionClient is created in ReceiveMode.PeekLock mode (which is the default).
            await _SubscriptionClient.CompleteAsync(message.SystemProperties.LockToken);

            // Note: Use the cancellationToken passed as necessary to determine if the subscriptionClient has already been closed.
            // If subscriptionClient has already been closed, you can choose to not call CompleteAsync() or AbandonAsync() etc.
            // to avoid unnecessary exceptions.
        }

        private static async Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            Console.WriteLine($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            Console.WriteLine("Exception context for troubleshooting:");
            Console.WriteLine($"- Endpoint: {context.Endpoint}");
            Console.WriteLine($"- Entity Path: {context.EntityPath}");
            Console.WriteLine($"- Executing Action: {context.Action}");

            await _SubscriptionClient.CloseAsync();

            Console.WriteLine("======================================================");
            Console.WriteLine("Refreshing Token.");
            Console.WriteLine("======================================================");

            RenewClientAsync(_SBSubscriptionConnectionData);
        }

        private static async Task<SBSubscriptionConnectionData> GetSBSubscription(string Destinatary)
        {
            HttpClient httpClient = new HttpClient();
            HttpResponseMessage response = await httpClient.GetAsync("http://localhost:59469/api/SBSubscription/" + Destinatary);

            string content = await response.Content.ReadAsStringAsync();

            SBSubscriptionConnectionData sbSubscriptionConnectionData = JsonConvert.DeserializeObject<SBSubscriptionConnectionData>(content);

            Console.WriteLine("Service Bus Endpoint: " + sbSubscriptionConnectionData.Endpoint);

            return sbSubscriptionConnectionData;
        }

        private static async Task<SBSubscriptionConnectionData> GetSBSASToken(string Destinatary, string Subscription)
        {
            HttpClient httpClient = new HttpClient();
            HttpResponseMessage response = await httpClient.GetAsync("http://localhost:59469/api/SBSubscription/" + Destinatary + "/" + Subscription);

            string content = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<SBSubscriptionConnectionData>(content);
        }

        private static void Dispatcher()
        {
            while (!Stop)
            {
                bool dequeued = _SBReceiveQueue.TryDequeue(out SBMessage message);

                if (dequeued)
                {
                    SBEvent.Invoke(message);
                }
                else
                {
                    Console.WriteLine("Dispatcher().Sleep()");
                    Thread.Sleep(100);
                }
            }
        }
    }
}
