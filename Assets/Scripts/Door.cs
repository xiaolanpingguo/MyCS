using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject keyGo;
    public bool unLocked;
    public float moveSpeed;
    public AudioClip openDoorClip;
    public AudioClip cantOpenClip;
    public AudioClip doorClip;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            if (!unLocked)
            {
                if (keyGo == null)
                {
                    unLocked = true;
                    Game1.AudioSourceMgr.PlaySound(openDoorClip);
                    Game1.AudioSourceMgr.PlaySound(doorClip);
                }
                else
                {
                    Game1.AudioSourceMgr.PlaySound(cantOpenClip);
                }
            }
            
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (unLocked)
        {
            transform.Translate(-Vector3.up * Time.deltaTime * moveSpeed * 0.3f);
            if (transform.position.y <= -1.4)
            {
                Destroy(gameObject);
            }
        }
    }
}
