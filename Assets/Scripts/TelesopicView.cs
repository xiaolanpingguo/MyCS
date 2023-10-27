using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TelesopicView : MonoBehaviour
{
    public float zoomLevel = 2;
    public float zoomSpeed=100;
    private float initFOV;
    private bool openTelesopicView;

    // Start is called before the first frame update
    void Start()
    {
        initFOV = Camera.main.fieldOfView;
    }

    // Update is called once per frame
    void Update()
    {
        if (openTelesopicView)
        {
            OpenTelesopicView();
        }
        else
        {
            CloseTelesopicView();
        }
    }

    private void OpenTelesopicView()
    {
        if (Camera.main.fieldOfView!=initFOV/zoomLevel)
        {
            if (Mathf.Abs(Camera.main.fieldOfView - initFOV / zoomLevel)<5 )
            {
                Camera.main.fieldOfView = initFOV / zoomLevel;
            }
            else
            {
                Camera.main.fieldOfView -= Time.deltaTime * zoomSpeed;
            }
        }
        //Game1.UIMgr.OpenOrCloseTelesopicView();
    }

    private void CloseTelesopicView()
    {
        if (Camera.main.fieldOfView != initFOV)
        {
            if (Mathf.Abs(Camera.main.fieldOfView - initFOV) < 5)
            {
                Camera.main.fieldOfView = initFOV;
            }
            else
            {
                Camera.main.fieldOfView += Time.deltaTime * zoomSpeed;
            }
        }
        //Game1.UIMgr.OpenOrCloseTelesopicView(false);
    }

    public void OpenTheTelesopicView(bool open=true)
    {
        openTelesopicView = open;
    }
}
