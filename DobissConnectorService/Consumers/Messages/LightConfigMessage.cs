using System.Text.Json.Serialization;

namespace DobissConnectorService.Consumers.Messages
{
    public class Device
    {
        public Device(string name, int module, int id)
        {
            identifiers = [$"dobiss_{module}x{id}"];
            manufacturer = "Dobiss";
            this.name = name;
            via_device = $"dobiss";
        }
        public List<string> identifiers { get; set; }
        public string manufacturer { get; set; }
        public string name { get; set; }
        public string via_device { get; set; }
    }

    public class LightConfigMessage
    {
        public LightConfigMessage(string name, int module, int id)
        {
            unique_id = $"dobiss_{module}x{id}".ToLower();
            cmd_t = $"homeassistant/light/set/{unique_id}";
            device = new Device(name, module, id);
            this.name = name;
            optimistic = false;
            schema = "json";
            stat_t = "~/state";
            brightness = false;
            Path = $"homeassistant/light/{unique_id}";
            command_template = "{ \"state\": {{ value }} }";
        }

        public string cmd_t { get; set; }
        public Device device { get; set; }
        public string name { get; set; }
        public bool optimistic { get; set; }
        public string schema { get; set; }
        public string stat_t { get; set; }
        public string unique_id { get; set; }
        public string command_template { get; set; }

        [JsonPropertyName("~")]
        public string Path { get; set; }
        public bool brightness { get; set; }
    }
}
