namespace MicroRabbit.Domain.Core.Events
{
    /// <summary>
    /// Clase base para eventos de dominio o integración.
    /// </summary>
    public abstract class Event
    {
        public DateTime Timestamp { get; protected set; }

        protected Event()
        {
            Timestamp = DateTime.Now;
        }
    }
}
