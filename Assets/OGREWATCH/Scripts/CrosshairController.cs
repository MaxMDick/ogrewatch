using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrosshairController : MonoBehaviour
{
    public Camera myCamera;
    public GameObject whiteCrosshair;
    public GameObject orangeCrosshair;
    RaycastHit hitInfo;

    void Update()
    {
        if (Physics.Raycast(myCamera.transform.position, myCamera.transform.forward, out hitInfo))
        {
            if (hitInfo.transform.GetComponent<Damageable>())
            {
                whiteCrosshair.SetActive(false);
                orangeCrosshair.SetActive(true);
            }
            else
            {
                orangeCrosshair.SetActive(false);
                whiteCrosshair.SetActive(true);
            }
        }
    }
}
