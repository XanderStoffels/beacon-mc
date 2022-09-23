using System.Collections.Concurrent;

namespace Beacon.Server.Utils;

internal abstract class ObjectPool<T> where T : new()
{
    private static SharedPool<T>? sharedPool;
    public static ObjectPool<T> Shared => sharedPool ??= new SharedPool<T>();
    public abstract T Get();
    public abstract void Return(T t);

    private sealed class SharedPool<TObject> : ObjectPool<TObject> where TObject : new()
    {
        private readonly ConcurrentStack<TObject> _stack = new();

        public override TObject Get()
        {
            return _stack.TryPop(out var obj) ? obj : new TObject();
        }

        public override void Return(TObject u)
        {
            _stack.Push(u);
        }
    }
}