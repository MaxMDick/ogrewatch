using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MyPlayerMovement : MonoBehaviour
{
	[Header("References")]
	private Rigidbody rb;
	public Transform cameraHolder;
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

	public float dotProduct;
	public float currentSpeed;


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
		//rb.AddForce(cameraHolder.forward * 10f, ForceMode.Impulse);
	}

	void Update()
	{
		isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight / 2 + 0.1f);
	}

	void FixedUpdate()
	{
		Move();
	}

	private void Move()
	{
		//// Find target velocity
		//currentVelocity = rb.velocity;
		//targetVelocity = new Vector3(move.x, 0, move.y);
		//targetVelocity *= groundAccel;

		//// Align direction with player camera
		//targetVelocity = cameraHolder.TransformDirection(targetVelocity);

		//// Calculate force
		//Vector3 velocityChange = (targetVelocity - currentVelocity);
		//// Limits force applied to only horizontal plane
		//velocityChange = new Vector3(velocityChange.x, 0, velocityChange.z); 
		//// Limit force value
		//Vector3.ClampMagnitude(velocityChange, maxForce);

		//rb.AddForce(velocityChange, ForceMode.VelocityChange);












		// If your going fast in one direction, forward input in that direction should not slow you down
		// I can't just change the drag attribute on the rigidbody, because I don't want airdrag to affect jump height (I want jump height to scale linearly with jump force for every class which can't be done if they have different drag values)







		//currentVelocity = rb.velocity;
		//currentSpeed = currentVelocity.magnitude;

		//targetVelocity = new Vector3(move.x, 0, move.y);
		//targetVelocity *= maxGroundSpeed;
		//targetSpeed = targetVelocity.magnitude;

		//targetVelocity = cameraHolder.TransformDirection(targetVelocity);

		//velocityChange = (targetVelocity - currentVelocity);
		//velocityChange = new Vector3(velocityChange.x, 0, velocityChange.z);

		//if (move == Vector2.zero) // No Player input
		//{
		//	velocityChange *= (groundDrag * Time.fixedDeltaTime);
		//}
		//else
		//{
		//	velocityChange *= (groundAccel * Time.fixedDeltaTime);
		//}

		//rb.AddForce(velocityChange, ForceMode.VelocityChange);



		current = rb.velocity;

		target = new Vector3(move.x, 0, move.y);
		target = orientation.TransformDirection(target);


		target = target.normalized;


		target = new Vector3(target.x, 0, target.z);
		target *= maxGroundSpeed;

		dotProduct = Vector3.Dot(current, target);
		correcting = target - current;

		drag = current - (Vector3.Dot(current, target) / target.magnitude) * target;
		drag = -drag;
		


		if (dotProduct < maxGroundSpeed * maxGroundSpeed)
		{
			rb.AddForce(groundAccel * Time.fixedDeltaTime * correcting.normalized, ForceMode.VelocityChange);
			rb.AddForce(groundDrag * Time.fixedDeltaTime * drag.normalized, ForceMode.VelocityChange);
		}

		currentSpeed = rb.velocity.magnitude;

	}

	private void Jump()
	{
		if (isGrounded)
		{
			rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
		}
	}
}
