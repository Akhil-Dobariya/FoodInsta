using Azure.Messaging.ServiceBus;
using Mango.Services.EmailAPI.Data;
using Mango.Services.EmailAPI.Models;
using Mango.Services.EmailAPI.Service;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Text;

namespace Mango.Services.EmailAPI.Messaging
{
    public class AzureServiceBusConsumer:IAzureServiceBusConsumer
    {
        private readonly string servicebusConnectionString;
        private readonly string emailcartqueue;
        private readonly IConfiguration _configuration;
        private ServiceBusProcessor emailProcessor;
        private readonly EmailService _emailService;

        public AzureServiceBusConsumer(IConfiguration configuration, EmailService emailService)
        {
            _configuration = configuration;
            _emailService=emailService;

            servicebusConnectionString = _configuration.GetValue<string>("ConnectionStrings:ServiceBusConnection");
            emailcartqueue = _configuration.GetValue<string>("TopicAndQueueNames:EmailShoppingCartQueue");

            var client = new ServiceBusClient(servicebusConnectionString);
            emailProcessor = client.CreateProcessor(emailcartqueue);
        }

        public async Task Start()
        {
            emailProcessor.ProcessMessageAsync += OnEmailCartRequestReceived;
            emailProcessor.ProcessErrorAsync += ErrorHandler;
            await emailProcessor.StartProcessingAsync();
        }

        private async Task OnEmailCartRequestReceived(ProcessMessageEventArgs args)
        {
            var message = args.Message;
            var body = Encoding.UTF8.GetString(message.Body);

            CartDTO objMsg = JsonConvert.DeserializeObject<CartDTO>(body);

            try
            {
                await _emailService.EmailCartAndLog(objMsg);

                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception ex)
            {

            }
        }

        private Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }

        public async Task Stop()
        {
            await emailProcessor.StopProcessingAsync(); ;
            await emailProcessor.DisposeAsync();
        }
    }
}
