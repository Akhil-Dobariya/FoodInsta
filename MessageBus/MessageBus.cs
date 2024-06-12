using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MessageBus
{
    public class MessageBus : IMessageBus
    {
        private string ConnectionString = "Endpoint=sb://mangowebad.servicebus.windows.net/;SharedAccessKeyName=mangoweb;SharedAccessKey=wKHLUrHOUZ6Q25N0oPUImIYPrl+cnFIOl+ASbENQetA=;EntityPath=emailshoppingcart";
        public async Task PublishMessage(object message, string topic_queue_name)
        {
            await using var client = new ServiceBusClient(ConnectionString);

            ServiceBusSender sender = client.CreateSender(topic_queue_name);

            string stringmessage = JsonConvert.SerializeObject(message);

            ServiceBusMessage sbMessage = new ServiceBusMessage(Encoding.UTF8.GetBytes(stringmessage)) { CorrelationId=Guid.NewGuid().ToString() };

            await sender.SendMessageAsync(sbMessage);
            await client.DisposeAsync();
        }
    }
}
