using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TestMyPlayerMovement : MonoBehaviour
{
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

	[Header("References")]
	private Rigidbody rb;
	//public Transform cameraHolder;
	public Transform orientation;

	[Header("Max Speed")]
	public float maxGroundSpeed;
	public float maxAirSpeed;
	[Header("Acceleration")] // Acceleration applied for player movement inputs
	public float groundAccel;
	public float airAccel;
	[Header("Drag")] // Drag applied when player is not inputting any movements
	public float groundDrag;
	public float airDrag;
	[Header("Jump")]
	public float jumpForce;
	[Header("Other")]
	public float maxForce = 100f;
	public float playerHeight = 2f;
	public bool isGrounded;

	[Header("Inputs")] // should all be private serializefields
	public Vector2 move;

	public Vector3 current;
	public Vector3 target;
	public Vector3 correcting;

	public float cortarDot;
	public float currentSpeed;

	public void OnLook(InputAction.CallbackContext context)
	{
		look = context.ReadValue<Vector2>();
	}

	public void OnMove(InputAction.CallbackContext context)
	{
		move = context.ReadValue<Vector2>();
	}

	public void OnJump(InputAction.CallbackContext context)
	{
		Jump();
	}

	void Start()
	{
		rb = GetComponent<Rigidbody>();
		Cursor.lockState = CursorLockMode.Locked;
	}

	void Update()
	{
		isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight / 2 + 0.1f);


		currentSpeed = rb.velocity.magnitude;

		float rayScale = 1f;
		Vector3 aboveCube = transform.position + new Vector3(0, 1, 0);
		
		Debug.DrawRay(aboveCube, current * rayScale, Color.red);
		Debug.DrawRay(aboveCube, target * rayScale, Color.green);
		Debug.DrawRay(aboveCube, correcting * rayScale, Color.blue);
	}

	void FixedUpdate()
	{
		Move();
	}

	void LateUpdate()
	{
		Look();
	}

	private void Move()
	{
		current = rb.velocity;
		current = new Vector3(current.x, 0, current.z);

		target = new Vector3(move.x, 0, move.y);
		target = orientation.TransformDirection(target);
		target = new Vector3(target.x, 0, target.z);
		if (isGrounded)
		{
			target *= maxGroundSpeed;
		}
		else
		{
			target *= maxAirSpeed;
		}
		

		correcting = target - current;
		cortarDot = Vector3.Dot(correcting, target);

		if (cortarDot > 0) // Player has input, but not yet up to target velocity
		{
			if (isGrounded)
			{
				correcting *= groundAccel;
			}
			else
			{
				correcting *= airAccel;
			}
		}
		else if (cortarDot == 0) // Player has no input
		{
			if (isGrounded)
			{
				correcting *= groundDrag;
			}
			else
			{
				correcting *= airDrag;
			}
		}

		rb.AddForce(Time.fixedDeltaTime * correcting, ForceMode.VelocityChange);
	}

	private void Look()
	{
		lookRotation.y += (look.x * sensX * multiplier);
		lookRotation.x += (-look.y * sensY * multiplier);
		lookRotation.x = Mathf.Clamp(lookRotation.x, -90, 90);
		//cameraHolder.localRotation = Quaternion.Euler(lookRotation.x, lookRotation.y, 0);
		orientation.rotation = Quaternion.Euler(0, lookRotation.y, 0);
	}

	private void Jump()
	{
		if (isGrounded)
		{
			rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
		}
	}
}
