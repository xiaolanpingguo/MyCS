using Lockstep.Game;
using Lockstep.Logging;
using UnityEngine;


public class Game : MonoBehaviour 
{
    public int MaxEnemyCount = 10;
    public bool IsClientMode = false;
    public bool IsRunVideo;
    public bool IsVideoMode = false;
    public string RecordFilePath;
    public bool HasInit = false;

    private Launcher m_launcher = new Launcher();
    private ServiceContainer m_serviceContainer;

    private void Awake()
    {
        gameObject.AddComponent<PingMono>();
        gameObject.AddComponent<InputMono>();
        m_serviceContainer = new UnityServiceContainer();
        m_serviceContainer.GetService<IConstStateService>().GameName = "ARPGDemo";
        m_serviceContainer.GetService<IConstStateService>().IsClientMode = IsClientMode;
        m_serviceContainer.GetService<IConstStateService>().IsVideoMode = IsVideoMode;
        m_serviceContainer.GetService<IGameStateService>().MaxEnemyCount = MaxEnemyCount;
        Lockstep.Logging.Logger.OnMessage += OnLog;
        Screen.SetResolution(1024, 768, false);

        m_launcher.DoAwake(m_serviceContainer);
    }

    private void Start()
    {
        var stateService = GetService<IConstStateService>();
        string path = Application.dataPath;
#if UNITY_EDITOR
        path = Application.dataPath + "/../../../";
#elif UNITY_STANDALONE_OSX
        path = Application.dataPath + "/../../../../../";
#elif UNITY_STANDALONE_WIN
        path = Application.dataPath + "/../../../";
#endif
        UnityEngine.Debug.Log("log path set to: " + path);
        stateService.RelPath = path;
        m_launcher.DoStart();
        HasInit = true;
    }

    private void Update()
    {
        m_serviceContainer.GetService<IConstStateService>().IsRunVideo = IsVideoMode;
        m_launcher.DoUpdate(Time.deltaTime);
    }

    private void OnDestroy()
    {
        m_launcher.DoDestroy();
    }

    private void OnApplicationQuit()
    {
        m_launcher.OnApplicationQuit();
    }

    public T GetService<T>() where T : IService
    {
        return m_serviceContainer.GetService<T>();
    }

    private static void OnLog(object sender, LogEventArgs args)
    {
        switch (args.LogSeverity)
        {
            case LogSeverity.Info:
                UnityEngine.Debug.Log(args.Message);
                break;
            case LogSeverity.Warn:
                UnityEngine.Debug.LogWarning(args.Message);
                break;
            case LogSeverity.Error:
                UnityEngine.Debug.LogError(args.Message);
                break;
            case LogSeverity.Exception:
                UnityEngine.Debug.LogError(args.Message);
                break;
        }
    }
}