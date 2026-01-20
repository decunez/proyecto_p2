using Azure.Messaging.ServiceBus;
using System.Text.Json;

namespace ProyectoP2.Web.Services
{
    public class ServiceBusProducer
    {
        private readonly IConfiguration _configuration;

        public ServiceBusProducer(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task EnviarMensajeAsync(object mensaje)
        {
            // Leemos la config del appsettings.json
            string connectionString = _configuration["AzureSettings:ServiceBusConnectionString"];
            string queueName = _configuration["AzureSettings:QueueName"];

            // Creamos el cliente y el remitente
            await using var client = new ServiceBusClient(connectionString);
            ServiceBusSender sender = client.CreateSender(queueName);

            // Convertimos el objeto (nombre y mensaje) a JSON
            string jsonBody = JsonSerializer.Serialize(mensaje);
            ServiceBusMessage busMessage = new ServiceBusMessage(jsonBody);

            // Enviamos a la cola
            await sender.SendMessageAsync(busMessage);
        }
    }
}