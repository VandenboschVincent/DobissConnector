using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DobissConnectorService.Dobiss.Models
{
    public class Settings
    {
        public int Delay { get; set; } = 5000;
        [Required]
        public Dictionary<int, ModuleType> Modules { get; set; } = default!;
        [Required]
        public string DobissIp { get; set; } = default!;
        public int DobissPort { get; set; } = 10001!;
        [Required]
        public string MqttIp { get; set; } = default!;
        public int Mqttport { get; set; } = 1883;
        public string? MqttUser { get; set; }
        public string? MqttPassword { get; set; }
    }

}
