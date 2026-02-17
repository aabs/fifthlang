namespace Fifth.System.Runtime;

// NOTE: These interfaces are part of the Fifth runtime surface.
// They represent callable closure instances after lowering/defunctionalisation.

public interface IClosure<out TReturn>
{
    TReturn Apply();
}

public interface IClosure<in T1, out TReturn>
{
    TReturn Apply(T1 t1);
}

public interface IClosure<in T1, in T2, out TReturn>
{
    TReturn Apply(T1 t1, T2 t2);
}

public interface IClosure<in T1, in T2, in T3, out TReturn>
{
    TReturn Apply(T1 t1, T2 t2, T3 t3);
}

public interface IClosure<in T1, in T2, in T3, in T4, out TReturn>
{
    TReturn Apply(T1 t1, T2 t2, T3 t3, T4 t4);
}

public interface IClosure<in T1, in T2, in T3, in T4, in T5, out TReturn>
{
    TReturn Apply(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5);
}

public interface IClosure<in T1, in T2, in T3, in T4, in T5, in T6, out TReturn>
{
    TReturn Apply(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6);
}

public interface IClosure<in T1, in T2, in T3, in T4, in T5, in T6, in T7, out TReturn>
{
    TReturn Apply(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7);
}

public interface IClosure<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, out TReturn>
{
    TReturn Apply(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8);
}

public interface IActionClosure
{
    void Apply();
}

public interface IActionClosure<in T1>
{
    void Apply(T1 t1);
}

public interface IActionClosure<in T1, in T2>
{
    void Apply(T1 t1, T2 t2);
}

public interface IActionClosure<in T1, in T2, in T3>
{
    void Apply(T1 t1, T2 t2, T3 t3);
}

public interface IActionClosure<in T1, in T2, in T3, in T4>
{
    void Apply(T1 t1, T2 t2, T3 t3, T4 t4);
}

public interface IActionClosure<in T1, in T2, in T3, in T4, in T5>
{
    void Apply(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5);
}

public interface IActionClosure<in T1, in T2, in T3, in T4, in T5, in T6>
{
    void Apply(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6);
}

public interface IActionClosure<in T1, in T2, in T3, in T4, in T5, in T6, in T7>
{
    void Apply(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7);
}

public interface IActionClosure<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8>
{
    void Apply(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8);
}
