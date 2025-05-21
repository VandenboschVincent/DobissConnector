using DobissConnectorService.Dobiss.Utils;
using System.Xml.Linq;

namespace DobissConnectorService.Dobiss.Models
{
    public class DobissModule(int index, ModuleType type, bool isMaster, int outputCount)
    {
        public int Index { get; } = index;
        public ModuleType Type { get; } = type;
        public bool IsMaster { get; } = isMaster;
        public int OutputCount { get; } = outputCount;
        public List<DobissOutput> Outputs { get; set; } = [];

    }
}
