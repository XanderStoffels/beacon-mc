namespace Beacon.Server.Plugins
{
    public interface IPluginController
    {
        public ValueTask LoadAsync();
        public ValueTask UnloadAsync();
        public ValueTask PublishEvent<TEvent>(TEvent e, CancellationToken cToken = default);
    }
}
