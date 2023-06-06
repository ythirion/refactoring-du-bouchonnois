using Bouchonnois.Service;

namespace Bouchonnois.Domain
{
    public class PartieDeChasse
    {
        public PartieDeChasse(Guid id, Terrain terrain)
        {
            Id = id;
            Chasseurs = new List<Chasseur>();
            Terrain = terrain;
            Status = PartieStatus.EnCours;
            Events = new List<Event>();
        }

        public PartieDeChasse(Guid id, Terrain terrain, List<Chasseur> chasseurs, List<Event> events)
            : this(id, terrain)
        {
            Chasseurs = chasseurs;
            Events = events;
        }

        public Guid Id { get; init; }
        public List<Chasseur> Chasseurs { get; init; }
        public Terrain Terrain { get; init; }
        public PartieStatus Status { get; set; }
        public List<Event> Events { get; init; }
    }
}