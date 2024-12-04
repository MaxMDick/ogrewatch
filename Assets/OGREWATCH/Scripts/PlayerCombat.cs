using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
	public Animator animator;
	public bool mouseHeld = false;

	public List<Attack> attacks = new List<Attack>();
	
	public float timer = 0f;
	public AttackPhase currentAttackPhase = AttackPhase.Idle;

	public bool doAttack = false;

	public int comboIndex = 0;

	public string attackName;
	public float windupDuration;
	public float releaseDuration;
	public float recoveryDuration;
	
	public void OnAttack(InputAction.CallbackContext context)
	{
		if (!mouseHeld)
		{
			TryAttack();
		}
		mouseHeld = context.ReadValueAsButton();
	}
	
	private void TryAttack()
	{
		if (currentAttackPhase == AttackPhase.Idle
		    || currentAttackPhase == AttackPhase.Release
		    || currentAttackPhase == AttackPhase.Recovery)
		{
			doAttack = true;
		}
		
		if (currentAttackPhase == AttackPhase.Idle)
		{
			doAttack = true;
		}
		else if (currentAttackPhase == AttackPhase.Release)
		{
			doAttack = true;
		}
		else if (currentAttackPhase == AttackPhase.Recovery)
		{
			doAttack = true;
		}
	}

	private void SetAttackStats(int index)
	{
		attackName = attacks[index].animationClip.name;
		windupDuration = attacks[index].windupDuration;
		releaseDuration = attacks[index].releaseDuration;
		recoveryDuration = attacks[index].recoveryDuration;
	}

	void Update()
	{
		if (currentAttackPhase != AttackPhase.Idle)
		{
			timer += Time.deltaTime;
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
			animator.SetFloat("AttackSpeed", 0f);
			animator.CrossFadeInFixedTime("Combat Layer." + attackName, windupDuration);
			timer = 0f;
			doAttack = false;
		}
		else if (currentAttackPhase == AttackPhase.Windup && timer >= windupDuration)
		{
			// Debug.Log("Release");
			currentAttackPhase = AttackPhase.Release;
			animator.SetFloat("AttackSpeed", 1f / releaseDuration);
			timer = 0f;
		}
		else if (currentAttackPhase == AttackPhase.Release && timer >= releaseDuration)
		{
			currentAttackPhase = AttackPhase.Recovery;
			if (!doAttack)
			{
				// Debug.Log("Recovery");
				animator.SetFloat("AttackSpeed", 0f);
				animator.CrossFadeInFixedTime("Combat Layer.Empty", recoveryDuration);
				timer = 0f;
			}
		}
		else if (currentAttackPhase == AttackPhase.Recovery && timer >= recoveryDuration)
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
