namespace GermanyGlossary.Services
{
    // Abstraction over the current time so date-dependent logic can be tested with a fixed clock.
    public interface IClock
    {
        DateTime UtcNow { get; }
    }

    public class SystemClock : IClock
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
