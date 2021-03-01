using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Code
{
	[Serializable]
	public class AbilityEffect
	{
		public EffectType Type;
		public EffectTarget Target;
		public int Value;

		public void Apply(Actor user, Actor target)
		{
			switch (Type) {
				case EffectType.DealDamage: target.DealDamage(Value); break;
				case EffectType.Block: target.GainBlock(Value); break;
				case EffectType.ModifyActionPoints: target.ActionPoints += Value; break;
				default:
					throw new Exception($"Unknown effect type {Type}");
			}
		}

		public IEnumerable<Actor> GetEffectTarget(Actor user, Actor selection)
		{
			switch (Target) {
				case EffectTarget.Target: yield return selection; break;
				case EffectTarget.Self: yield return user; break;
				default:
					throw new Exception($"Unknown effect target {Target}");
			}
		}

	}
}
