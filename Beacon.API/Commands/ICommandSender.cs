namespace Beacon.API.Commands
{
    public interface ICommandSender
    {
        public IServer Server { get; }
        public Task SendMessageAsync(string message);
    }
}
