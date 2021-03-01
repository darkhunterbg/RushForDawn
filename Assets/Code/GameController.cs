using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Code
{
	[DefaultExecutionOrder(-10000)]
	public class GameController : MonoBehaviour
	{
		public static GameController Instance { get; private set; }

		public int Level = 0;

		public int Scrap = 0;
		public BattleGameState BattleState;

		public List<Actor> PartyDefinition;

		public List<Actor> Party { get; private set; } = new List<Actor>();

		public Actor Enemy;

		public void NewBattle()
		{
			++Level;
			List<Actor> enemies = new List<Actor>();
			for (int i = 0; i < Level; ++i)
				enemies.Add(Enemy);
			BattleState.Init(Party, enemies);
			BattleState.gameObject.SetActive(true);
		}

		private void Awake()
		{
			Instance = this;
		}

		// Start is called before the first frame update
		void Start()
		{
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