using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Code
{
	[RequireComponent(typeof(Actor))]
	[DefaultExecutionOrder(10)]
	class RandomActorAI : MonoBehaviour, ActorAI
	{
		public class Action
		{
			public Ability Ability;
			public Ability Target;
		}

		public Actor Actor { get; private set; }

		public void Awake()
		{
			Actor = GetComponent<Actor>();
		}


		public ActorAI.Action Act()
		{
			if (!Actor.CanAct)
				return null;

			List<Ability> validAbilities = Actor.Abilities;

			validAbilities.RemoveAll(a => !a.CanExecute(BattleGameState.Instance.Actors));

			if (validAbilities.Count == 0)
				return null;

			var ability = validAbilities[Random.Range(0, validAbilities.Count)];
			var validTargets = ability.GenerateTargets(BattleGameState.Instance.Actors).ToList();
			var target = validTargets[Random.Range(0, validTargets.Count)];

			return new ActorAI.Action()
			{
				Ability = ability,
				Target = target,
			};
		}
	}
}
