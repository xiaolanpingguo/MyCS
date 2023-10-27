using Lockstep.Game;


public class BaseGameServicesContainer : ServiceContainer 
{
    public BaseGameServicesContainer()
    {
        // engine common
        RegisterService(new RandomService());
        RegisterService(new CommonStateService());
        RegisterService(new ConstStateService());
        RegisterService(new IdService());

        RegisterService(new SimulatorService());
        RegisterService(new NetworkService());
        RegisterService(new GameResourceService());
        RegisterService(new GameStateService());
        RegisterService(new GameConfigService());
        RegisterService(new GameInputService());
    }
}