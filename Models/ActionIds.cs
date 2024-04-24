using System.Reflection;

namespace StartaneousAPI.Models
{
    public class ActionIds
    {
        public int actionTypeId;
        public int selectedStructureId;
        public List<int> selectedModulesId;
        public ActionIds()
        {
            selectedModulesId = new List<int>();
        }
    }
}
