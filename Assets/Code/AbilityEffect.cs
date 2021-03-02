using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Code
{
	public class AbilityExecutionContext
	{
		public int DamageDealt;
		public int BlockGained;
	}

	[Serializable]
	public class AbilityEffect
	{
		public EffectType Type;
		public EffectTarget Target;
		public int Value;


		public void Apply(Actor user, Actor target, AbilityExecutionContext context)
		{
			switch (Type) {
				case EffectType.DealDamage: context.DamageDealt += target.DealDamage(Value); break;
				case EffectType.Block: context.BlockGained += target.GainBlock(Value); break;
				case EffectType.ModifyActionPoints: target.ActionPoints += Value; break;
				case EffectType.DealDamageHealthScaled: context.DamageDealt+= target.DealDamage(user.MissingHealth * Value); break;
				case EffectType.AddBlockFromDamage:
					context.BlockGained += target.GainBlock(Value * context.DamageDealt); break;
				default:
					throw new Exception($"Unknown effect type {Type}");
			}
		}

		public IEnumerable<Actor> GetEffectTarget(Actor user, Actor selection)
		{
			switch (Target) {
				case EffectTarget.Target: yield return selection; break;
				case EffectTarget.Self: yield return user; break;
				case EffectTarget.RandomEnemy: {

						var enemies = user.GetEnemies().ToList();
						if (enemies.Count == 0) {
							yield break;
						}

						int r = UnityEngine.Random.Range(0, enemies.Count);

						yield return enemies[r];

						break;
					}
				case EffectTarget.AllEnemies: {

						foreach (var e in user.GetEnemies())
							yield return e;

						break;
					}
				default:
					throw new Exception($"Unknown effect target {Target}");
			}
		}

	}
}
