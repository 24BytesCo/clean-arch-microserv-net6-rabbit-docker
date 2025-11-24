using RabbitMQ.Client;
using System.Text;

/// <summary>
/// Configuracion de la fabrica de conexiones hacia RabbitMQ en localhost.
/// </summary>
var factory = new ConnectionFactory
{
    HostName = "localhost",
    UserName = "24bytes",
    Password = "24bytes"
};

/// <summary>
/// Creacion de la conexion y del canal que se usara para enviar mensajes.
/// </summary>
using var connection = factory.CreateConnection();
using var chanel = connection.CreateModel();

/// <summary>
/// Declaracion de la cola de trabajo. Si no existe, la crea.
/// Si ya existe con la misma configuracion, no la modifica.
/// </summary>
chanel.QueueDeclare(
    queue: "24bytesQueue",
    durable: false,
    exclusive: false,
    autoDelete: false,
    arguments: null
);

string message = "Hello World!";

// El mensaje debe enviarse como arreglo de bytes codificado en UTF8.
var body = Encoding.UTF8.GetBytes(message);

/// <summary>
/// Publicacion del mensaje en el exchange por defecto,
/// usando como routingKey el nombre de la cola.
/// </summary>
chanel.BasicPublish(
    exchange: "",
    routingKey: "24bytesQueue",
    basicProperties: null,
    body: body
);

Console.WriteLine("[x] Mensaje enviado, Sent {0}", message);

Console.WriteLine("Presiona [enter] para salir de la aplicacion");
Console.ReadLine();
