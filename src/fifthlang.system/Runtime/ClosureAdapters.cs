using System;

namespace Fifth.System.Runtime;

public static class ClosureAdapters
{
    public static IClosure<TReturn> FromFunc<TReturn>(Func<TReturn> func)
        => new FuncClosure<TReturn>(func);

    public static IClosure<T1, TReturn> FromFunc<T1, TReturn>(Func<T1, TReturn> func)
        => new FuncClosure<T1, TReturn>(func);

    public static IClosure<T1, T2, TReturn> FromFunc<T1, T2, TReturn>(Func<T1, T2, TReturn> func)
        => new FuncClosure<T1, T2, TReturn>(func);

    public static IClosure<T1, T2, T3, TReturn> FromFunc<T1, T2, T3, TReturn>(Func<T1, T2, T3, TReturn> func)
        => new FuncClosure<T1, T2, T3, TReturn>(func);

    public static IClosure<T1, T2, T3, T4, TReturn> FromFunc<T1, T2, T3, T4, TReturn>(Func<T1, T2, T3, T4, TReturn> func)
        => new FuncClosure<T1, T2, T3, T4, TReturn>(func);

    public static IClosure<T1, T2, T3, T4, T5, TReturn> FromFunc<T1, T2, T3, T4, T5, TReturn>(Func<T1, T2, T3, T4, T5, TReturn> func)
        => new FuncClosure<T1, T2, T3, T4, T5, TReturn>(func);

    public static IClosure<T1, T2, T3, T4, T5, T6, TReturn> FromFunc<T1, T2, T3, T4, T5, T6, TReturn>(Func<T1, T2, T3, T4, T5, T6, TReturn> func)
        => new FuncClosure<T1, T2, T3, T4, T5, T6, TReturn>(func);

    public static IClosure<T1, T2, T3, T4, T5, T6, T7, TReturn> FromFunc<T1, T2, T3, T4, T5, T6, T7, TReturn>(Func<T1, T2, T3, T4, T5, T6, T7, TReturn> func)
        => new FuncClosure<T1, T2, T3, T4, T5, T6, T7, TReturn>(func);

    public static IClosure<T1, T2, T3, T4, T5, T6, T7, T8, TReturn> FromFunc<T1, T2, T3, T4, T5, T6, T7, T8, TReturn>(Func<T1, T2, T3, T4, T5, T6, T7, T8, TReturn> func)
        => new FuncClosure<T1, T2, T3, T4, T5, T6, T7, T8, TReturn>(func);

    public static IActionClosure FromAction(Action action)
        => new ActionClosure(action);

    public static IActionClosure<T1> FromAction<T1>(Action<T1> action)
        => new ActionClosure<T1>(action);

    public static IActionClosure<T1, T2> FromAction<T1, T2>(Action<T1, T2> action)
        => new ActionClosure<T1, T2>(action);

    public static IActionClosure<T1, T2, T3> FromAction<T1, T2, T3>(Action<T1, T2, T3> action)
        => new ActionClosure<T1, T2, T3>(action);

