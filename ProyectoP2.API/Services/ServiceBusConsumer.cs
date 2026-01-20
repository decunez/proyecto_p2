using Newtonsoft.Json;
using ProyectoP2.API.Data;
using ProyectoP2.API.Models;

using Experimental.System.Messaging; // O Experimental.System.Messaging

namespace ProyectoP2.API
{
    public class ServiceBusConsumer : BackgroundService
    {
        // No inyectamos el DbContext directamente, sino la fábrica para crearlo cuando se necesite
        private readonly IServiceScopeFactory _scopeFactory;

        public ServiceBusConsumer(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            string rutaCola = @".\Private$\colasoporte";

            // 1. Asegurar que la cola existe
            if (!MessageQueue.Exists(rutaCola))
            {
                MessageQueue.Create(rutaCola);
            }

            using (var cola = new MessageQueue(rutaCola))
            {
                // Importante para poder leer texto complejo (JSON)
                cola.Formatter = new XmlMessageFormatter(new[] { typeof(string) });

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        // 2. Intentar recibir mensaje con un tiempo límite de 1 segundo
                        // Esto evita que el hilo se congele eternamente si no hay mensajes.
                        var mensaje = cola.Receive(TimeSpan.FromSeconds(1));

                        if (mensaje != null)
                        {
                            // Leer el contenido del mensaje
                            string cuerpoJson = mensaje.Body.ToString();

                            // Deserializar el JSON a objeto Soporte
                            var soporteRecibido = JsonConvert.DeserializeObject<Soporte>(cuerpoJson);

                            // 3. Guardar en Base de Datos usando un Scope
                            using (var scope = _scopeFactory.CreateScope())
                            {
                                // Obtenemos el DbContext dentro de este scope temporal
                                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                                dbContext.Soportes.Add(soporteRecibido);
                                await dbContext.SaveChangesAsync();

                                Console.WriteLine($"[EXITO] Ticket guardado: {soporteRecibido.Asunto}");
                            }
                        }
                    }
                    catch (MessageQueueException mqEx)
                    {
                        // Si el error es "Timeout" (se acabó el tiempo de espera), lo ignoramos y seguimos el bucle
                        if (mqEx.MessageQueueErrorCode == MessageQueueErrorCode.IOTimeout)
                        {
                            // No hay mensajes, esperamos un poco y volvemos a intentar
                            await Task.Delay(1000, stoppingToken);
                        }
                        else
                        {
                            Console.WriteLine($"Error de MSMQ: {mqEx.Message}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error general procesando mensaje: {ex.Message}");
                    }
                }
            }
        }
    }
}