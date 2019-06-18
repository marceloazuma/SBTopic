using System;
using System.Threading.Tasks;

namespace SBTopic.Receive
{
    class Program
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args">First command line argument must be Executor or Sales to match the filter</param>
        /// <returns></returns>
        static async Task Main(string[] args)
        {
            await SBReceive.Init(args[0]);

            using (ClientReceive clientReceive1 = new ClientReceive(1))
            {
                using (ClientReceive clientReceive2 = new ClientReceive(2))
                {
                    using (ClientReceive clientReceive3 = new ClientReceive(3))
                    { 
                        Console.WriteLine("======================================================");
                        Console.WriteLine("Press ENTER key to exit after receiving all the messages.");
                        Console.WriteLine("======================================================");

                        Console.ReadLine();
                    }
                }
            }
        }
    }
}
