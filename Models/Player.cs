using System;

namespace StarTaneousAPI.Models
{
    [Serializable]
    public class Player
    {
        public Guid StationId { get; set; }
        public List<Guid> FleetIds { get; set; }

        public Player(Guid stationId, List<Guid> fleetIds = null)
        {
            StationId = stationId;
            FleetIds = fleetIds ?? new List<Guid>() { Guid.NewGuid() };
        }
    }
}