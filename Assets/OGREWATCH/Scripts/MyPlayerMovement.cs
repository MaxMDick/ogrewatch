using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class MyPlayerMovement : MonoBehaviour
{
	[Header("References")]
	private Rigidbody rb;
	private CapsuleCollider cc;
	public Transform orientation;
	public MyPlayerStatus myPlayerStatus;
	public Animator animator;
	public CameraEffects cameraEffects;

	[Header("Ground Movement")]
	// public float maxGroundSpeed;
	public float groundAccel;
	public float groundDrag;
	public float forwardSpeed;
	public float sideSpeed;
	public float backSpeed;

	[Header("Air Movement")]
	public float maxAirSpeed;
	public float airAccel;
	public float airDrag;

	[Header("Gravity")]
	// public float gravity;
	public float gravityAscending;
	public float gravityDescending;

	[Header("Ground Check")]
	public float groundCheckRadiusMultiplier = 0.9f;
	public float groundCheckDistance = 0.05f;
	RaycastHit groundCheckHit = new();

	[Header("Slope Check")]
	public float slopeCheckRadiusMultiplier = 0.9f;
	public float slopeCheckDistance = 0.05f;
	public float maxSlopeAngle;
	public float slopeEscapeSpeed;
	public float recentlyJumpedPeriod = 0.2f; // Must be longer than coyoteTimePeriod
	public float coyoteTimePeriod = 0.1f;
	RaycastHit slopeCheckHit = new();
	float lastGroundedAtTime;
	float lastJumpedAtTime;

	[Header("Jump")]
	public float jumpForce;

	[Header("Other")]
	public float abilityForce;
	public float playerHeight = 2f;
	

	[Header("Status")] // should all be private serializefields
	public bool jumpPressed;
	public bool isGrounded;
	public float slopeAngle;
	public Vector2 move;
	public Vector3 current;
	public Vector3 target;
	public Vector3 correcting;
	public float cortarDot;
	
	
	// For display only
	public float currentSpeed;
	public Vector3 currentVelocity;
	public bool coyoteTime;
	public bool recentlyJumped;

	public bool landedThisFrame;

	public float forwardDiagonalSpeed;
	public float backDiagonalSpeed;

	public void OnMove(InputAction.CallbackContext context)
	{
		move = context.ReadValue<Vector2>();
	}

	public void OnJump(InputAction.CallbackContext context)
	{
		jumpPressed = context.ReadValue<float>() > 0f;
	}

	public void OnAbility1(InputAction.CallbackContext context)
	{
		Ability1();
	}

	void Start()
	{
		rb = GetComponent<Rigidbody>();
		cc = GetComponent<CapsuleCollider>();
		myPlayerStatus = GetComponent<MyPlayerStatus>();
		rb.useGravity = false;

		float x, y;
		// angleRadians = Math.Atan(sideSpeed / forwardSpeed);
		// Debug.Log(angleRadians);
		// x = sideSpeed * (float)Math.Cos(angleRadians); // or forwardSpeed * sin
		// diagonalForwardSpeed = new Vector2(x, x).magnitude;
		x = sideSpeed * (float)Math.Cos(Math.PI / 4);
		y = forwardSpeed * (float)Math.Sin(Math.PI / 4);
		// Debug.Log(x);
		// Debug.Log(y);
		forwardDiagonalSpeed = new Vector2(x, y).magnitude;
		
		x = sideSpeed * (float)Math.Cos(Math.PI / 4);
		y = backSpeed * (float)Math.Sin(Math.PI / 4);
		// Debug.Log(x);
		// Debug.Log(y);
		backDiagonalSpeed = new Vector2(x, y).magnitude;
	}

	void Update()
	{
		UpdateAnimator();
	}

	void FixedUpdate()
	{
		landedThisFrame = false;
		bool wasGrounded = isGrounded;
		isGrounded = GroundDetection();
		if (wasGrounded && !isGrounded)
		{
			lastGroundedAtTime = Time.time;
		}
		else if (!wasGrounded && isGrounded)
		{
			landedThisFrame = true;
		}

		slopeAngle = SlopeDetection();

		if (jumpPressed)
		{
			TryJump();
		}
		TrySnapToPlane();
		Move();
		ApplyGravity();



		// For display
		currentSpeed = rb.velocity.magnitude;
		currentVelocity = rb.velocity;
		coyoteTime = IsInCoyoteTime();
		recentlyJumped = RecentlyJumped();
	}

	private bool GroundDetection()
	{ 
		// https://www.youtube.com/watch?v=SgBFtsckCJE&list=PLxAJ_iP7Q93v70WAqtPoA8MYNLJeIKvPU&index=8
		float sphereCastRadius = cc.radius * groundCheckRadiusMultiplier;
		float sphereCastTravelDistance = cc.bounds.extents.y - sphereCastRadius + groundCheckDistance;
		return Physics.SphereCast(rb.position, sphereCastRadius, Vector3.down, out groundCheckHit, sphereCastTravelDistance);
	}

	private float SlopeDetection()
	{
		float sphereCastRadius = cc.radius * slopeCheckRadiusMultiplier;
		float sphereCastTravelDistance = cc.bounds.extents.y - sphereCastRadius + slopeCheckDistance;
		Physics.SphereCast(rb.position, sphereCastRadius, Vector3.down, out slopeCheckHit, sphereCastTravelDistance);
		return Vector3.Angle(Vector3.up, slopeCheckHit.normal);
	}

	private bool IsSlopeTraversable()
	{
		return slopeAngle <= maxSlopeAngle;
	}

	private bool IsOnGround()
	{
		return isGrounded;
	}

	private bool IsInCoyoteTime()
	{
		return !RecentlyJumped() && Time.time < lastGroundedAtTime + coyoteTimePeriod;
	}

	private bool RecentlyJumped()
	{
		return Time.time < lastJumpedAtTime + recentlyJumpedPeriod;
	}

	private Vector3 ProcessStatusEffects(Vector3 vector)
	{
		if (myPlayerStatus.IsStunned())
		{
			vector = Vector3.zero;
		}
		else if (myPlayerStatus.IsRooted())
		{
			vector = Vector3.zero;
		}
		else if (myPlayerStatus.IsSlowed())
		{
			vector *= myPlayerStatus.GetEffectiveSlowMultiplier();
		}
		return vector;
	}

	private void Move()
	{
		current = rb.velocity;
		target = new Vector3(move.x, 0, move.y); // Already normalized
		target = orientation.TransformDirection(target);

		
		// if (move.y > 0)
		// {
		// 	if (move.x > 0)
		// 	{
		// 		Debug.Log("Forward Right");
		// 	}
		// 	else if (move.x < 0)
		// 	{
		// 		Debug.Log("Forward Left");
		// 	}
		// 	else
		// 	{
		// 		Debug.Log("Forward");
		// 	}
		// }
		// else if (move.y < 0)
		// {
		// 	if (move.x > 0)
		// 	{
		// 		Debug.Log("Back Right");
		// 	}
		// 	else if (move.x < 0)
		// 	{
		// 		Debug.Log("Back Left");
		// 	}
		// 	else
		// 	{
		// 		Debug.Log("Back");
		// 	}
		// }
		// else
		// {
		// 	if (move.x > 0)
		// 	{
		// 		Debug.Log("Right");
		// 	}
		// 	else if (move.x < 0)
		// 	{
		// 		Debug.Log("Left");
		// 	}
		// 	else
		// 	{
		// 		Debug.Log("Stationary");
		// 	}
		// }


		if (IsOnGround()) 
		{
			// target = AlignWithPlaneSetSpeed(target, groundCheckHit.normal, maxGroundSpeed);
			target = AlignWithPlaneSetSpeed(target, groundCheckHit.normal, 1f);
			if (move.y > 0)
			{
				if (move.x > 0)
				{
					// Debug.Log("Forward Right");
					target *= forwardDiagonalSpeed;
				}
				else if (move.x < 0)
				{
					// Debug.Log("Forward Left");
					target *= forwardDiagonalSpeed;
				}
				else
				{
					// Debug.Log("Forward");
					target *= forwardSpeed;
				}
			}
			else if (move.y < 0)
			{
				if (move.x > 0)
				{
					// Debug.Log("Back Right");
					target *= backDiagonalSpeed;
				}
				else if (move.x < 0)
				{
					// Debug.Log("Back Left");
					target *= backDiagonalSpeed;
				}
				else
				{
					// Debug.Log("Back");
					target *= backSpeed;
				}
			}
			else
			{
				if (move.x > 0)
				{
					// Debug.Log("Right");
					target *= sideSpeed;
				}
				else if (move.x < 0)
				{
					// Debug.Log("Left");
					target *= sideSpeed;
				}
				else
				{
					// Debug.Log("Stationary");
				}
			}
		}
		else
		{
			current = new Vector3(current.x, 0, current.z);
			target = new Vector3(target.x, 0, target.z);
			target *= maxAirSpeed;
		}

		target = ProcessStatusEffects(target);
		correcting = target - current;
		cortarDot = Vector3.Dot(correcting, target); // >0 if player has input but not yet up to target velocity, ==0 if player has no input

		if (IsOnGround() && IsSlopeTraversable())
		{
			if (cortarDot > 0)
			{
				correcting *= groundAccel;
			}
			else if (cortarDot == 0)
			{
				correcting *= groundDrag;
			}

			// Bug: Jumping in place sends player in a direction along the plane
			correcting = AlignWithPlaneSetSpeed(correcting, groundCheckHit.normal, correcting.magnitude);
		}
		else
		{
			if (cortarDot > 0)
			{
				correcting *= airAccel;
			}
			else if (cortarDot == 0)
			{
				correcting *= airDrag;
			}

			correcting = new Vector3(correcting.x, 0, correcting.z);
		}

		// Apply Force
		rb.AddForce(Time.fixedDeltaTime * correcting, ForceMode.VelocityChange);
	}

	private void TrySnapToPlane()
	{
		// Align current velocity along slope
		if (IsOnGround() && IsSlopeTraversable() && !RecentlyJumped()  && rb.velocity.magnitude <= slopeEscapeSpeed)
		{
			rb.velocity = AlignWithPlaneSetSpeed(rb.velocity, groundCheckHit.normal, rb.velocity.magnitude);
			
			
			// WIP
			// if (landedThisFrame && Vector3.Dot(slopeCheckHit.normal.normalized, Vector3.up) >= 1)
			// {
			// 	Debug.Log("Landed");
			// 	rb.velocity = AlignWithPlaneSetSpeed(new Vector3(rb.velocity.x, 0, rb.velocity.z), groundCheckHit.normal, rb.velocity.magnitude);
			// }
			// else
			// {
			// 	rb.velocity = AlignWithPlaneSetSpeed(rb.velocity, groundCheckHit.normal, rb.velocity.magnitude);
			// }
			
		}
		// if (IsOnGround() && IsSlopeTraversable() && !RecentlyJumped())
		// {
		// 	rb.MovePosition(Vector3.down * groundCheckHit.distance);
		// }
	}

	private void ApplyGravity()
	{
		if (IsOnGround() && IsSlopeTraversable())
		{
			return;
		}

		if (rb.velocity.y >= 0)
		{
			rb.AddForce(gravityAscending * Vector3.down, ForceMode.Acceleration);
		}
		else
		{
			rb.AddForce(gravityDescending * Vector3.down, ForceMode.Acceleration);
		}
	}

	private void UpdateAnimator()
	{
		animator.SetFloat("Speed", rb.velocity.magnitude);
	}

	private Vector3 AlignWithPlaneSetSpeed(Vector3 vector, Vector3 planeNormal, float speed)
	{
		// Bug: Magnitude of returned vector is sometimes less than the input
		return speed * Vector3.ProjectOnPlane(vector, planeNormal).normalized;
	}

	private void TryJump()
	{
		if (myPlayerStatus.IsStunned() || myPlayerStatus.IsRooted()) return;
		
		// Bug: Holding jump applies the force multiple times and causes you to jump slightly higher
		if ((IsOnGround() || IsInCoyoteTime()) && IsSlopeTraversable() && !RecentlyJumped())
		{
			Debug.Log("Jump");
			rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
			rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
			lastJumpedAtTime = Time.time;
		} 
	}

	private void Ability1()
	{
		// Vector3 abilityTarget;
		// if (move == Vector2.zero)
		// {
		// 	abilityTarget = Vector3.up * 0.5f;
		// }
		// else
		// {
		// 	abilityTarget = new Vector3(move.x, 0, move.y);
		// }
		//
		// abilityTarget = orientation.TransformDirection(abilityTarget);
		// rb.AddForce(abilityForce * abilityTarget, ForceMode.Impulse);
		
		
		
		
		
		
		
		
		
		// myPlayerStatus.ApplySlow(Random.Range(5, 10), Random.Range(0f, 1f));
		
		
		
		
		
		
		cameraEffects.CameraShake();
	}
}
