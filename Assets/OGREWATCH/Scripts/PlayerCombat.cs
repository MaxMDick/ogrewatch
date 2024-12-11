using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
	public Animator animator;
	public bool mouseHeldLeft = false;

	public string attackSpeedParam = "AttackAnimationSpeed";

	public List<Attack> attacks = new List<Attack>();
	
	public float phaseTimer = 0f;
	public CombatPhase currentCombatPhase = CombatPhase.Idle;

	public bool doAttack = false;

	public int comboIndex = 0;
	public Attack currentAttack;

	[Header("Tracers")]
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
	// public GameObject hitObject;
	public GameObject hitEffect;

	// public bool hitStutter;
	public float hitStutterDuration;
	public bool hitStuttering;
	public float hitStutterTimer;
	
	[Header("Parry Test")]
	public MyPlayerLookAirTest myPlayerLookAirTest;
	public bool mouseHeldRight = false;
	public bool doParry = false;
	public List<GameObject> interactedObjects = new List<GameObject>();
	public float parryWindupDuration;
	public float parryReleaseDuration;
	public float parryRecoveryDuration;
	public AnimationClip idleAnimation;
	public GameObject parryEffect;
	public Collider parryCollider;
	
	public void OnAttack(InputAction.CallbackContext context)
	{
		if (!mouseHeldLeft)
		{
			TryAttack();
		}
		mouseHeldLeft = context.ReadValueAsButton();
	}

	public void OnAttack(bool value) // For triggering from another script instead of InputAction
	{
		if (value)
		{
			TryAttack();
		}
	}

	public void OnParry(InputAction.CallbackContext context)
	{
		if (!mouseHeldRight)
		{
			TryParry();
		}
		mouseHeldRight = context.ReadValueAsButton();
	}

	public bool IsParrying()
	{
		if (currentCombatPhase == CombatPhase.ParryWindup
		    || currentCombatPhase == CombatPhase.ParryRelease)
		{
			return true;
		}

		return false;
	}
	
	void Start()
	{
		UpdatePositionsList(currentTracerPositions);

		if (parryCollider)
		{
			parryCollider.enabled = false;
		}
	}

	void Update()
	{
		UpdateAttackPhase();
		UpdateTracers();
		
	}

	private void HitStutter()
	{
		hitStuttering = true;
		hitStutterTimer = 0f;
		phaseTimer -= hitStutterDuration;
		animator.SetFloat(attackSpeedParam, 0f);
	}
	
	private void TryAttack()
	{
		if (currentCombatPhase == CombatPhase.Idle
		    || currentCombatPhase == CombatPhase.Release
		    || currentCombatPhase == CombatPhase.Recovery)
		{
			doAttack = true;
		}
	}

	private void TryParry()
	{
		if (currentCombatPhase != CombatPhase.ParryWindup
		    && currentCombatPhase != CombatPhase.ParryRelease)
		{
			doParry = true;
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
		if (currentCombatPhase != CombatPhase.Idle)
		{
			phaseTimer += Time.deltaTime;
		}

		if (doParry && (currentCombatPhase == CombatPhase.Idle || currentCombatPhase == CombatPhase.Windup ||
		                currentCombatPhase == CombatPhase.Recovery))
		{
			currentCombatPhase = CombatPhase.ParryWindup;
			phaseTimer = 0f;
			doParry = false;

			animator.SetFloat(attackSpeedParam, 0f);
			if (myPlayerLookAirTest.LastLookDirectionX() > 0)
			{
				animator.CrossFadeInFixedTime("Combat Layer.Parry_Right", parryWindupDuration);
			}
			else
			{
				animator.CrossFadeInFixedTime("Combat Layer.Parry_Left", parryWindupDuration);
			}
			
			parryCollider.enabled = true;
			
		}
		else if (currentCombatPhase == CombatPhase.ParryWindup && phaseTimer >= parryWindupDuration)
		{
			currentCombatPhase = CombatPhase.ParryRelease;
			phaseTimer = 0f;
			
			animator.SetFloat(attackSpeedParam, 1f / parryReleaseDuration);
			
			// parryCollider.enabled = true;
		}
		else if (currentCombatPhase == CombatPhase.ParryRelease && phaseTimer >= parryReleaseDuration)
		{
			currentCombatPhase = CombatPhase.ParryRecovery;
			phaseTimer = 0f;
			
			animator.SetFloat(attackSpeedParam, 0f);
			animator.CrossFadeInFixedTime("Combat Layer." + idleAnimation.name, parryReleaseDuration);
			
			parryCollider.enabled = false;
		}
		else if (currentCombatPhase == CombatPhase.ParryRecovery && phaseTimer >= parryRecoveryDuration)
		{
			currentCombatPhase = CombatPhase.Idle;
			phaseTimer = 0f;
			animator.SetFloat(attackSpeedParam, 1f);
		}
		
		
		
		
		
		
		
		
		
		
		
		
		

		if (currentCombatPhase == CombatPhase.Release)
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
		
		
		
		
		
		
		
		
		
		

		if (doAttack && (currentCombatPhase == CombatPhase.Idle || currentCombatPhase == CombatPhase.Recovery))
		{
			if (currentCombatPhase == CombatPhase.Recovery)
			{
				comboIndex = (comboIndex + 1) % attacks.Count;
			}
			SetAttackStats(comboIndex);
			
			// Debug.Log("Windup");
			currentCombatPhase = CombatPhase.Windup;
			animator.SetFloat(attackSpeedParam, 0f);
			animator.CrossFadeInFixedTime("Combat Layer." + currentAttack.animationClip.name, currentAttack.windupDuration);
			phaseTimer = 0f;
			doAttack = false;
		}
		else if (currentCombatPhase == CombatPhase.Windup && phaseTimer >= currentAttack.windupDuration)
		{
			// Debug.Log("Release");
			currentCombatPhase = CombatPhase.Release;
			animator.SetFloat(attackSpeedParam, 1f / currentAttack.releaseDuration);
			phaseTimer = 0f;
			
			UpdatePositionsList(currentTracerPositions);
			interactedObjects.Clear();
		}
		else if (currentCombatPhase == CombatPhase.Release && phaseTimer >= currentAttack.releaseDuration)
		{
			currentCombatPhase = CombatPhase.Recovery;
			if (!doAttack)
			{
				// Debug.Log("Recovery");
				animator.SetFloat(attackSpeedParam, 0f);
				animator.CrossFadeInFixedTime("Combat Layer." + idleAnimation.name, currentAttack.recoveryDuration);
				phaseTimer = 0f;
			}
		}
		else if (currentCombatPhase == CombatPhase.Recovery && phaseTimer >= currentAttack.recoveryDuration)
		{
			// Debug.Log("Idle");
			currentCombatPhase = CombatPhase.Idle;
			phaseTimer = 0f;
			
			comboIndex = 0;
			SetAttackStats(comboIndex);
			
			animator.SetFloat(attackSpeedParam, 1f);
		}
	}

	private void UpdateTracers()
	{
		if (currentCombatPhase == CombatPhase.Release)
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
					
					// if (Physics.Linecast(lastTracerPositions[i], currentTracerPositions[i], out hitInfo, -1, QueryTriggerInteraction.Collide))
					if (Physics.Linecast(lastTracerPositions[i], currentTracerPositions[i], out hitInfo))
					{
						hitPart = hitInfo.collider.transform.GetComponent<Damageable>();
						// Debug.Break();
						// Debug.Log(i + " : " + hitInfo.transform.name);
						// Debug.Log(i + " : " + hitInfo.collider.transform.name);
						if (hitPart)
						{
							// Debug.Log("hit part - " + hitPart);
							GameObject hitObject = hitPart.GetOwner();
							if (!interactedObjects.Contains(hitObject) && hitObject != this.gameObject)
							{
								interactedObjects.Add(hitObject);
								
								
								Debug.Log(hitObject.name + " - " + hitPart.GetPartName());

								if (hitPart.IsParryCollider())
								{
									Instantiate(parryEffect, hitInfo.point, Quaternion.identity);
								}
								else if (hitObject.GetComponent<PlayerCombat>().IsParrying())
								{
									Instantiate(parryEffect, hitInfo.point, Quaternion.identity);
								}
								else
								{
									Instantiate(hitEffect, hitInfo.point, Quaternion.identity);
									HitStutter();
									hitObject.GetComponent<Health>().TakeDamage(20f);
								}
							}
						}
					}
				}
			}
		}
	}
}

public enum CombatPhase
{
	Idle,
	Windup,
	Release,
	Recovery,
	ParryWindup,
	ParryRelease,
	ParryRecovery
}

[System.Serializable]
public class Attack
{
	public AnimationClip animationClip;
	public float windupDuration;
	public float releaseDuration;
	public float recoveryDuration;
}
