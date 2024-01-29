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

        public PartieDeChasse(Guid id, Terrain terrain, List<Chasseur> chasseurs)
            : this(id, terrain)
        {
            Chasseurs = chasseurs;
        }

        public PartieDeChasse(Guid id, Terrain terrain, List<Chasseur> chasseurs, List<Event> events,
            PartieStatus status)
            : this(id, terrain, chasseurs, status)
        {
            Events = events;
        }

        public PartieDeChasse(Guid id, Terrain terrain, List<Chasseur> chasseurs, PartieStatus status)
            : this(id, terrain, chasseurs)
        {
            Status = status;
        }


        public Guid Id { get; }
        public List<Chasseur> Chasseurs { get; }
        public Terrain Terrain { get; }
        public PartieStatus Status { get; set; }
        public List<Event> Events { get; init; }
    }
}