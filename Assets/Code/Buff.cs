using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Code
{

	[Serializable]
	public class Buff
	{
		public Sprite Icon;
		public string Name;
		public string Description;


		public bool KeepBlockNextTurn;
		public bool IsTitanGuard;
		public bool IsTaunted;

		public void Apply(Actor actor)
		{
			if (actor.ActiveBuffs.ContainsKey(this))
				actor.ActiveBuffs[this]++;
			else
				actor.ActiveBuffs.Add(this, 1);

			actor.UpdateBuffsGUI();
		}

		public bool TickOnStartTurn = false;

		public override string ToString() => Name;
	}

}
