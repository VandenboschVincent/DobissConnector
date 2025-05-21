using DobissConnectorService.Dobiss.Models;
using Mediator;

namespace DobissConnectorService.CommandHandlers.Commands
{
    public class ChangeLightCommand(Light light, int? newState) : ICommand
    {
        public Light Light { get; } = light;
        public int? NewState { get; set; } = newState;
    }
}
