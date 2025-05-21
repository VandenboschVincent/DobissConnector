namespace DobissConnectorService.Dobiss.Models
{
    public class Light(int moduleKey, int key, ModuleType moduleType, string name, LightType lightType)
    {
        public int ModuleKey { get; } = moduleKey;
        public int Key { get; } = key;
        public ModuleType ModuleType { get; } = moduleType;
        public LightType LightType { get; } = lightType;
        public string Name { get; } = name;
        public int CurrentValue { get; set; }
    }
}
