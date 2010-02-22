using System;

namespace TiledMatrixInversion.ParallelBlockMatrixInverterSlim
{
    public class AbstractOperation
    {
        public readonly int I;
        public readonly int J;
        public readonly int K;

        public AbstractOperation() { }
        public AbstractOperation(int i) : this(i, 0, 0) { }
        public AbstractOperation(int i, int j) : this(i, j, 0) { }
        public AbstractOperation(int i, int j, int k)
        {
            I = i;
            J = j;
            K = k;
        }

        public override string ToString()
        {
            return string.Format("I = {0}, J = {1}, K = {2}", I, J, K);
        }
    }

    public class AbstractOperation<T> : AbstractOperation
    {
        public T OP;

        public AbstractOperation(T op) { OP = op; }
        public AbstractOperation(int i, T op) : base(i) { OP = op; }
        public AbstractOperation(int i, int j, T op) : base(i, j) { OP = op; }
        public AbstractOperation(int i, int j, int k, T op) : base(i, j, k) { OP = op; }

        public override string ToString()
        {
            return string.Format("{0}: I = {1}, J = {2}, K = {3}", OP, I, J, K);
        }
    }
}