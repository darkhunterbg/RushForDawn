using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Code
{
	interface ActorAI
	{
		public class Action
		{
			public Ability Ability;
			public Actor Target;
		}

		ActorAI.Action Act();
	}
}
