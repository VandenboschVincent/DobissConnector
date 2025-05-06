using DobissConnectorService.Dobiss.Models;
using Mediator;

namespace DobissConnectorService.Handlers.Messages
{
    public class DimLightMessage : ICommand
    {
        public DimLightMessage(Light light, int newState)
        {
            Light = light;
            NewState = newState;
        }
        public Light Light { get; }
        public int NewState { get; set; }
    }
}
