using DobissConnectorService.Dobiss.Models;
using Mediator;

namespace DobissConnectorService.Handlers.Messages
{
    public class ToggleLightMessage : ICommand
    {
        public ToggleLightMessage(Light light, int? newState)
        {
            Light = light;
            NewState = newState;
        }
        public Light Light { get; }
        public int? NewState { get; set; }
    }
}
