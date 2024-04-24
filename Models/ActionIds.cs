using System.Reflection;

namespace StartaneousAPI.Models
{
    [Serializable]
    public class ActionIds
    {
        public int? actionTypeId { get; set; }
        public Guid? selectedStructureId { get; set; }
        public List<Guid>? selectedModulesIds { get; set; }
    }
}
