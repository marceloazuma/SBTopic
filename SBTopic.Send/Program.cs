using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;


namespace SBTopic.Send
{
    class Program
    {
        const string ServiceBusConnectionString = "Endpoint=sb://xp-sbteste.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=Dq698VqvxAJKSWPsOOuEuYVKXQjjnGtpvM7j1EaCIZU=";
        const string TopicName = "Teste";
        static ITopicClient topicClient;

        static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();
        }

        static async Task MainAsync()
        {
            const int numberOfMessages = 105;
            topicClient = new TopicClient(ServiceBusConnectionString, TopicName);

            // Send messages.
            await SendMessagesAsync(numberOfMessages);

            //Console.ReadKey();

            await topicClient.CloseAsync();
        }

        static async Task SendMessagesAsync(int numberOfMessagesToSend)
        {
            try
            {
                Random random = new Random();

                for (var i = 0; i < numberOfMessagesToSend; i++)
                {
                    // Create a new message to send to the topic
                    string messageBody = $"Message {i}";
                    Message message = new Message(Encoding.UTF8.GetBytes(messageBody));

                    message.To = (i % 2 == 0) ? "Executor" : "Sales";

                    // Write the body of the message to the console
                    Console.WriteLine($"Sending message: {messageBody}");

                    // Send the message to the topic
                    await topicClient.SendAsync(message);

                    int wait = random.Next(1000);

                    Thread.Sleep(wait);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine($"{DateTime.Now} :: Exception: {exception.Message}");
            }
        }
    }
}
