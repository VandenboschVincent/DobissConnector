using System.Xml.Linq;

namespace DobissConnectorService.Dobiss.Models
{
    public class DobissModule
    {
        public DobissModule(int module, List<DobissOutput> outputs)
        {
            this.module = module;
            this.outputs = outputs;
        }
        public int module { get; }
        public List<DobissOutput> outputs { get; }

    }
}
