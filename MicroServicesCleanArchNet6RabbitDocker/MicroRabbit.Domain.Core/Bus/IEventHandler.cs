
using MicroRabbit.Domain.Core.Events;

namespace MicroRabbit.Domain.Core.Bus
{
    /// <summary>
    /// Contrato de manejador para un tipo específico de evento.
    /// </summary>
    public interface IEventHandler<in TEvent> : IEventHandler
        where TEvent : Event
    {
        /// <summary>
        /// Ejecuta la lógica del manejador ante la llegada del evento.
        /// </summary>
        /// <param name="event">Evento recibido.</param>
        Task Handle(TEvent @event);
    }

    /// <summary>
    /// Interfaz marcador para simplificar el registro de manejadores.
    /// </summary>
    public interface IEventHandler
    {
    }
}
