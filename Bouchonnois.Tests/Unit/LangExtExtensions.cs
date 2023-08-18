using LanguageExt;

namespace Bouchonnois.Tests.Unit;

public static class LangExtExtensions
{
    public static TRight RightUnsafe<TLeft, TRight>(this Either<TLeft, TRight> either) => either.RightToSeq()[0];
}