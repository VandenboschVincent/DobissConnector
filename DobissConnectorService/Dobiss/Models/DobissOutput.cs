namespace DobissConnectorService.Dobiss.Models
{
    public class DobissOutput(int index, int moduleIndex, LightType type, byte groupIndex, string name)
    {
        public int Index { get; } = index;
        public int ModuleIndex { get; } = moduleIndex;
        public LightType Type { get;  } = type;
        public byte GroupIndex { get;  } = groupIndex;
        public string Name { get; } = name;
    }
}
