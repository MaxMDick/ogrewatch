using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteTimer : MonoBehaviour
{
    public float timeToDelete = 1f;
    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= timeToDelete)
        {
            Destroy(this.gameObject);
        }
    }
}
