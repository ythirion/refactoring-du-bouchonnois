using Domain.Core;
using LanguageExt;
using LanguageExt.UnsafeValueAccess;
using static System.String;
using static Domain.Core.AsyncHelper;

namespace Bouchonnois.Domain;

public sealed partial class PartieDeChasse
{
    public Either<Error, string> Consulter(IPartieDeChasseRepository repository)
        => RunSync(async () => await repository.EventsFor(Id)
            .Map(FormatEvents)
            .ValueUnsafe()
        );

    private static string FormatEvents(Seq<IEvent> events)
        => Join(Environment.NewLine,
            events.Map(@event => $"{@event.Date:HH:mm} - {@event}")
        );
}