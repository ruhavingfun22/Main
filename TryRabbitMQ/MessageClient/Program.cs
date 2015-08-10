using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MessageClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "52.24.25.127" }; // or "WIN-NDTTGRKJKD7"
            factory.Port = 5672;
            factory.UserName = "keith";
            factory.Password = "password";

            Operation foo = Operation.Receive;
            string message = "Keith is the bomb!";
            if (args.Length>0)
            {
                if (args[0].Equals("Send", StringComparison.OrdinalIgnoreCase))
                {
                    foo = Operation.Send;
                }
                if (args.Length > 1 && args[1] != null)
                {
                    message = args[1];
                }
            }

            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare("hello", false, false, false, null);

                    switch (foo)
                    {
                        case Operation.Send:
                            Send(channel, message);
                            break;
                        case Operation.Receive:
                            Receive(channel);
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Send the message to RabbitMQ
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        public static void Send(IModel channel, string message)
        {
            var body = Encoding.UTF8.GetBytes(message);
            var properties = channel.CreateBasicProperties();
            properties.DeliveryMode = 2;

            channel.BasicPublish("", "hello", null, body);
            Console.WriteLine(" [x] Sent {0}", message);
        }

        public static void Receive(IModel channel)
        {
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine(" [x] Received {0}", message);
            };
            channel.BasicConsume(queue: "hello", noAck: true, consumer: consumer);

            Console.WriteLine("Press <ENTER> to stop listening.");
            Console.ReadLine();

        }

        public enum Operation
        { 
            Send=0,
            Receive = 1,
        }
    }

}
