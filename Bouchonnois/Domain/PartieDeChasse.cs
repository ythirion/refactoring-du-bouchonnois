using Bouchonnois.Service;

namespace Bouchonnois.Domain
{
    public class PartieDeChasse
    {
        public Guid Id { get; set; }
        public List<Chasseur> Chasseurs { get; set; }
        public Terrain Terrain { get; set; }
        public PartieStatus Status { get; set; }
        public List<Event> Events { get; set; }
    }
}