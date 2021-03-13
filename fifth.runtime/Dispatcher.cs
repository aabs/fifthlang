namespace Fifth.Runtime
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///   The runtime component responsible for invoking functions on the stack, 
    ///   and resolving values on the stack.
    /// </summary>
    public class Dispatcher : IDispatcher
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Dispatcher" /> class.
        /// </summary>
        /// <param name="stack">The stack.</param>
        public Dispatcher(ActivationFrame frame) => Frame = frame;

        public IRuntimeStack Stack => Frame.Stack;

        /// <summary>
        ///     Gets or sets the instruction stack.
        /// </summary>
        /// <value>The stack.</value>
        public ActivationFrame Frame { get; set; }

        public void DispatchWhileNotEmpty()
        {
            while (!Stack.IsEmpty)
            {
                Dispatch();
            }
        }

        /// <summary>
        ///     Dispatch takes a function from the top of the stack, and then attempts to invoke it with
        ///     arguments gathered from the stack below
        /// </summary>
        /// <remarks>
        ///   If the element at the top of the stack is not a function of some sort, then it is left alone.
        ///   If it is a meta function then the dispatcher itself, including the activation frame, is passed 
        ///   to the meta function, giving it much more power over the runtime environment.
        ///   If it is a function, then metadata for the function wrapper is gathered, and used to resolves 
        ///   values from the stack.
        /// </summary>
        public void Dispatch()
        {
            // if x is a constant then return to stack
            var topOfStack = Stack.Pop();
            switch (topOfStack)
            {
                case ValueStackElement vse:
                    Stack.Push(vse);
                    break;
                case MetaFunctionStackElement mfe:
                    _ = mfe.MetaFunction.Invoke(this); // what would it mean to reassign the frame here...
                    break;
                case FunctionStackElement fe:
                    // if x is a func requiring (m) params: resolve m values into values
                    var args = new List<object>();
                    foreach (var t in fe.Function.ArgTypes)
                    {
                        var o = Resolve();
                        // check that types of values match type requirements of x
                        if (!t.IsInstanceOfType(o))
                        {
                            // TODO: maybe try to resolve an implicit coercion operator here.
                            throw new Exception("Invalid Parameter Type"); // TODO need better error message
                        }

                        args.Add(o);
                    }

                    // pass values to x as args
                    args.Reverse(); // return to same order they were passed onto the stack
                    var result = fe.Function.Invoke(args.ToArray());
                    // push result onto stack
                    Stack.PushConstantValue(result); // TODO: can't assume this will always be a value
                    break;
            }

        }

        /// <summary>
        ///     Resolve, attempts to convert functions at the top of the stack into actual values. If
        ///     they are real functions, then it recurses into those functions till it is able to get a
        ///     number off the stack to return.
        /// </summary>
        /// <returns>
        ///     an object. Either the value on the top of the stack, or the result of dispatching the
        ///     function on top of the stack.
        /// </returns>
        public object Resolve()
        {
            if (Stack.IsEmpty)
            {
                return null;
            }

            var x = Stack.Pop();
            switch (x)
            {
                case ValueStackElement vse:
                    return vse.Value;
                case VariableReferenceStackElement vrse:
                    var valueObject = Frame.Environment[vrse.VariableName];
                    var referencedVariableValue = ResolveTypedVariable(valueObject);
                    return referencedVariableValue;
            }

            // we can't resolve this value directly, we need to recurse via dispatch
            Stack.Push(x);
            Dispatch();
            x = Stack.Pop();
            // TODO: this code has issues - what if we need to support higher order functions?
            // functions that return functions will never resolve to a ValueStackElement here.
            // this piece needs to be more sophisticated...
            var result = x as ValueStackElement;
            return result?.Value;
        }

        public T Resolve<T>() where T:class => Resolve() as T;

        /// <summary>
        ///     try to resolve the type of the value and get its internal value
        /// </summary>
        /// <returns>Value if it can find it, as an object</returns>
        public object ResolveTypedVariable(IValueObject vo)
        {
            var pi = vo.GetType().GetProperty("Value");

            if (pi?.CanRead ?? false)
            {
                return pi!.GetMethod!.Invoke(vo, new object[] { });
            }

            return null;
        }
    }
}
