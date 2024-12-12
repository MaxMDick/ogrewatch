using System.Collections;
using System.Collections.Generic;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.InputSystem;

public class Combat : MonoBehaviour
{
	public enum CombatPhase
	{
		Idle,
		AttackWindup,
		AttackRelease,
		AttackRecovery,
		ParryWindup,
		ParryRelease,
		ParryRecovery,
		RiposteWindow
	}

	public enum QueueableAction
	{
		None,
		Attack,
		Parry
	}

	public Weapon weapon;

	[Header("Script References")]
	public Animator animator;
	public MyPlayerLookAirTest myPlayerLookAirTest;

	[Header("General")]
	public float idleRetainComboDuration = 0f;
	public string animationSpeedParameter = "AttackAnimationSpeed";
	public GameObject hitEffect;
	public GameObject parryEffect;
	public Collider parryCollider;
	// public float parryWindupDuration;
	// public float parryReleaseDuration;
	// public float parryRecoveryDuration;
	// public float riposteWindupModifier;
	// public float riposteReleaseModifier;

	[Header("General Status")]
	public float idleRetainComboTimer = 0f;
	public bool attackButtonHeld = false;
	public bool parryButtonHeld = false;
	public QueueableAction queuedAction = QueueableAction.None;
	public CombatPhase combatPhase = CombatPhase.Idle;
	public float combatPhaseDuration;
	public float combatPhaseTimer = 0f;
	public int comboIndex = 0;
	public Attack attackInformation;

	[Header("Tracers")]
	public Transform tracerStart;
	public Transform tracerEnd;
	public float tracerFrequency; // Keep in mind FixedUpdate is 0.02s
	public int tracerAmount; // Amount between start and end
	public bool debugTracers = true;
	public float debugTracerDuration = 0.1f;

	[Header("Tracer Status")]
	public float scaleBetween = 0f;
	public float tracerTimer;
	public Vector3 differenceVector;
	public List<Vector3> currentTracerPositions = new List<Vector3>();
	public List<Vector3> lastTracerPositions = new List<Vector3>();
	
	[Header("Interactions Status")]
	public bool successfullyParried = false;
	public List<GameObject> objectsInteractedWith = new List<GameObject>();
	private RaycastHit hitInfo;
	public Damageable hitDamageable;

	[Header("Stutter")]
	public bool stutterMeOnHit;
	public float stutterMeOnHitDuration;
	public bool stutterEnemyOnParry;
	public float stutterEnemyOnParryDuration;

	[Header("Stutter Status")]
	public float stutterDuration = -1f;
	public float stutterTimer = 0f;

	public void OnAttack(InputAction.CallbackContext context)
	{
		if (!attackButtonHeld)
		{
			QueueAttack();
		}
		attackButtonHeld = context.ReadValueAsButton();
	}

	public void OnAttack(bool value)
	{
		if (value)
		{
			QueueAttack();
		}
	}

	public void OnParry(InputAction.CallbackContext context)
	{
		if (!parryButtonHeld)
		{
			QueueParry();
		}
		parryButtonHeld = context.ReadValueAsButton();
	}

	public bool IsParrying()
	{
		if (combatPhase == CombatPhase.ParryWindup
		    || combatPhase == CombatPhase.ParryRelease)
		{
			return true;
		}
		return false;
	}

	public void SuccessfulParry(GameObject enemy)
	{
		if (combatPhase == CombatPhase.ParryWindup
		    || combatPhase == CombatPhase.ParryRelease)
		{
			successfullyParried = true;

			if (stutterEnemyOnParry)
			{
				enemy.GetComponent<Combat>().MakeStutter(stutterEnemyOnParryDuration);
			}
		}
	}

	public void MakeStutter(float duration)
	{
		if (stutterTimer > 0f)
		{
			Debug.Log("Stutter Continue");
			stutterDuration += duration;
			combatPhaseTimer -= duration;
		}
		else
		{
			Debug.Log("Stutter Start");
			stutterTimer = 0f;
			stutterDuration = duration;
			animator.SetFloat(animationSpeedParameter, 0f);
		}
	}

	void Start()
	{
		// UpdatePositionsList(currentTracerPositions);

		if (parryCollider)
		{
			parryCollider.enabled = false;
		}
	}

	void Update()
	{
		if (stutterDuration > 0)
		{
			ProcessStutter();
		}
		else
		{
			ProcessCombatPhase();
		
			if (combatPhase == CombatPhase.AttackRelease)
			{
				UpdateTracers();
			}
		}
	}
	
