using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
	//[Header("Attacking")]
	//public float attackDistance = 3f;
	//public float attackDelay = 0.4f;
	//public float attackSpeed = 1f;
	//public int attackDamage = 1f;
	//public LayerMask attackLayer;

	//public GameObject hitEffect;
	//public AudioClip swordSwing;
	//public AudioClip hitSound;

	//bool attacking = false;
	//bool readyToAttack = true;
	//int attackCount;

	//public void OnFire(InputAction.CallbackContext context)
	//{
	//	Attack();
	//}

	//// Start is called before the first frame update
	//void Start()
 //   {
        
 //   }

 //   // Update is called once per frame
 //   void Update()
 //   {
	//	isGrounded = MyPlayerMovement.isGrounded;

	//	// Repeat Inputs
	//	if (input.Attack.IsPressed())
	//	{
	//		Attack();
	//	}

	//	SetAnimations();
 //   }

	//private void Attack()
	//{
	//	if (!readyToAttack || attacking) return;

	//	readyToAttack = false;
	//	attacking = true;

	//	Invoke(nameof(ResetAttack), attackSpeed);
	//	Invoke(nameof(AttackRaycast), attackDelay);

	//	audioSource.pitch = Random.Range(0.9f, 1.1f);
	//	audioSource.PlayOneShot(swordSwing);
	//}

	//private void ResetAttack()
	//{
	//	attacking = false;
	//	readyToAttack = true;
	//}

	//private void AttackRaycast()
	//{
	//	if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, attackDistance, attackLayer))
	//	{
	//		HitTarget(hit.point);
	//	}
	//}

	//private void HitTarget(Vector3 pos)
	//{
	//	audioSource.pitch = 1;
	//	audioSource.PlayOneShot(hitSound);

	//	GameObject GO = Instantiate(hitEffect, pos, Quaternion.identity);
	//	Destroy(GO, 20);
	//}
}
