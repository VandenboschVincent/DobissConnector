using DobissConnectorService.Dobiss.Models;
using Mediator;

namespace DobissConnectorService.CommandHandlers.Commands
{
    public class ChangeLightCommand : ICommand
    {
        public ChangeLightCommand(Light light, int? newState)
        {
            Light = light;
            NewState = newState;
        }
        public Light Light { get; }
        public int? NewState { get; set; }
    }
}
