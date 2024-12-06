using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{
	[SerializeField]
	Transform cameraPosition;

	public float shakeTiltAngle;
	public float shakeTime;
	
	public float timer = 0f;
	public bool shook = false;

    void Update()
    {
		transform.position = cameraPosition.position;
		

		if (shook)
		{
			if (timer < shakeTime)
			{
				timer += Time.deltaTime;
			}
			else
			{
				shook = false;
				transform.Rotate(0, 0, -shakeTiltAngle);
			}
		}
    }

    public void CameraShake()
    {
	    if (!shook)
	    {
		    Debug.Log("Camera Shake");
		    shook = true;
		    timer = 0f;
		    transform.Rotate(0, 0, shakeTiltAngle);
	    }
    }
}
