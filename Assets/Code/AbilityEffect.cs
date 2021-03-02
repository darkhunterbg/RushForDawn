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
		public Actor LastTarget;
	}

	[Serializable]
	public class AbilityEffect
	{
		public EffectType Type;
		public EffectTarget Target;
		public int Value;

		public bool IsDamageEffect => Type == EffectType.DealDamage || Type == EffectType.DealDamageHealthScaled
			|| Type == EffectType.DealDamageFromBlock || Type == EffectType.DealDamageUnblockable;

		public void Apply(Actor user, Actor target, AbilityExecutionContext context)
		{

			switch (Type) {
				case EffectType.DealDamage: context.DamageDealt += target.DealDamage(DamageScale(user, Value)); break;
				case EffectType.Block: context.BlockGained += target.GainBlock(BlockScale(user,Value)); break;
				case EffectType.ModifyActionPoints: target.ActionPoints += Value; break;
				case EffectType.DealDamageHealthScaled: context.DamageDealt += target.DealDamage(DamageScale(user, user.MissingHealth * Value)); break;
				case EffectType.AddBlockFromDamage:
					context.BlockGained += target.GainBlock(Value * context.DamageDealt); break;
				case EffectType.AddBuff:
					var buff = GameController.Instance.Buffs[Value];
					buff.Apply(target);
					break;
				case EffectType.DealDamageFromBlock:
					context.DamageDealt += target.DealDamage(DamageScale(user, Value * user.Block)); break;
				case EffectType.DealDamageUnblockable:
					context.DamageDealt += target.DealDamage(DamageScale(user, Value), ignoreBlock: true); break;
				default:
					throw new Exception($"Unknown effect type {Type}");
			}
		}

		private int DamageScale(Actor user, int damage)
		{
			damage += user.Strength;

			if (user.ActiveBuffs.Any(v => v.Key.Weak)) {
				return UnityEngine.Mathf.CeilToInt((damage * 75) / 100);
			} else {
				return damage;
			}
		}
		private int BlockScale(Actor user, int block)
		{
			return block + user.Dexterity;
		}

		public IEnumerable<Actor> GetEffectTarget(Actor user, Actor selection, AbilityExecutionContext context)
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
				case EffectTarget.RandomAlly: {

						var allies = user.GetAllies().ToList();
						if (allies.Count == 0) {
							yield break;
						}

						int r = UnityEngine.Random.Range(0, allies.Count);

						yield return allies[r];

						break;
					}
				case EffectTarget.AllEnemies: {

						foreach (var e in user.GetEnemies())
							yield return e;

						break;
					}
				case EffectTarget.AllAllies: {

						foreach (var e in user.GetAllies())
							yield return e;

						break;
					}
				case EffectTarget.LastTarget: {
						yield return context.LastTarget;

						break;
					}
				default:
					throw new Exception($"Unknown effect target {Target}");
			}
		}

	}
}
