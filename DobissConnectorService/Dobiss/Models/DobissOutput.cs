using System.Xml.Linq;

namespace DobissConnectorService.Dobiss.Models
{

    public class DobissOutput
    {
        public DobissOutput(int address, int status)
        {
            this.address = address;
            this.status = status;
        }
        public int address { get; }
        public int status { get; }
        public override string ToString()
        {
            return $"{address} {status}";
        }
    }
}
