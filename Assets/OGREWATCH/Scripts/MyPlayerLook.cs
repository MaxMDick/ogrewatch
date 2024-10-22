using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MyPlayerLook : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	Transform cameraHolder;
	[SerializeField]
	Transform orientation;

	[Header("Sensitivity")]
	[SerializeField]
	private float sensX;
	[SerializeField]
	private float sensY;

	private Vector2 look;
	private Vector2 lookRotation;

	[SerializeField]
	[Tooltip("Enables a large range of sensitivity values")]
	float multiplier = 0.01f;

	public void OnLook(InputAction.CallbackContext context)
	{
		look = context.ReadValue<Vector2>();
	}

	void Start()
	{
		Cursor.lockState = CursorLockMode.Locked;
	}

	void LateUpdate()
	{
		Look();
	}

	private void Look()
	{
		lookRotation.y += (look.x * sensX * multiplier);
		lookRotation.x += (-look.y * sensY * multiplier);
		lookRotation.x = Mathf.Clamp(lookRotation.x, -90, 90);
		cameraHolder.localRotation = Quaternion.Euler(lookRotation.x, lookRotation.y, 0);
		orientation.rotation = Quaternion.Euler(0, lookRotation.y, 0);
	}
}
