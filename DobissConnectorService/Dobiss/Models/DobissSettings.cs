using System.ComponentModel.DataAnnotations;

namespace DobissConnectorService.Dobiss.Models
{
    public class DobissSettings
    {
        public int Delay { get; set; } = 5000;
        [Required]
        public string DobissIp { get; set; } = default!;
        public int DobissPort { get; set; } = 10001!;
       
    }

    public class MqttSettings
    {
        public string? MqttIp { get; set; }
        public int Mqttport { get; set; } = 1883;
        public string? MqttUser { get; set; }
        public string? MqttPassword { get; set; }
    }
}
