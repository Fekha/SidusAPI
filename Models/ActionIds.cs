using System.Reflection;

namespace StartaneousAPI.Models
{
    [Serializable]
    public class ActionIds
    {
        public int? actionTypeId { get; set; }
        public Guid? selectedUnitId { get; set; }
        public List<Guid>? selectedModulesIds { get; set; }
        public List<Coords>? selectedCoords { get; set; }
        public int? generatedModuleId { get; set; }
        public Guid? generatedGuid { get; set; }
    }
}
