namespace DobissConnectorService.Dobiss.Models
{
    public class DobissGroupData
    {
        public DobissGroupData(int id, string name)
        {
            this.id = id;
            this.name = name;
        }
        public int id { get; }
        public string name { get; }
        public override string ToString()
        {
            return $"{id} {name}";
        }
    }
}
