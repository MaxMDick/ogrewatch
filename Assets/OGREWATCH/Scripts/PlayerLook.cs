using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLook : MonoBehaviour
{
	[SerializeField]
	private float sensX;
	[SerializeField]
	private float SensY;
	[SerializeField]
	Transform cameraHolder;
	[SerializeField]
	Transform orientation;

	float mouseX;
	float mouseY;

	float multiplier = 0.01f;

	float xRotation;
	float yRotation;

	void Start()
	{
		//cam = GetComponentInChildren<Camera>();

		Cursor.lockState = CursorLockMode.Locked;
	}

	void Update()
	{
		MyInput();
		cameraHolder.localRotation = Quaternion.Euler(xRotation, yRotation, 0);
		orientation.rotation = Quaternion.Euler(0, yRotation, 0);
	}

	private void MyInput()
	{
		mouseX = Input.GetAxisRaw("Mouse X");
		mouseY = Input.GetAxisRaw("Mouse Y");

		yRotation += mouseX * sensX * multiplier; // Rotation on y-axis in Unity is horizontal
		xRotation -= mouseY * SensY * multiplier; // Rotation on x-axis in Unity is vertical

		xRotation = Mathf.Clamp(xRotation, -90, 90f);
	}
}