	private void QueueAttack()
	{
		queuedAction = QueueableAction.Attack;
	}

	private void QueueParry()
	{
		queuedAction = QueueableAction.Parry;
	}

	private void SetAttackInformation(int index, bool riposte = false)
	{
		attackInformation = new Attack(weapon.attackList[index]);
		if (riposte)
		{
			attackInformation.windupDuration *= weapon.riposteWindupModifier;
			attackInformation.releaseDuration *= weapon.riposteReleaseModifier;
			attackInformation.damage *= weapon.riposteDamageModifier;
		}
	}

	private void ProcessStutter()
	{
		if (stutterTimer < stutterDuration)
		{
			stutterTimer += Time.deltaTime;
		}
		else
		{
			Debug.Log("Stutter End");
			stutterDuration = -1f;
			stutterTimer = 0f;
			animator.SetFloat(animationSpeedParameter, 1f / combatPhaseDuration);
		}
	}
	
	private void UpdatePositionsList(List<Vector3> positions)
	{
		positions.Clear();
		
		scaleBetween = 1f / (tracerAmount + 1);
		float scaledCount = scaleBetween;
		differenceVector = tracerEnd.position - tracerStart.position;
		
		positions.Add(tracerStart.position);
		for (int i = 0; i < tracerAmount; i++)
		{
			positions.Add(tracerStart.position + differenceVector * scaledCount);
			scaledCount += scaleBetween;
		}
		positions.Add(tracerEnd.position);
	}

	private void UpdateTracers()
	{
		tracerTimer += Time.deltaTime;
		if (tracerTimer >= tracerFrequency)
		{
			tracerTimer = 0f;
			
			lastTracerPositions = new List<Vector3>(currentTracerPositions);
			UpdatePositionsList(currentTracerPositions);
			
			for (int i = 0; i < currentTracerPositions.Count; i++)
			{
				if (debugTracers)
				{
					Debug.DrawLine(lastTracerPositions[i], currentTracerPositions[i], Color.red, debugTracerDuration);
				}

				if (Physics.Linecast(lastTracerPositions[i], currentTracerPositions[i], out hitInfo))
				{
					hitDamageable = hitInfo.collider.GetComponent<Damageable>();
					if (hitDamageable)
					{
						GameObject hitObject = hitDamageable.GetOwner();
						if (!objectsInteractedWith.Contains(hitObject) && hitObject != this.gameObject)
						{
							Debug.Log(hitObject.name + " - " + hitDamageable.GetPartName());
							objectsInteractedWith.Add(hitObject);

							// if (hitDamageable.IsParryCollider())
							// {
							// 	Instantiate(parryEffect, hitInfo.point, Quaternion.identity);
							// }
							Combat hitObjectCombat = hitObject.GetComponent<Combat>();
							if (hitObjectCombat && hitObjectCombat.IsParrying())
							{
								Instantiate(parryEffect, hitInfo.point, Quaternion.identity);
								hitObjectCombat.SuccessfulParry(this.gameObject);
							}
							else if (!hitDamageable.IsParryCollider())
							{
								Instantiate(hitEffect, hitInfo.point, Quaternion.identity);
								hitObject.GetComponent<Health>().TakeDamage(attackInformation.damage);
								
								if (stutterMeOnHit)
								{
									this.MakeStutter(stutterMeOnHitDuration);
								}
							}
						}
					}
				}
			}
		}
	}
	
