using System;

namespace StarTaneousAPI.Models
{
    [Serializable]
    public class Player
    {
        public Guid StationGuid { get; set; }
        public List<Guid> FleetGuids { get; set; }

        public Player(Guid stationId, List<Guid>? fleetIds = null)
        {
            StationGuid = stationId;
            FleetGuids = fleetIds ?? new List<Guid>() { Guid.NewGuid() };
        }
    }
}