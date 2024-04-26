using System;

namespace StarTaneousAPI.Models
{
    [Serializable]
    public class Player
    {
        public Guid StationId { get; set; }
        public List<Guid> ShipIds { get; set; }

        public Player(Guid stationId, List<Guid> shipIds = null)
        {
            StationId = stationId;
            ShipIds = shipIds ?? new List<Guid>() { Guid.NewGuid() };
        }
    }
}