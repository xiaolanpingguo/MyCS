using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSourceManager
{
    private SoundPlayer m_soundPlayer;

    public AudioSourceManager(SoundPlayer soundPlayer)
    {
        m_soundPlayer = soundPlayer;
    }

    public void Init()
    {
        Game1.PoolMgr.InitPool(m_soundPlayer, 20);
    }

    public void PlaySound(AudioClip audioClip, float pitchMin = 1, float pitchMax = 1)
    {
        Game1.PoolMgr.GetInstance<SoundPlayer>(m_soundPlayer).PlayClip(audioClip,pitchMin,pitchMax);
    }
}
