using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    public static Game Instance { get; private set; }

    public static AudioSourceManager AudioSourceMgr { get; private set; }
    public static PoolManager PoolMgr { get; private set; }
    public static UIManager UIMgr { get; private set; }

    public SoundPlayer SoundPlayer;

    // Start is called before the first frame update
    void Awake()
    {
        Instance = this;
        PoolMgr = new PoolManager();

        AudioSourceMgr = new AudioSourceManager(SoundPlayer);
        AudioSourceMgr.Init();
        UIMgr = new UIManager();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
