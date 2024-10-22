using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MyPlayerMovement : MonoBehaviour
{
	[Header("References")]
	private Rigidbody rb;
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
	public float abilityForce;
	public float playerHeight = 2f;
	public bool isGrounded;

	[Header("Inputs")] // should all be private serializefields
	public Vector2 move;

	public Vector3 current;
	public Vector3 target;
	public Vector3 correcting;

	public float cortarDot;
	public float currentSpeed;


	public void OnMove(InputAction.CallbackContext context)
	{
		move = context.ReadValue<Vector2>();
	}

	public void OnJump(InputAction.CallbackContext context)
	{
		Jump();
	}

	public void OnAbility1(InputAction.CallbackContext context)
	{
		Ability1();
	}

	void Start()
	{
		rb = GetComponent<Rigidbody>();
	}

	void Update()
	{
		isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight / 2 + 0.1f);



		currentSpeed = rb.velocity.magnitude;

		//float rayScale = 1f;
		//Vector3 aboveCube = transform.position + new Vector3(0, 1, 0);

		//Debug.DrawRay(aboveCube, current * rayScale, Color.red);
		//Debug.DrawRay(aboveCube, target * rayScale, Color.green);
		//Debug.DrawRay(aboveCube, correcting * rayScale, Color.blue);
	}

	void FixedUpdate()
	{
		Move();
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

	private void Jump()
	{
		if (isGrounded)
		{
			rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
		}
	}

	private void Ability1()
	{
		Vector3 abilityTarget = new Vector3(move.x, 0, move.y);
		abilityTarget = orientation.TransformDirection(abilityTarget);
		rb.AddForce(abilityForce * abilityTarget, ForceMode.Impulse);
	}
}
