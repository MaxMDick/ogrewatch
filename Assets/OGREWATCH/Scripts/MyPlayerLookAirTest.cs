using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MyPlayerLookAirTest : MonoBehaviour
{
	[Header("References")]
	// [SerializeField]
	// Transform cameraHolder;
	[SerializeField]
	Transform orientation;

	[Header("Sensitivity")]
	[SerializeField]
	private float sensX;
	[SerializeField]
	private float sensY;

	public Vector2 m_look;
	private Vector2 lookRotation;
	private int m_look_last;

	[SerializeField]
	[Tooltip("Enables a large range of sensitivity values")]
	float multiplier = 0.01f;
	
	
	
	[Header("Spine Rotation Testing")]
	public Transform spineToRotate;
	public float maxLookUpAngle;
	public float maxLookDownAngle;
	public Transform hipToRotate;
	public MyPlayerMovement myPlayerMovement;

	public void OnLook(InputAction.CallbackContext context)
	{
		m_look = context.ReadValue<Vector2>();
		if (m_look.x > 0)
		{
			m_look_last = 1;
		}
		else if (m_look.x < 0)
		{
			m_look_last = -1;
		}
	}

	public int LastLookDirectionX()
	{
		return m_look_last;
	}

	void Start()
	{
		Cursor.lockState = CursorLockMode.Locked;
	}

	void LateUpdate()
	{
		Look();
	}

	private Vector2 ProcessStatusEffects(Vector2 vector)
	{
		// if (myPlayerStatus.IsStunned())
		// {
		// 	vector = new Vector2(0f, vector.y);
		// }

		return vector;
	}

	private void Look()
	{
		Vector2 look = ProcessStatusEffects(m_look);
		lookRotation.y += (look.x * sensX * multiplier);
		lookRotation.x += (-look.y * sensY * multiplier);
		lookRotation.x = Mathf.Clamp(lookRotation.x, -maxLookUpAngle, maxLookDownAngle);
		orientation.rotation = Quaternion.Euler(0, lookRotation.y, 0);

		if (myPlayerMovement.isGrounded)
		{
			spineToRotate.localEulerAngles = new Vector3(lookRotation.x, spineToRotate.localRotation.y, spineToRotate.localEulerAngles.z);
			hipToRotate.localEulerAngles = new Vector3(-90, hipToRotate.localEulerAngles.y, hipToRotate.localEulerAngles.z);
		}
		else
		{
			spineToRotate.localEulerAngles = new Vector3(0, spineToRotate.localRotation.y, spineToRotate.localEulerAngles.z);
			hipToRotate.localEulerAngles = new Vector3(lookRotation.x - 90, hipToRotate.localEulerAngles.y, hipToRotate.localEulerAngles.z);
		}
	}
}
