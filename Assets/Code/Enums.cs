﻿using System;
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
	}

	public enum EffectType
	{
		DealDamage,
		Block,
		ModifyActionPoints,
	}

	public enum EffectTarget
	{
		Target,
		Self
	}
}