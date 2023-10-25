using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager
{
    public GameObject imgSnipeRifle;
    public GameObject imgTakeDamge;
    public GameObject weaponUIViewGo;
    public GameObject[] weaponUIGos;
    public Text textHealth;
    public Text textBulletNum;
    public GameObject imgDeadGo;
    public AudioClip lossClip;

    public void OpenOrCloseTelesopicView(bool open=true)
    {
        imgSnipeRifle.SetActive(open);
    }

    public void ShowTakeDamageView()
    {
        imgTakeDamge.SetActive(true);
        //CancelInvoke();
        //Invoke("HideTakeDamageView",2);
    }

    public void HideTakeDamageView()
    {
        imgTakeDamge.SetActive(false);
    }

    public void ShowOrHideWeaponUIView(bool show)
    {
        weaponUIViewGo.SetActive(show);
    }

    public void ChangeWeaponUIView(int id)
    {
        for (int i = 0; i < weaponUIGos.Length; i++)
        {
            weaponUIGos[i].SetActive(false);
        }
        weaponUIGos[id].SetActive(true);
    }

    public void UpdateHPValue(int value)
    {
        textHealth.text = value.ToString();
    }

    public void UpdateBulletNum(int curretNum,int totalNum)
    {
        textBulletNum.text = curretNum.ToString() + "/" + totalNum.ToString();
    }

    public void ShowDeadUI()
    {
        imgDeadGo.SetActive(true);
        //Invoke("PlayLossMusic",2);
    }

    private void PlayLossMusic()
    {
        Game.AudioSourceMgr.PlaySound(lossClip);
        //Invoke("LoadcurrentScene", 3);
    }

    private void LoadcurrentScene()
    {
        SceneManager.LoadScene(0);
    }
}
