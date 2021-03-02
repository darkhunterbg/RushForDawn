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
		AllAllies,
	}

	public enum EffectType
	{
		DealDamage,
		Block,
		ModifyActionPoints,
		DealDamageHealthScaled,
		AddBlockFromDamage,
		AddBuff,
		DealDamageFromBlock,
		DealDamageUnblockable,
	}

	public enum EffectTarget
	{
		Target,
		Self,
		RandomEnemy,
		AllEnemies,
		AllAllies,
		RandomAlly,
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
