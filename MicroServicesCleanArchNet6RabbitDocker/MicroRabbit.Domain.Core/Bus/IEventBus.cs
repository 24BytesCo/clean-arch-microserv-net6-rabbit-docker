
using MicroRabbit.Domain.Core.Commands;
using MicroRabbit.Domain.Core.Events;

namespace MicroRabbit.Domain.Core.Bus
{
    /// <summary>
    /// Define el contrato del bus para enviar comandos y publicar/suscribir eventos de integración.
    /// </summary>
    public interface IEventBus
    {
        /// <summary>
        /// Envía un comando hacia el lado de comandos del sistema.
        /// </summary>
        /// <typeparam name="T">Tipo concreto del comando.</typeparam>
        void SendCommand<T>(T command) where T : Command;

        /// <summary>
        /// Publica un evento de integración en el bus.
        /// </summary>
        /// <typeparam name="T">Tipo concreto del evento.</typeparam>
        void Publish<T>(T @event) where T : Event;

        /// <summary>
        /// Suscribe un manejador a un tipo de evento específico.
        /// </summary>
        /// <typeparam name="T">Tipo del evento a escuchar.</typeparam>
        /// <typeparam name="TH">Tipo del manejador del evento.</typeparam>
        void Subscribe<T, TH>()
            where T : Event
            where TH : IEventHandler<T>;


    }
}
