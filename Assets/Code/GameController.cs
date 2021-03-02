using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Code
{
	[DefaultExecutionOrder(-10000)]
	public class GameController : MonoBehaviour
	{
		[System.Serializable]
		public class LevelSettings
		{
			public int MinEnemies;
			public int MaxEnemies;
			public int Budget;
			public int MinUnitCost;
			public int MaxUnitCost;

			public List<Actor> Fixed;
		}

		public static GameController Instance { get; private set; }

		public int Level { get; private set; } = 0;

		public int Scrap = 0;
		public BattleGameState BattleState;

		public List<Actor> PartyDefinition;

		public List<Actor> Party { get; private set; } = new List<Actor>();

		public List<Actor> Enemies = new List<Actor>();

		public Tooltip Tooltip;

		public List<Buff> Buffs;


		public List<Ability> ShopAbilities;

		public List<LevelSettings> Levels;


		public void NewBattle()
		{
			++Level;

			var settings = Levels[Level - 1];
			List<Actor> enemies = new List<Actor>();

			if (settings.Fixed.Count != 0) {
				enemies.AddRange(settings.Fixed);
			} else {
				int budget = settings.Budget;
				while (budget >= settings.MinUnitCost && enemies.Count < settings.MaxEnemies) {
					int maxUnitCost = Mathf.Min(budget, settings.MaxUnitCost);
					var enemy = GetRandomEnemy(settings.MinUnitCost, maxUnitCost);
					if (enemy == null)
						break;

					enemies.Add(enemy);
					budget -= enemy.ScrapReward;
				}
			}

			BattleState.Init(Party, enemies);
			BattleState.gameObject.SetActive(true);
		}

		private Actor GetRandomEnemy(int minBudget, int maxBudget)
		{
			var validUnits = Enemies.Where(c => c.ScrapReward >= minBudget && c.ScrapReward <= maxBudget).ToList();
			if (validUnits.Count == 0)
				return null;

			int r = UnityEngine.Random.Range(0, validUnits.Count);
			return validUnits[r];
		}

		private void Awake()
		{
			Instance = this;
		}

		// Start is called before the first frame update
		void Start()
		{
			UnityEngine.Random.InitState(System.Environment.TickCount);

			Tooltip.gameObject.SetActive(false);

			if (Party.Count == 0) {
				foreach (var p in PartyDefinition) {
					var o = GameObject.Instantiate(p);
					p.SetToMaxHP();
					Party.Add(o);
					o.gameObject.SetActive(false);
				}
			}

			NewBattle();
		}

		// Update is called once per frame
		void Update()
		{

		}
	}

}