	private void ProcessCombatPhase()
	{
		if (combatPhase == CombatPhase.Idle && comboIndex != 0 && combatPhaseTimer >= idleRetainComboDuration)
		{
			Debug.Log("Reset Combo");
			comboIndex = 0;
		}
		
		// PROCESS SUCCESSFUL PARRY
		if (successfullyParried)
		{
			if (queuedAction == QueueableAction.None)
			{
				Debug.Log("SuccessfulParry -I-> RiposteWindow");
				combatPhase = CombatPhase.RiposteWindow;
				combatPhaseTimer = 0f;
				successfullyParried = false;
				// comboIndex = 0;

				combatPhaseDuration = weapon.riposteWindowDuration;
				animator.SetFloat(animationSpeedParameter, 0f);
				animator.CrossFadeInFixedTime("Combat Layer." + weapon.idleAnimation.name, combatPhaseDuration);
			}
			else if (queuedAction == QueueableAction.Attack)
			{
				Debug.Log("SuccessfulParry -IQ-> AttackWindup");
				combatPhase = CombatPhase.AttackWindup;
				combatPhaseTimer = 0f;
				queuedAction = QueueableAction.None;
				successfullyParried = false;
				
				SetAttackInformation(comboIndex, true);
				// comboIndex = (comboIndex + 1) % weapon.attackList.Count;
				
				animator.SetFloat(animationSpeedParameter, 0f);
				combatPhaseDuration = attackInformation.windupDuration;
				animator.CrossFadeInFixedTime("Combat Layer." + attackInformation.animationClip.name, combatPhaseDuration);
			}
		}
		
		// PROCESS STATE INTERRUPTS
		if (queuedAction != QueueableAction.None)
		{
			if (combatPhase == CombatPhase.Idle)
			{
				if (queuedAction == QueueableAction.Attack)
				{
					Debug.Log("Idle -I-> AttackWindup");
					combatPhase = CombatPhase.AttackWindup;
					combatPhaseTimer = 0f;
					queuedAction = QueueableAction.None;
					
					SetAttackInformation(comboIndex);
					
					animator.SetFloat(animationSpeedParameter, 0f);
					combatPhaseDuration = attackInformation.windupDuration;
					animator.CrossFadeInFixedTime("Combat Layer." + attackInformation.animationClip.name, combatPhaseDuration);
				}
				else if (queuedAction == QueueableAction.Parry)
				{
					Debug.Log("Idle -I-> ParryWindup");
					combatPhase = CombatPhase.ParryWindup;
					combatPhaseTimer = 0f;
					queuedAction = QueueableAction.None;
					
					animator.SetFloat(animationSpeedParameter, 0f);
					combatPhaseDuration = weapon.parryWindupDuration;
					if (myPlayerLookAirTest.LastLookDirectionX() > 0)
					{
						animator.CrossFadeInFixedTime("Combat Layer.Parry_Right", combatPhaseDuration);
					}
					else
					{
						animator.CrossFadeInFixedTime("Combat Layer.Parry_Left", combatPhaseDuration);
					}
				}
			}
			else if (combatPhase == CombatPhase.AttackWindup)
			{
				if (queuedAction == QueueableAction.Parry)
				{
					Debug.Log("AttackWindup -I-> ParryWindup");
					combatPhase = CombatPhase.ParryWindup;
					combatPhaseTimer = 0f;
					queuedAction = QueueableAction.None;
					
					animator.SetFloat(animationSpeedParameter, 0f);
					combatPhaseDuration = weapon.parryWindupDuration;
					if (myPlayerLookAirTest.LastLookDirectionX() > 0)
					{
						animator.CrossFadeInFixedTime("Combat Layer.Parry_Right", combatPhaseDuration);
					}
					else
					{
						animator.CrossFadeInFixedTime("Combat Layer.Parry_Left", combatPhaseDuration);
					}
				}
			}
			else if (combatPhase == CombatPhase.AttackRecovery)
			{
				if (queuedAction == QueueableAction.Attack)
				{
					Debug.Log("AttackRecovery -I-> AttackWindup");
					combatPhase = CombatPhase.AttackWindup;
					combatPhaseTimer = 0f;
					queuedAction = QueueableAction.None;
					
					// comboIndex = (comboIndex + 1) % weapon.attackList.Count;
					SetAttackInformation(comboIndex);
					
					animator.SetFloat(animationSpeedParameter, 0f);
					combatPhaseDuration = attackInformation.windupDuration;
					animator.CrossFadeInFixedTime("Combat Layer." + attackInformation.animationClip.name, combatPhaseDuration);
				}
				else if (queuedAction == QueueableAction.Parry)
				{
					Debug.Log("AttackRecovery -I-> ParryWindup");
					combatPhase = CombatPhase.ParryWindup;
					combatPhaseTimer = 0f;
					queuedAction = QueueableAction.None;
					
					animator.SetFloat(animationSpeedParameter, 0f);
					combatPhaseDuration = weapon.parryWindupDuration;
					if (myPlayerLookAirTest.LastLookDirectionX() > 0)
					{
						animator.CrossFadeInFixedTime("Combat Layer.Parry_Right", combatPhaseDuration);
					}
					else
					{
						animator.CrossFadeInFixedTime("Combat Layer.Parry_Left", combatPhaseDuration);
					}
				}
			}
			else if (combatPhase == CombatPhase.RiposteWindow)
			{
				if (queuedAction == QueueableAction.Attack)
				{
					Debug.Log("RiposteWindow -I-> AttackWindup");
					combatPhase = CombatPhase.AttackWindup;
					combatPhaseTimer = 0f;
					queuedAction = QueueableAction.None;
					
					SetAttackInformation(comboIndex, true);
					
					animator.SetFloat(animationSpeedParameter, 0f);
					combatPhaseDuration = attackInformation.windupDuration;
					animator.CrossFadeInFixedTime("Combat Layer." + attackInformation.animationClip.name, combatPhaseDuration);
				}
				else if (queuedAction == QueueableAction.Parry)
				{
					Debug.Log("RiposteWindow -I-> ParryWindup");
					combatPhase = CombatPhase.ParryWindup;
					combatPhaseTimer = 0f;
					queuedAction = QueueableAction.None;
					
					animator.SetFloat(animationSpeedParameter, 0f);
					combatPhaseDuration = weapon.parryWindupDuration;
					if (myPlayerLookAirTest.LastLookDirectionX() > 0)
					{
						animator.CrossFadeInFixedTime("Combat Layer.Parry_Right", combatPhaseDuration);
					}
					else
					{
						animator.CrossFadeInFixedTime("Combat Layer.Parry_Left", combatPhaseDuration);
					}
				}
			}
		}
		
		
		// ADVANCE STATE MACHINE
		combatPhaseTimer += Time.deltaTime;
		
		// UPDATE STATE MACHINE
		//	// ATTACK STATES
		if (combatPhase == CombatPhase.AttackWindup && combatPhaseTimer >= attackInformation.windupDuration)
		{
			Debug.Log("AttackWindup --> AttackRelease");
			combatPhase = CombatPhase.AttackRelease;
			combatPhaseTimer = 0f;
			queuedAction = QueueableAction.None;
			
			combatPhaseDuration = attackInformation.releaseDuration;
			animator.SetFloat(animationSpeedParameter, 1f / combatPhaseDuration);
			
			UpdatePositionsList(currentTracerPositions);
			objectsInteractedWith.Clear();
		}
		else if (combatPhase == CombatPhase.AttackRelease && combatPhaseTimer >= attackInformation.releaseDuration)
		{
			comboIndex = (comboIndex + 1) % weapon.attackList.Count;
			
			if (queuedAction == QueueableAction.None)
			{
				Debug.Log("AttackRelease --> AttackRecovery");
				combatPhase = CombatPhase.AttackRecovery;
				combatPhaseTimer = 0f;
				
				animator.SetFloat(animationSpeedParameter, 0f);
				combatPhaseDuration = attackInformation.recoveryDuration;
				animator.CrossFadeInFixedTime("Combat Layer." + weapon.idleAnimation.name, combatPhaseDuration);
			}
			else if (queuedAction == QueueableAction.Attack)
			{
				Debug.Log("AttackRelease -Q-> AttackWindup");
				combatPhase = CombatPhase.AttackWindup;
				combatPhaseTimer = 0f;
				queuedAction = QueueableAction.None;

				// comboIndex = (comboIndex + 1) % weapon.attackList.Count;
				SetAttackInformation(comboIndex);
				
				animator.SetFloat(animationSpeedParameter, 0f);
				combatPhaseDuration = attackInformation.windupDuration;
				animator.CrossFadeInFixedTime("Combat Layer." + attackInformation.animationClip.name, combatPhaseDuration);
			}
			else if (queuedAction == QueueableAction.Parry)
			{
				Debug.Log("AttackRelease -Q-> ParryWindup");
				combatPhase = CombatPhase.ParryWindup;
				combatPhaseTimer = 0f;
				queuedAction = QueueableAction.None;
				
				animator.SetFloat(animationSpeedParameter, 0f);
				combatPhaseDuration = weapon.parryWindupDuration;
				if (myPlayerLookAirTest.LastLookDirectionX() > 0)
				{
					animator.CrossFadeInFixedTime("Combat Layer.Parry_Right", combatPhaseDuration);
				}
				else
				{
					animator.CrossFadeInFixedTime("Combat Layer.Parry_Left", combatPhaseDuration);
				}
			}
		}
		else if (combatPhase == CombatPhase.AttackRecovery && combatPhaseTimer >= attackInformation.recoveryDuration)
		{
			Debug.Log("AttackRecovery --> Idle");
			combatPhase = CombatPhase.Idle;
			combatPhaseTimer = 0f;
			queuedAction = QueueableAction.None;

			// comboIndex = 0;
			SetAttackInformation(comboIndex);
			
			combatPhaseDuration = Mathf.NegativeInfinity;
			animator.SetFloat(animationSpeedParameter, 1f);
		}
		//	// PARRY STATES
		else if (combatPhase == CombatPhase.ParryWindup && combatPhaseTimer >= weapon.parryWindupDuration)
		{
			Debug.Log("ParryWindup --> ParryRelease");
			combatPhase = CombatPhase.ParryRelease;
			combatPhaseTimer = 0f;
			if (queuedAction == QueueableAction.Parry)
			{
				queuedAction = QueueableAction.None;
			}

			combatPhaseDuration = weapon.parryReleaseDuration;
			animator.SetFloat(animationSpeedParameter, 1f / combatPhaseDuration);
		}
		else if (combatPhase == CombatPhase.ParryRelease && combatPhaseTimer >= weapon.parryReleaseDuration)
		{
			Debug.Log("ParryRelease --> ParryRecovery");
			combatPhase = CombatPhase.ParryRecovery;
			combatPhaseTimer = 0f;
			if (queuedAction == QueueableAction.Parry)
			{
				queuedAction = QueueableAction.None;
			}
			
			animator.SetFloat(animationSpeedParameter, 0f);
			combatPhaseDuration = weapon.parryRecoveryDuration;
			animator.CrossFadeInFixedTime("Combat Layer." + weapon.idleAnimation.name, combatPhaseDuration);
		}
		else if (combatPhase == CombatPhase.ParryRecovery && combatPhaseTimer >= weapon.parryRecoveryDuration)
		{
			if (queuedAction == QueueableAction.Attack)
			{
				Debug.Log("ParryRecovery -Q-> AttackWindup");
				combatPhase = CombatPhase.AttackWindup;
				combatPhaseTimer = 0f;
				queuedAction = QueueableAction.None;

				// comboIndex = (comboIndex + 1) % weapon.attackList.Count;
				SetAttackInformation(comboIndex);
				
				animator.SetFloat(animationSpeedParameter, 0f);
				combatPhaseDuration = attackInformation.windupDuration;
				animator.CrossFadeInFixedTime("Combat Layer." + attackInformation.animationClip.name, combatPhaseDuration);
			}
			else if (queuedAction == QueueableAction.Parry)
			{
				Debug.Log("ParryRecovery -Q-> ParryWindup");
				combatPhase = CombatPhase.ParryWindup;
				combatPhaseTimer = 0f;
				queuedAction = QueueableAction.None;
				
				animator.SetFloat(animationSpeedParameter, 0f);
				combatPhaseDuration = weapon.parryWindupDuration;
				if (myPlayerLookAirTest.LastLookDirectionX() > 0)
				{
					animator.CrossFadeInFixedTime("Combat Layer.Parry_Right", combatPhaseDuration);
				}
				else
				{
					animator.CrossFadeInFixedTime("Combat Layer.Parry_Left", combatPhaseDuration);
				}
			}
			else
			{
				Debug.Log("ParryRecovery --> Idle");
				combatPhase = CombatPhase.Idle;
				combatPhaseTimer = 0f;
				queuedAction = QueueableAction.None;
				
				// comboIndex = 0;

				combatPhaseDuration = Mathf.NegativeInfinity;
				animator.SetFloat(animationSpeedParameter, 1f);
			}
		}
		else if (combatPhase == CombatPhase.RiposteWindow && combatPhaseTimer >= weapon.riposteWindowDuration)
		{
			Debug.Log("RiposteWindow --> Idle");
			combatPhase = CombatPhase.Idle;
			combatPhaseTimer = 0f;
			queuedAction = QueueableAction.None;

			combatPhaseDuration = Mathf.NegativeInfinity;
			animator.SetFloat(animationSpeedParameter, 1f);
		}
	}
}

[System.Serializable]
public class Attack
{
	public AnimationClip animationClip;
	public float windupDuration;
	public float releaseDuration;
	public float recoveryDuration;
	public float damage;

	public Attack(Attack other)
	{
		animationClip = other.animationClip;
		windupDuration = other.windupDuration;
		releaseDuration = other.releaseDuration;
		recoveryDuration = other.recoveryDuration;
		damage = other.damage;
	}
}

[System.Serializable]
public class Weapon
{
	[Header("Parry")]
	public float parryWindupDuration;
	public float parryReleaseDuration;
	public float parryRecoveryDuration;

	[Header("Riposte")]
	public float riposteWindowDuration;
	public float riposteWindupModifier;
	public float riposteReleaseModifier;
	public float riposteDamageModifier;
	
	[Header("Attack List")]
	public List<Attack> attackList = new List<Attack>();
	
	[Header("Other Animations")]
	public AnimationClip idleAnimation;
	public AnimationClip leftParryAnimation;
	public AnimationClip rightParryAnimation;
}