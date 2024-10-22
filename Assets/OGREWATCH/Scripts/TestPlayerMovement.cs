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
	public float groundStop;
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
	public Vector3 drag;
	public Vector3 realDrag;

	public float dotProduct;
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




		Vector3 aboveCube = transform.position + new Vector3(0, 1, 0);
		float rayScale = 1f;

		Debug.DrawRay(aboveCube, current * rayScale, Color.red);
		Debug.DrawRay(aboveCube, target * rayScale, Color.green);
		Debug.DrawRay(aboveCube, correcting * rayScale, Color.blue);
		//Debug.DrawRay(current, current + target, Color.yellow);
		Debug.DrawRay(aboveCube, drag * rayScale, Color.gray);
		Debug.DrawRay(aboveCube, realDrag * rayScale, Color.black);
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

		target = new Vector3(move.x, 0, move.y);
		target = orientation.TransformDirection(target).normalized;


		drag = new Vector3(move.y, 0, -move.x);
		drag = orientation.TransformDirection(drag).normalized;


		target = new Vector3(target.x, 0, target.z);
		target *= maxGroundSpeed;

		dotProduct = Vector3.Dot(current.normalized, target.normalized);
		correcting = target - current;


		if (Vector3.Dot(current, drag) > 0)
		{
			realDrag = -drag;
		}
		else
		{
			realDrag = drag;
		}

		if (Vector3.Dot(current, drag) == 0)
		{
			realDrag = Vector3.zero;
		}

		if (target == Vector3.zero && current.magnitude < 1f)
		{
			correcting = Vector3.zero;
		}


		if (dotProduct < 1)
		{
			rb.AddForce(groundAccel * Time.fixedDeltaTime * correcting.normalized, ForceMode.VelocityChange);
			rb.AddForce(groundDrag * Time.fixedDeltaTime * realDrag.normalized, ForceMode.VelocityChange);
		}

		currentSpeed = rb.velocity.magnitude;

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
