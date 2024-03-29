﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Code
{
	public class Ability : MonoBehaviour
	{
		public Sprite Icon;
		public string Name;
		public int Cost;
		public int ScrapCost = 10;
		public string Description;

		public string Replaces;
		public int ActionPointsCost = 1;
		public ClassType Class;

		public int ExecuteCount = 1;

		public AnimationClip Clip;

		public float EffectDelay = 0;

		public AbilityTargetGeneratorType TargetGenerator;

		public List<AbilityEffect> Effects;

		[Header("Other")]
		public Actor User;



		public void Awake()
		{
			this.name = $"{Name}_Ability";
		}

		public int GetActualCost()
		{
			if (User == null)
				return Cost;

			if (User.ImmuneToCost)
				return 0;

			var costBuff = User.ActiveBuffs.Keys.FirstOrDefault(b => b.AbilityCostReduction > 0);
			if (costBuff != null)
				return Math.Max(0, Cost - costBuff.AbilityCostReduction);

			return  Cost;
		}

		public bool CanExecute(IEnumerable<Actor> actors)
		{
			return User.ActionPoints >= ActionPointsCost && actors.Any(a => IsValidTarget(a));
		}


		public bool IsValidTarget(Actor actor)
		{
			if (!actor.IsAlive)
				return false;

			if (User.ActiveBuffs.Any(b => b.Key.IsTaunted)) {
				var target = User.GetEnemies().FirstOrDefault(t => t.ActiveBuffs.Any(b => b.Key.IsTitanGuard));
				if (target != null && actor != target)
					return false;
			}


			switch (TargetGenerator) {
				case AbilityTargetGeneratorType.OneAlly: return User.IsAlly(actor);
				case AbilityTargetGeneratorType.OneAllyExcludeSelf: return User != actor && User.IsAlly(actor);
				case AbilityTargetGeneratorType.OneEnemy: return User.IsEnemy(actor);
				case AbilityTargetGeneratorType.Self: return User == actor;
				case AbilityTargetGeneratorType.AllAllies: return User.IsAlly(actor);
				case AbilityTargetGeneratorType.AllEnemies: return User.IsEnemy(actor);
			}

			return false;
		}

		public IEnumerable<Actor> GenerateTargets(IEnumerable<Actor> actors)
		{
			foreach (var actor in actors) {
				if (IsValidTarget(actor))
					yield return actor;
			}

		}

		public void ExecuteAbility(Actor selection, Action endCallback)
		{


			User.ActionPoints -= ActionPointsCost;

			StartCoroutine(WaitForAnimationCrt(selection, endCallback));

		}

		private AbilityExecutionContext _context;

		private void ApplyEffect(Actor selection)
		{
			_context = new AbilityExecutionContext();

			int thorns = 0;

			foreach (var effect in Effects) {
				var targets = effect.GetEffectTarget(User, selection, _context);


				foreach (var target in targets) {
					effect.Apply(User, target, _context);
					thorns += target.ActiveBuffs.Keys.Sum(s => s.TakeDamageWhenAttacked);
					_context.LastTarget = target;
				}
			}

			if (thorns > 0 && Effects.Any(f => f.IsDamageEffect)) {
				User.DealDamage(thorns);
			}

		}

		private IEnumerator WaitForAnimationCrt(Actor target, Action endCallback)
		{
			var animation = User.GetComponentInChildren<Animation>();

			if (Clip != null && animation != null) {

				for (int i = 0; i < ExecuteCount; ++i) {
					animation.AddClip(Clip, Clip.name);
					animation.clip = Clip;
					animation.Play();

					if (EffectDelay > 0)
						yield return new WaitForSeconds(EffectDelay);

					ApplyEffect(target);

					yield return new WaitForSeconds(Clip.averageDuration - EffectDelay);

					animation.RemoveClip(Clip);


					yield return new WaitForSeconds(0.1f);
				}
			} else {
				for (int i = 0; i < ExecuteCount; ++i) {
					ApplyEffect(target);
					yield return new WaitForSeconds(0.1f);

				}
			}

			if (Cost > 0 && !User.ImmuneToCost) {
				var costBuff = User.ActiveBuffs.Keys.FirstOrDefault(b => b.AbilityCostReduction > 0);
				if (costBuff == null)
					User.DealDamage(Cost, ignoreBlock: true);
				else {
					User.ActiveBuffs[costBuff]--;

					var cost = GetActualCost();
					if(cost > 0)
						User.DealDamage(cost, ignoreBlock: true);

					User.RefreshBuffs();
				}
			}

			yield return new WaitForSeconds(0.1f);


			endCallback?.Invoke();

		}
	}
}
