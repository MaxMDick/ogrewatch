using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
	[Header("References")]
	public Rigidbody rb;
	public GameObject cameraHolder;

	[Header("General")]
	public float speed, sensitivity, maxForce, jumpForce, jumpWaitTime;	

	[Header("Player Inputs")]
	[SerializeField]
	private Vector2 look;
	[SerializeField]
	private Vector2 move;

	[Header("Status")]
	[SerializeField]
	private Vector2 lookRotation;
	public Vector3 displayVelocityVector;
	public bool grounded;
	[SerializeField]
	private float timeGrounded = 0.0f;

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
		if (!grounded && state)
		{
			timeGrounded = 0.0f;
		}
		grounded = state;
	}

	void Start()
	{
		Cursor.lockState = CursorLockMode.Locked;
	}

	void Update()
	{
		if (grounded)
		{
			timeGrounded += Time.deltaTime;
		}
	}

	void FixedUpdate()
	{
		Move();
		displayVelocityVector = rb.velocity;
		//Debug.DrawRay(rb.transform.position, rb.velocity, Color.green, 0.01f);
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

		// Align direction with player camera
		targetVelocity = cameraHolder.transform.TransformDirection(targetVelocity);

		// Calculate force
		Vector3 velocityChange = (targetVelocity - currentVelocity);
		velocityChange = new Vector3(velocityChange.x, 0, velocityChange.z);

		// Limit force
		Vector3.ClampMagnitude(velocityChange, maxForce);

		rb.AddForce(velocityChange, ForceMode.VelocityChange);
	}

	void Look()
	{
		lookRotation.x += (look.x * sensitivity);
		lookRotation.y += (-look.y * sensitivity);
		lookRotation.y = Mathf.Clamp(lookRotation.y, -90, 90);
		cameraHolder.transform.eulerAngles = new Vector3(
			lookRotation.y,
			lookRotation.x,
			cameraHolder.transform.eulerAngles.z);
	}

	void Jump()
	{
		//if (grounded && rb.velocity.y >= 0)
 
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
