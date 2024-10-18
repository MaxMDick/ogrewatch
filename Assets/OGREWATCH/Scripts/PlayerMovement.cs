using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
	float playerHeight = 2f;

	[Header("Movement")]
	public float moveSpeed = 6f;
	public float movementMultiplier = 10f;
	public float airMultiplier = 0.4f;

	[Header("Jumping")]
	public float jumpForce = 15f;

	[Header("Drag")]
	public float groundDrag = 6f;
	public float airDrag = 2f;

	[SerializeField]
	Transform orientation;

	float horizontalMovement;
	float verticalMovement;

	bool isGrounded;

	Vector3 moveDirection;

	Rigidbody rb;

	void Start()
	{
		rb = GetComponent<Rigidbody>();
		rb.freezeRotation = true;
	}

	void Update()
	{
		isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight / 2 + 0.1f);

		MyInput();
		ControlDrag();

		if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
		{
			Jump();
		}
	}

	void FixedUpdate()
	{
		MovePlayer();
	}

	private void MyInput()
	{
		horizontalMovement = Input.GetAxisRaw("Horizontal");
		verticalMovement = Input.GetAxisRaw("Vertical");

		moveDirection = orientation.forward * verticalMovement + orientation.right * horizontalMovement;
	}

	private void MovePlayer()
	{
		if (isGrounded)
		{
			rb.AddForce(moveDirection.normalized * moveSpeed * movementMultiplier, ForceMode.Acceleration);
		}
		else if (!isGrounded)
		{
			rb.AddForce(moveDirection.normalized * moveSpeed * movementMultiplier * airMultiplier, ForceMode.Acceleration);
		}
	}

	private void ControlDrag()
	{
		if (isGrounded)
		{
			rb.drag = groundDrag;
		}
		else
		{
			rb.drag = airDrag;
		}
	}

	private void Jump()
	{
		rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
	}
}
