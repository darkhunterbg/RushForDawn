using System;
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

		public AnimationClip Clip;

		public int ActionPointsCost = 1;

		public AbilityTargetGeneratorType TargetGenerator;

		public List<AbilityEffect> Effects;

		[Header("Other")]
		public Actor User;



		public void Awake()
		{
			this.name = $"{Name}_Ability";
		}

		public bool CanExecute(IEnumerable<Actor> actors)
		{
			return User.ActionPoints >= ActionPointsCost && actors.Any(a => IsValidTarget(a));
		}


		public bool IsValidTarget(Actor actor)
		{
			if (!actor.IsAlive)
				return false;

			switch (TargetGenerator) {
				case AbilityTargetGeneratorType.OneAlly: return User.IsAlly(actor);
				case AbilityTargetGeneratorType.OneAllyExcludeSelf: return User != actor && User.IsAlly(actor);
				case AbilityTargetGeneratorType.OneEnemy: return User.IsEnemy(actor);
				case AbilityTargetGeneratorType.Self: return User == actor;
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

			foreach (var effect in Effects) {
				var targets = effect.GetEffectTarget(User, selection);
				foreach (var target in targets)
					effect.Apply(User, target);
			}


			StartCoroutine(WaitForAnimationCrt(endCallback));

		}



		private IEnumerator WaitForAnimationCrt(Action endCallback)
		{
			if (Clip != null) {

				var animation = User.GetComponentInChildren<Animation>();
				if (animation != null) {
					animation.AddClip(Clip, Clip.name);
					animation.clip = Clip;
					animation.Play();

					yield return new WaitForSeconds(Clip.averageDuration);

					animation.RemoveClip(Clip);
				}

				yield return new WaitForSeconds(0.1f);
			}

			if (Cost > 0)
				User.DealDamage(Cost, ignoreBlock: true);

			yield return new WaitForSeconds(0.1f);


			endCallback?.Invoke();

		}
	}
}
