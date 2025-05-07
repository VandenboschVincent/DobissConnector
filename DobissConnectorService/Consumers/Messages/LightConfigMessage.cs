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

    public class DimLightConfigMessage : LightConfigMessage
    {
        public DimLightConfigMessage(string name, int module, int id) : base(name, module, id)
        {
            brightness = true;
            brightness_scale = 100;
            brightness_command_topic = $"homeassistant/light/set/{unique_id}";
            brightness_state_topic = $"~/state";
            on_command_type = "brightness";
            brightness_value_template = "{{ value | float | int }}";
        }

        public int brightness_scale { get; set; }
        public string on_command_type { get; set; }
        public string brightness_state_topic { get; set; }
        public string brightness_command_topic { get; set; }
        public string brightness_value_template { get; set; }
    }

    public class LightConfigMessage
    {
        public LightConfigMessage(string name, int module, int id)
        {
            unique_id = $"dobiss_{module}x{id}".ToLower();
            command_topic = $"homeassistant/light/set/{unique_id}";
            device = new Device(name, module, id);
            this.name = name;
            optimistic = false;
            schema = "json";
            state_topic = "~/state";
            brightness = false;
            Path = $"homeassistant/light/{unique_id}";
            state_value_template = "{{ value | float | int }}";
        }
        public string command_topic { get; set; }
        public Device device { get; set; }
        public string name { get; set; }
        public bool optimistic { get; set; }
        public string schema { get; set; }
        public string state_topic { get; set; }
        public string unique_id { get; set; }
        public string state_value_template { get; set; }

        [JsonPropertyName("~")]
        public string Path { get; set; }
        public bool brightness { get; set; }
    }
}
