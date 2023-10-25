using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShaker : MonoBehaviour
{
    private Vector3 initPosition;
    private float remainingShakeTime;
    private float shakeStrength;

    // Start is called before the first frame update
    void Start()
    {
        initPosition = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if (remainingShakeTime>0)
        {
            remainingShakeTime -= Time.deltaTime;
            if (remainingShakeTime<=0)
            {
                transform.localPosition = initPosition;
            }
            else
            {
                Vector3 randomDir = Random.insideUnitSphere;
                transform.localPosition = initPosition + shakeStrength*randomDir;
            }
        }
    }

    public void SetShakeValue(float shakeTime,float strength)
    {
        remainingShakeTime = shakeTime;
        shakeStrength = strength;
    }
}
