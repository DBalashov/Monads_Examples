using System;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public readonly struct Either<L, R>
    {
        readonly L    left;
        readonly R    right;
        readonly bool isLeft;

        public Either(R value)
        {
            isLeft = false;
            left   = default;
            right  = value;
        }

        public Either(L value)
        {
            isLeft = true;
            left   = value;
            right  = default;
        }

        #region TryGetValue

        public bool TryGetValue(out L value)
        {
            if (isLeft)
            {
                value = left;
                return true;
            }

            value = default;
            return false;
        }

        public bool TryGetValue(out R value)
        {
            if (!isLeft)
            {
                value = right;
                return true;
            }

            value = default;
            return false;
        }

        #endregion

        #region Case

        public void Case(Action<L> actionL, Action<R> actionR)
        {
            if (isLeft) actionL(left);
            else actionR(right);
        }

        public RESULT Case<RESULT>(Func<L, RESULT> actionL, Func<R, RESULT> actionR) =>
            isLeft ? actionL(left) : actionR(right);

        public Task<RESULT> Case<RESULT>(Func<L, Task<RESULT>> actionL, Func<R, Task<RESULT>> actionR) =>
            isLeft ? actionL(left) : actionR(right);

        #endregion

        public L GetValueOrDefault(L       defaultValue) => isLeft ? left : defaultValue;
        public R GetValueOrDefault(R       defaultValue) => !isLeft ? right : defaultValue;
        public L GetValueOrDefault(Func<L> defaultValue) => isLeft ? left : defaultValue();
        public R GetValueOrDefault(Func<R> defaultValue) => !isLeft ? right : defaultValue();
        
        #region Equals / GetHashCode

        public override bool Equals(object? obj) =>
            obj is Either<L, R> e && (isLeft == e.isLeft && ((isLeft && Equals(left, e.left)) || (!isLeft && Equals(right, e.right))));

        public override int GetHashCode() => isLeft.GetHashCode() ^ (isLeft ? left.GetHashCode() : right.GetHashCode());

        #endregion

        #region L -> Either<L,R> / R -> Either<L,R>

        public static implicit operator Either<L, R>(L value) => new(value);

        public static implicit operator Either<L, R>(R value) => new(value);

        #endregion

#if DEBUG
        public override string ToString() => (isLeft ? left?.ToString() : right?.ToString()) ?? "Empty";
#endif
    }
}