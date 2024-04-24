using System.Reflection;

namespace StartaneousAPI.Models
{
    public class ActionIds
    {
        public int actionTypeId { get; set; }
        public Guid? selectedStructureId { get; set; }
        public List<int>? selectedModulesIds { get; set; }
    }
}
