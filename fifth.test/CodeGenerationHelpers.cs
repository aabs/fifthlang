namespace Fifth.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Fifth.Runtime;

    public static class CodeGenerationHelpers
    {
        public static bool Matches(this ActivationStack stack, params StackElement[] elementsToCompareAgainst)
        {
            var elements = stack.Export();
            var elementsFromStack = elements as List<StackElement> ?? elements.ToList();

            return elementsFromStack.SequenceEquals(elementsToCompareAgainst);
        }

        public static bool SequenceEquals<T>(this IEnumerable<T> seq1, IEnumerable<T> seq2)
        where T : IEquatable<T>
        {
            if ((seq1 == null && seq2 != null) || (seq2 == null && seq1 != null))
            {
                return false;
            }
            if (Object.ReferenceEquals(seq1, seq2))
                return true;
            var en1 = seq1.GetEnumerator();
            var en2 = seq2.GetEnumerator();

            var hasData = en1.MoveNext();
            en2.MoveNext();

            while (hasData)
            {
                if (!en1.Current.Equals(en2.Current))
                {
                    return false;
                }

                hasData = en1.MoveNext();
                en2.MoveNext();
            }

            return true;
        }
    }
}