    public static IActionClosure<T1, T2, T3, T4> FromAction<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action)
        => new ActionClosure<T1, T2, T3, T4>(action);

    public static IActionClosure<T1, T2, T3, T4, T5> FromAction<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action)
        => new ActionClosure<T1, T2, T3, T4, T5>(action);

    public static IActionClosure<T1, T2, T3, T4, T5, T6> FromAction<T1, T2, T3, T4, T5, T6>(Action<T1, T2, T3, T4, T5, T6> action)
        => new ActionClosure<T1, T2, T3, T4, T5, T6>(action);

    public static IActionClosure<T1, T2, T3, T4, T5, T6, T7> FromAction<T1, T2, T3, T4, T5, T6, T7>(Action<T1, T2, T3, T4, T5, T6, T7> action)
        => new ActionClosure<T1, T2, T3, T4, T5, T6, T7>(action);

    public static IActionClosure<T1, T2, T3, T4, T5, T6, T7, T8> FromAction<T1, T2, T3, T4, T5, T6, T7, T8>(Action<T1, T2, T3, T4, T5, T6, T7, T8> action)
        => new ActionClosure<T1, T2, T3, T4, T5, T6, T7, T8>(action);

    private sealed class FuncClosure<TReturn> : IClosure<TReturn>
    {
        private readonly Func<TReturn> _func;

        public FuncClosure(Func<TReturn> func)
        {
            _func = func ?? throw new ArgumentNullException(nameof(func));
        }

        public TReturn Apply() => _func();
    }

    private sealed class FuncClosure<T1, TReturn> : IClosure<T1, TReturn>
    {
        private readonly Func<T1, TReturn> _func;

        public FuncClosure(Func<T1, TReturn> func)
        {
            _func = func ?? throw new ArgumentNullException(nameof(func));
        }

        public TReturn Apply(T1 t1) => _func(t1);
    }

    private sealed class FuncClosure<T1, T2, TReturn> : IClosure<T1, T2, TReturn>
    {
        private readonly Func<T1, T2, TReturn> _func;

        public FuncClosure(Func<T1, T2, TReturn> func)
        {
            _func = func ?? throw new ArgumentNullException(nameof(func));
        }

        public TReturn Apply(T1 t1, T2 t2) => _func(t1, t2);
    }

    private sealed class FuncClosure<T1, T2, T3, TReturn> : IClosure<T1, T2, T3, TReturn>
    {
        private readonly Func<T1, T2, T3, TReturn> _func;

        public FuncClosure(Func<T1, T2, T3, TReturn> func)
        {
            _func = func ?? throw new ArgumentNullException(nameof(func));
        }

        public TReturn Apply(T1 t1, T2 t2, T3 t3) => _func(t1, t2, t3);
    }

    private sealed class FuncClosure<T1, T2, T3, T4, TReturn> : IClosure<T1, T2, T3, T4, TReturn>
    {
        private readonly Func<T1, T2, T3, T4, TReturn> _func;

        public FuncClosure(Func<T1, T2, T3, T4, TReturn> func)
        {
            _func = func ?? throw new ArgumentNullException(nameof(func));
        }

        public TReturn Apply(T1 t1, T2 t2, T3 t3, T4 t4) => _func(t1, t2, t3, t4);
    }

    private sealed class FuncClosure<T1, T2, T3, T4, T5, TReturn> : IClosure<T1, T2, T3, T4, T5, TReturn>
    {
        private readonly Func<T1, T2, T3, T4, T5, TReturn> _func;

        public FuncClosure(Func<T1, T2, T3, T4, T5, TReturn> func)
        {
            _func = func ?? throw new ArgumentNullException(nameof(func));
        }

        public TReturn Apply(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5) => _func(t1, t2, t3, t4, t5);
    }

    private sealed class FuncClosure<T1, T2, T3, T4, T5, T6, TReturn> : IClosure<T1, T2, T3, T4, T5, T6, TReturn>
    {
        private readonly Func<T1, T2, T3, T4, T5, T6, TReturn> _func;

        public FuncClosure(Func<T1, T2, T3, T4, T5, T6, TReturn> func)
        {
            _func = func ?? throw new ArgumentNullException(nameof(func));
        }

        public TReturn Apply(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6) => _func(t1, t2, t3, t4, t5, t6);
    }

    private sealed class FuncClosure<T1, T2, T3, T4, T5, T6, T7, TReturn> : IClosure<T1, T2, T3, T4, T5, T6, T7, TReturn>
    {
        private readonly Func<T1, T2, T3, T4, T5, T6, T7, TReturn> _func;

        public FuncClosure(Func<T1, T2, T3, T4, T5, T6, T7, TReturn> func)
        {
            _func = func ?? throw new ArgumentNullException(nameof(func));
        }

        public TReturn Apply(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7) => _func(t1, t2, t3, t4, t5, t6, t7);
    }

    private sealed class FuncClosure<T1, T2, T3, T4, T5, T6, T7, T8, TReturn> : IClosure<T1, T2, T3, T4, T5, T6, T7, T8, TReturn>
    {
        private readonly Func<T1, T2, T3, T4, T5, T6, T7, T8, TReturn> _func;

        public FuncClosure(Func<T1, T2, T3, T4, T5, T6, T7, T8, TReturn> func)
        {
            _func = func ?? throw new ArgumentNullException(nameof(func));
        }

        public TReturn Apply(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8) => _func(t1, t2, t3, t4, t5, t6, t7, t8);
    }

    private sealed class ActionClosure : IActionClosure
    {
        private readonly Action _action;

        public ActionClosure(Action action)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
        }

        public void Apply() => _action();
    }

    private sealed class ActionClosure<T1> : IActionClosure<T1>
    {
        private readonly Action<T1> _action;

        public ActionClosure(Action<T1> action)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
        }

        public void Apply(T1 t1) => _action(t1);
    }

    private sealed class ActionClosure<T1, T2> : IActionClosure<T1, T2>
    {
        private readonly Action<T1, T2> _action;

        public ActionClosure(Action<T1, T2> action)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
        }

        public void Apply(T1 t1, T2 t2) => _action(t1, t2);
    }

    private sealed class ActionClosure<T1, T2, T3> : IActionClosure<T1, T2, T3>
    {
        private readonly Action<T1, T2, T3> _action;

        public ActionClosure(Action<T1, T2, T3> action)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
        }

        public void Apply(T1 t1, T2 t2, T3 t3) => _action(t1, t2, t3);
    }

    private sealed class ActionClosure<T1, T2, T3, T4> : IActionClosure<T1, T2, T3, T4>
    {
        private readonly Action<T1, T2, T3, T4> _action;

        public ActionClosure(Action<T1, T2, T3, T4> action)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
        }

        public void Apply(T1 t1, T2 t2, T3 t3, T4 t4) => _action(t1, t2, t3, t4);
    }

    private sealed class ActionClosure<T1, T2, T3, T4, T5> : IActionClosure<T1, T2, T3, T4, T5>
    {
        private readonly Action<T1, T2, T3, T4, T5> _action;

        public ActionClosure(Action<T1, T2, T3, T4, T5> action)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
        }

        public void Apply(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5) => _action(t1, t2, t3, t4, t5);
    }

    private sealed class ActionClosure<T1, T2, T3, T4, T5, T6> : IActionClosure<T1, T2, T3, T4, T5, T6>
    {
        private readonly Action<T1, T2, T3, T4, T5, T6> _action;

        public ActionClosure(Action<T1, T2, T3, T4, T5, T6> action)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
        }

        public void Apply(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6) => _action(t1, t2, t3, t4, t5, t6);
    }

    private sealed class ActionClosure<T1, T2, T3, T4, T5, T6, T7> : IActionClosure<T1, T2, T3, T4, T5, T6, T7>
    {
        private readonly Action<T1, T2, T3, T4, T5, T6, T7> _action;

        public ActionClosure(Action<T1, T2, T3, T4, T5, T6, T7> action)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
        }

        public void Apply(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7) => _action(t1, t2, t3, t4, t5, t6, t7);
    }

    private sealed class ActionClosure<T1, T2, T3, T4, T5, T6, T7, T8> : IActionClosure<T1, T2, T3, T4, T5, T6, T7, T8>
    {
        private readonly Action<T1, T2, T3, T4, T5, T6, T7, T8> _action;

        public ActionClosure(Action<T1, T2, T3, T4, T5, T6, T7, T8> action)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
        }

        public void Apply(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8) => _action(t1, t2, t3, t4, t5, t6, t7, t8);
    }
}
