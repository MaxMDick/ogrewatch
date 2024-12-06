using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
	public Animator animator;
	public bool mouseHeld = false;

	public string attackSpeedParam = "AttackAnimationSpeed";

	public List<Attack> attacks = new List<Attack>();
	
	public float timer = 0f;
	public AttackPhase currentAttackPhase = AttackPhase.Idle;

	public bool doAttack = false;

	public int comboIndex = 0;
	public Attack currentAttack;

	[Header("Tracers")]
	public bool drawTracers = true;
	public Transform tracerStart;
	public Transform tracerEnd;
	public float tracerFrequency; // Keep in mind fixedUpdate is 0.02
	private float tracerTimer = 0f;
	public int tracerCount; // Between start and end
	public List<Vector3> currentTracerPositions = new List<Vector3>();
	public List<Vector3> lastTracerPositions = new List<Vector3>();
	public float scaleBetween = 0f;
	private Vector3 diffVec;
	public float tracerDuration;

	private RaycastHit hitInfo;
	public Damageable hitPart;
	public GameObject hitObject;
	public GameObject hitEffect;

	// public bool hitStutter;
	public float hitStutterDuration;
	public bool hitStuttering;
	public float hitStutterTimer;
	
	public void OnAttack(InputAction.CallbackContext context)
	{
		if (!mouseHeld)
		{
			TryAttack();
		}
		mouseHeld = context.ReadValueAsButton();
	}
	
	void Start()
	{
		UpdatePositionsList(currentTracerPositions);
	}

	void Update()
	{
		UpdateAttackPhase();
		
		if (drawTracers && currentAttackPhase == AttackPhase.Release)
		{
			tracerTimer += Time.deltaTime;
			if (tracerTimer >= tracerFrequency)
			{
				tracerTimer = 0f;
				// lastPosition = currentPosition;
				// currentPosition = tracerEnd.position;
				// Debug.DrawLine(lastPosition, currentPosition, Color.red, 1f);
				lastTracerPositions = new List<Vector3>(currentTracerPositions);
				UpdatePositionsList(currentTracerPositions);
				for (int i = 0; i < currentTracerPositions.Count; i++)
				{
					// Color col = Color.Lerp(Color.white, Color.red, (float)i) / currentTracerPositions.Count;
					Debug.DrawLine(lastTracerPositions[i], currentTracerPositions[i], Color.red, tracerDuration);
					
					// Physics.Linecast(lastTracerPositions[i], currentTracerPositions[i], out hitInfo, 0, QueryTriggerInteraction.Collide);
					if (Physics.Linecast(lastTracerPositions[i], currentTracerPositions[i], out hitInfo))
					{
						hitPart = hitInfo.transform.GetComponent<Damageable>();
						if (hitPart)
						{
							if (!hitObject)
							{
								hitObject = hitPart.GetOwner();
								Debug.Log(hitObject.name + " - " + hitPart.GetPartName());
								Instantiate(hitEffect, hitInfo.point, Quaternion.identity);

								HitStutter();
							}
						}
					}
				}
			}
		}
	}

	private void HitStutter()
	{
		hitStuttering = true;
		hitStutterTimer = 0f;
		timer -= hitStutterDuration;
		animator.SetFloat(attackSpeedParam, 0f);
	}
	
	private void TryAttack()
	{
		if (currentAttackPhase == AttackPhase.Idle
		    || currentAttackPhase == AttackPhase.Release
		    || currentAttackPhase == AttackPhase.Recovery)
		{
			doAttack = true;
		}
	}

	private void SetAttackStats(int index)
	{
		currentAttack = attacks[index];
	}

	private void UpdatePositionsList(List<Vector3> positions)
	{
		positions.Clear();
		
		scaleBetween = 1f / (tracerCount + 1);
		float curScale = scaleBetween;
		diffVec = tracerEnd.position - tracerStart.position;
		
		positions.Add(tracerStart.position);
		for (int i = 0; i < tracerCount; i++)
		{
			positions.Add(tracerStart.position + diffVec * curScale);
			curScale += scaleBetween;
		}
		positions.Add(tracerEnd.position);
	}

	private void UpdateAttackPhase()
	{
		if (currentAttackPhase != AttackPhase.Idle)
		{
			timer += Time.deltaTime;
		}

		if (currentAttackPhase == AttackPhase.Release)
		{
			if (hitStuttering)
			{
				hitStutterTimer += Time.deltaTime;
				if (hitStutterTimer >= hitStutterDuration)
				{
					hitStuttering = false;
					animator.SetFloat(attackSpeedParam, 1f / currentAttack.releaseDuration);
				}
			}
		}

		// if (timer >= releaseDuration * 0.5f && currentAttackPhase == AttackPhase.Release)
		// {
		// 	animator.SetFloat("AttackSpeed", -1f);
		// }
		

		if (doAttack && (currentAttackPhase == AttackPhase.Idle || currentAttackPhase == AttackPhase.Recovery))
		{
			if (currentAttackPhase == AttackPhase.Recovery)
			{
				comboIndex = (comboIndex + 1) % attacks.Count;
			}
			SetAttackStats(comboIndex);
			
			// Debug.Log("Windup");
			currentAttackPhase = AttackPhase.Windup;
			animator.SetFloat(attackSpeedParam, 0f);
			animator.CrossFadeInFixedTime("Combat Layer." + currentAttack.animationClip.name, currentAttack.windupDuration);
			timer = 0f;
			doAttack = false;
		}
		else if (currentAttackPhase == AttackPhase.Windup && timer >= currentAttack.windupDuration)
		{
			// Debug.Log("Release");
			currentAttackPhase = AttackPhase.Release;
			animator.SetFloat(attackSpeedParam, 1f / currentAttack.releaseDuration);
			timer = 0f;
			
			UpdatePositionsList(currentTracerPositions);
			hitObject = null;
		}
		else if (currentAttackPhase == AttackPhase.Release && timer >= currentAttack.releaseDuration)
		{
			currentAttackPhase = AttackPhase.Recovery;
			if (!doAttack)
			{
				// Debug.Log("Recovery");
				animator.SetFloat(attackSpeedParam, 0f);
				animator.CrossFadeInFixedTime("Combat Layer.Empty", currentAttack.recoveryDuration);
				timer = 0f;
			}
		}
		else if (currentAttackPhase == AttackPhase.Recovery && timer >= currentAttack.recoveryDuration)
		{
			// Debug.Log("Idle");
			currentAttackPhase = AttackPhase.Idle;
			timer = 0f;
			
			comboIndex = 0;
			SetAttackStats(comboIndex);
		}
	}
}

public enum AttackPhase
{
	Idle,
	Windup,
	Release,
	Recovery
}

[System.Serializable]
public class Attack
{
	public AnimationClip animationClip;
	public float windupDuration;
	public float releaseDuration;
	public float recoveryDuration;
}
