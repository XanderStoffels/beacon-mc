using System.Collections.Concurrent;

namespace Beacon.Util;


internal abstract class ObjectPool<T> where T : new()
{
    private static SharedPool<T>? _sharedPool;
    public static ObjectPool<T> Shared => _sharedPool ??= new SharedPool<T>();
    public abstract T Get();
    public abstract void Return(T t);

    private sealed class SharedPool<TObject> : ObjectPool<TObject> where TObject : new()
    {
        private readonly ConcurrentStack<TObject> _stack = new();
        private int _rentedCount;
        public override TObject Get()
        {
            var o = _stack.TryPop(out var obj) ? obj : new TObject();
            _rentedCount++;
            Console.WriteLine($"ObjectPool<{typeof(TObject).Name}>: GET ({_rentedCount}");
            return o;
        }

        public override void Return(TObject u)
        {
            _rentedCount--;
            _stack.Push(u);
            Console.WriteLine($"ObjectPool<{typeof(TObject).Name}>: RETURN ({_rentedCount})");

        }
    }
}