using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Code
{
	public enum BattleGameStateType
	{
		PartyIdle,
		AbilitySelected,
		AbilityExecuting,
	}

	public enum AbilityTargetGeneratorType
	{
		None,
		Self,
		OneEnemy,
		OneAlly,
		OneAllyExcludeSelf,
		AllEnemies,
	}

	public enum EffectType
	{
		DealDamage,
		Block,
		ModifyActionPoints,
		DealDamageHealthScaled,
		AddBlockFromDamage,
	}

	public enum EffectTarget
	{
		Target,
		Self,
		RandomEnemy,
		AllEnemies,
	}

	public enum ClassType
	{
		None,
		Volt,
		Null,
		Trace,
		Egg
	}
}
