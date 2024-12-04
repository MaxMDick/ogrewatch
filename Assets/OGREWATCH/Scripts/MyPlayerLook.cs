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
	[SerializeField]
	MyPlayerStatus myPlayerStatus;

	[Header("Sensitivity")]
	[SerializeField]
	private float sensX;
	[SerializeField]
	private float sensY;

	private Vector2 m_look;
	private Vector2 lookRotation;

	[SerializeField]
	[Tooltip("Enables a large range of sensitivity values")]
	float multiplier = 0.01f;

	public void OnLook(InputAction.CallbackContext context)
	{
		m_look = context.ReadValue<Vector2>();
	}

	void Start()
	{
		Cursor.lockState = CursorLockMode.Locked;
		myPlayerStatus = GetComponent<MyPlayerStatus>();
	}

	void LateUpdate()
	{
		Look();
	}

	private Vector2 ProcessStatusEffects(Vector2 vector)
	{
		if (myPlayerStatus.IsStunned())
		{
			vector = new Vector2(0f, vector.y);
		}

		return vector;
	}

	private void Look()
	{
		Vector2 look = ProcessStatusEffects(m_look);
		lookRotation.y += (look.x * sensX * multiplier);
		lookRotation.x += (-look.y * sensY * multiplier);
		lookRotation.x = Mathf.Clamp(lookRotation.x, -90, 90);
		cameraHolder.localRotation = Quaternion.Euler(lookRotation.x, lookRotation.y, 0);
		orientation.rotation = Quaternion.Euler(0, lookRotation.y, 0);
	}
}
