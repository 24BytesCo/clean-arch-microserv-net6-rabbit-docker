using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

/// <summary>
/// Configuracion de la fabrica de conexiones hacia RabbitMQ en localhost.
/// </summary>
var factory = new ConnectionFactory()
{
    HostName = "localhost",
    UserName = "24bytes",
    Password = "24bytes"
};

/// <summary>
/// Creacion de la conexion y del canal que se usara para recibir mensajes.
/// </summary>
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

/// <summary>
/// Declaracion de la misma cola desde la que se consumiran los mensajes.
/// Si no existe, la crea; si ya existe con la misma configuracion, no la modifica.
/// </summary>
channel.QueueDeclare(
    queue: "24bytesQueue",
    durable: false,
    exclusive: false,
    autoDelete: false,
    arguments: null
);

/// <summary>
/// Creacion del consumidor basado en eventos asociado al canal.
/// </summary>
var consumer = new EventingBasicConsumer(channel);

/// <summary>
/// Manejador que se ejecuta cada vez que llega un mensaje a la cola.
/// </summary>
consumer.Received += (model, ea) =>
{
    // El cuerpo del mensaje se expone como un buffer de bytes.
    var body = ea.Body.Span;

    // Se convierte el buffer de bytes a texto usando codificacion UTF8.
    var message = Encoding.UTF8.GetString(body);

    Console.WriteLine("Mensaje recibido, Received: {0}", message);
};

/// <summary>
/// Inicio del consumo de mensajes desde la cola.
/// </summary>
channel.BasicConsume(
    queue: "24bytesQueue",
    autoAck: true,
    consumer: consumer
);

Console.WriteLine("Presiona [enter] para salir de la aplicacion");
Console.ReadLine();
