using LanguageExt;

namespace Bouchonnois.Tests
{
    public static class LangExtExtensions
    {
        public static TRight RightUnsafe<TLeft, TRight>(this EitherAsync<TLeft, TRight> either) =>
            either.RightToSeq().Result[0];

        public static TRight RightUnsafe<TLeft, TRight>(this Either<TLeft, TRight> either) => either.RightToSeq()[0];

        public static TLeft LeftUnsafe<TLeft, TRight>(this EitherAsync<TLeft, TRight> either) =>
            either.LeftToSeq().Result[0];
    }
}