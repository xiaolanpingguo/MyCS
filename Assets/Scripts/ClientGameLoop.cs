using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientGameLoop
{
    private InputManager m_inputManager;

    public ClientGameLoop()
    {
        m_inputManager = new InputManager();
        m_inputManager.Init();
    }

    public void Update()
    {
        m_inputManager.Update();
    }
}
