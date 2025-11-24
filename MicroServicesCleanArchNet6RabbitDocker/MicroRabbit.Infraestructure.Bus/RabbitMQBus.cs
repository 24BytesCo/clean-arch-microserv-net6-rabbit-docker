using MediatR;
using MicroRabbit.Domain.Core.Bus;
using MicroRabbit.Domain.Core.Commands;
using MicroRabbit.Domain.Core.Events;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace MicroRabbit.Infraestructure.Bus
{
    /// <summary>
    /// Implementación de bus de eventos usando RabbitMQ como transporte.
    /// </summary>
    public sealed class RabbitMQBus : IEventBus
    {
        private readonly RabbitMQSettings _settings;
        private readonly IMediator _mediator;
        private readonly Dictionary<string, List<Type>> _handlers;
        private readonly List<Type> _eventTypes;

        public RabbitMQBus(IMediator mediator, IOptions<RabbitMQSettings> settings)
        {
            _mediator = mediator;
            _handlers = new Dictionary<string, List<Type>>();
            _eventTypes = new List<Type>();
            _settings = settings.Value;
        }

        /// <summary>
        /// Publica un evento en la cola cuyo nombre coincide con el tipo del evento.
        /// </summary>
        public void Publish<T>(T @event) where T : Event
        {
            var factory = new ConnectionFactory()
            {
                HostName = _settings.HostName,
                UserName = _settings.UserName,
                Password = _settings.Password
            };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                var eventName = @event.GetType().Name;

                // Cola nombrada por el tipo de evento usando el exchange por defecto.
                channel.QueueDeclare
                    (
                        queue: eventName,
                        durable: false,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null
                    );

                var message = JsonConvert.SerializeObject(@event);

                var body = Encoding.UTF8.GetBytes(message);

                // RoutingKey = nombre del evento para que llegue a su propia cola.
                channel.BasicPublish
                    (
                        exchange: "",
                        routingKey: eventName,
                        basicProperties: null,
                        body: body
                    );
            }

        }

        /// <summary>
        /// Envía un comando a través de MediatR. (Pendiente de implementar)
        /// </summary>
        public void SendCommand<T>(T command) where T : Command
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Registra un manejador para un tipo de evento y comienza a consumir su cola.
        /// </summary>
        public void Subscribe<T, TH>()
            where T : Event
            where TH : IEventHandler<T>
        {
            var eventName = typeof(T).Name;
            var handlerType = typeof(TH);

            if (!_eventTypes.Contains(typeof(T)))
            {
                _eventTypes.Add(typeof(T));
            }

            if (!_handlers.ContainsKey(eventName))
            {
                _handlers.Add(eventName, new List<Type>());
            }

            if (_handlers[eventName].Any(s => s == handlerType))
            {
                throw new ArgumentException($"Handler Type {handlerType.Name} already registered for '{eventName}'", nameof(handlerType));
            }

            // Registrar el handler y levantar el consumidor si aún no existía.
            _handlers[eventName].Add(handlerType);

            StartBasicConsume<T>();


        }

        /// <summary>
        /// Arranca un consumidor asíncrono para la cola del evento.
        /// </summary>
        private void StartBasicConsume<T>() where T : Event
        {
            var factory = new ConnectionFactory()
            {
                HostName = _settings.HostName,
                UserName = _settings.UserName,
                Password = _settings.Password,
                DispatchConsumersAsync = true, // Permite callbacks async en el consumidor.
            };

            var connection = factory.CreateConnection();

            var channel = connection.CreateModel();

            var eventName = typeof(T).Name;

            channel.QueueDeclare
                (
                    queue: eventName,
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                );

            var consumer = new AsyncEventingBasicConsumer(channel);

            // Suscribir callback que delega el procesamiento real.
            consumer.Received += Consumer_Received;

            channel.BasicConsume
                (
                    queue: eventName,
                    autoAck: true,
                    consumer: consumer
                );
        }

        /// <summary>
        /// Delegar el mensaje recibido a los manejadores registrados.
        /// </summary>
        private async Task Consumer_Received(object sender, BasicDeliverEventArgs @event)
        {
            var eventName = @event.RoutingKey;

            var message = Encoding.UTF8.GetString(@event.Body.Span);

            try
            {
                // Procesa el evento en los handlers registrados vía reflexión.
                await ProcessEvent(eventName, message).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // TODO: loguear o gestionar el error según la estrategia de reintentos.
            }


        }

        private async Task ProcessEvent(string eventName, string message)
        {
            if (_handlers.ContainsKey(eventName))
            {
                var suscriptions = _handlers[eventName];

                foreach (var suscription in suscriptions)
                {
                    var handler = Activator.CreateInstance(suscription);

                    if (handler == null) continue;

                    var eventType = _eventTypes.SingleOrDefault(r => r.Name == eventName);

                    var @event = JsonConvert.DeserializeObject(message, eventType);

                    // Invoca Handle de manera dinámica (handler genérico en tiempo de ejecución).
                    var concretType = typeof(IEventHandler).MakeGenericType(eventType);

                    await (Task)concretType.GetMethod("Handle").Invoke(handler, new object[] { @event });
                }
            }
        }
    }
}
