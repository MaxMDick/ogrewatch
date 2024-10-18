using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
	public Rigidbody rb;
	public GameObject cameraHolder;
	public float speed, sensitivity, maxForce, jumpForce;
	private Vector2 move, look;
	private float lookRotation;
	public bool grounded;

	public Vector3 displayVelocityVector;
	public Vector2 displayMoveVector;

	public void OnMove(InputAction.CallbackContext context)
	{
		move = context.ReadValue<Vector2>();
	}

	public void OnLook(InputAction.CallbackContext context)
	{
		look = context.ReadValue<Vector2>();
	}

	public void OnJump(InputAction.CallbackContext context)
	{
		Jump();
	}

	public void SetGrounded(bool state)
	{
		grounded = state;
	}

	void Start()
	{
		Cursor.lockState = CursorLockMode.Locked;
	}

	void FixedUpdate()
	{
		Move();
		displayVelocityVector = rb.velocity;
		displayMoveVector = move;
		Debug.DrawRay(rb.transform.position, rb.velocity, Color.green, 0.01f);
	}

	void LateUpdate()
	{
		Look();
	}

	void Move()
	{
		// Find target velocity
		Vector3 currentVelocity = rb.velocity;
		Vector3 targetVelocity = new Vector3(
			move.x, 
			0, 
			move.y);
		targetVelocity *= speed;

		// Align direction with player
		targetVelocity = transform.TransformDirection(targetVelocity);

		// Calculate force
		Vector3 velocityChange = (targetVelocity - currentVelocity);
		velocityChange = new Vector3(velocityChange.x, 0, velocityChange.z);

		// Limit force
		Vector3.ClampMagnitude(velocityChange, maxForce);

		rb.AddForce(velocityChange, ForceMode.VelocityChange);
	}

	void Look()
	{
		// Turn
		transform.Rotate(Vector3.up * look.x * sensitivity);

		// Look
		lookRotation += (-look.y * sensitivity);
		lookRotation = Mathf.Clamp(lookRotation, -90, 90);
		cameraHolder.transform.eulerAngles = new Vector3(
			lookRotation, 
			cameraHolder.transform.eulerAngles.y, 
			cameraHolder.transform.eulerAngles.z);
	}

	void Jump()
	{
		if (grounded)
		{
			// Trying to fix bug of jump height being reduced
			// This is because the player is able to jump slightly before landing
			// And that's because the ground check collision box has to be slightly below the rigidbody
			//rb.velocity = new Vector3(
			//	rb.velocity.x, 
			//	0, 
			//	rb.velocity.z) ;
			Vector3 jumpForces = Vector3.zero;
			jumpForces = Vector3.up * jumpForce;
			rb.AddForce(jumpForces, ForceMode.VelocityChange);
		}
	}
}
