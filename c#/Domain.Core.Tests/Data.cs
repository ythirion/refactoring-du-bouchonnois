using Domain.Core.Tests.Aggregates;
using FluentAssertions.Extensions;

namespace Domain.Core.Tests;

public static class Data
{
    public static readonly DateTime Now = new(2024, 6, 6, 14, 50, 45);
    public static readonly Func<DateTime> TimeProvider = () => Now;

    public static class Movies
    {
        public static class Oppenheimer
        {
            public const string Title = "Oppenheimer";
            public static readonly DateTime ReleaseDate = 19.July(2023);

            public static Movie Movie(Guid id) => new(id, TimeProvider, Title, ReleaseDate);
        }

        public static class Killers_of_the_Flower_Moon
        {
            public const string Title = "Killers of the Flower Moon";
            public static readonly DateTime ReleaseDate = 18.October(2023);

            public static Movie Movie(Guid id) => new(id, TimeProvider, Title, ReleaseDate);
        }
    }

    public static class Cinemas
    {
        public static class Grand_Rex
        {
            public const string Name = "Le Grand Rex";
            public const string City = "Paris";

            public static Cinema Cinema(Guid id) => new(id, TimeProvider, Name, City);
        }
    }
}