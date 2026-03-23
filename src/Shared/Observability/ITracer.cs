namespace Shared.Observability;

public interface ITracer
{
    IDisposable StartSpan(string name);
}
