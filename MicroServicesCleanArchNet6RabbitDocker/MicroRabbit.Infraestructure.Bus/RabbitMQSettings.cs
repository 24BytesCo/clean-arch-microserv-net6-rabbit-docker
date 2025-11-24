using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroRabbit.Infraestructure.Bus
{
    /// <summary>
    /// Configuración tipada para la conexión a RabbitMQ.
    /// </summary>
    public class RabbitMQSettings
    {
        /// <summary>
        /// Host de RabbitMQ.
        /// </summary>
        public string HostName { get; set; } = string.Empty;
        /// <summary>
        /// Usuario de acceso a RabbitMQ.
        /// </summary>
        public string UserName { get; set; } = string.Empty;
        /// <summary>
        /// Contraseña de acceso a RabbitMQ.
        /// </summary>
        public string Password { get; set; } = string.Empty;

    }
}
