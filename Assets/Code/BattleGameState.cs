﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Code
{
	[DefaultExecutionOrder(-1000)]
	public class BattleGameState : MonoBehaviour
	{
		[Header("GUI")]
		public Transform AbiltyBar;
		public UIAbility AbilityPrefab;
		public Button EndTurnButton;
		public Text ScrapText;

		public GameObject VictoryScreen;
		public Button NextBattleButton;

		public List<Actor> PlayerParty { get; private set; }
		public List<Actor> EnemyParty { get; private set; }
		public List<UIAbility> Abilities { get; private set; } = new List<UIAbility>();

		public Transform PlayerRoot;
		public Transform EnemeyRoot;

		public IEnumerable<Actor> Actors => PlayerParty.Concat(EnemyParty);

		public Actor SelectedActor = null;
		public Ability SelectedAbility = null;

		private Camera _camera;

		public BattleGameStateType State;

		private bool IsEnemyTurn;

		private bool AutoEndTurn = false;

		public static BattleGameState Instance { get; private set; }

		public void Awake()
		{
			_camera = Camera.main;
			Instance = this;
			EndTurnButton.onClick.AddListener(() =>
			{
				EndTurn();
			});
			NextBattleButton.onClick.AddListener(NextBattleClicked);
		}

		public void OnEnable()
		{
			PlayerParty = PlayerRoot.transform.GetComponentsInChildren<Actor>().ToList();
			EnemyParty = EnemeyRoot.transform.GetComponentsInChildren<Actor>(includeInactive:true).ToList();

			foreach (var ability in AbiltyBar.GetComponentsInChildren<UIAbility>())
				Destroy(ability.gameObject);
		}


		public void Init(List<Actor> playerParty, List<Actor> enemyPartyDef)
		{
			SelectedActor = null;
			SelectedAbility = null;

			IsEnemyTurn = false;

			VictoryScreen.gameObject.SetActive(false);

			PlayerParty.Clear();

			int i = 0;
			foreach (var p in playerParty) {


				if (p.IsAlive) {
					p.transform.localPosition = new Vector3(i / 3 * -3f, -(i - 1) * 2.75f, 0);
					if (i == 3) {
						var v = p.transform.localPosition;
						v.y = 0;
						p.transform.localPosition = v;
					}
					p.transform.SetParent(PlayerRoot.transform, worldPositionStays: false);
					p.gameObject.SetActive(true);

					PlayerParty.Add(p);
				}

				++i;
			}

			foreach(var e in EnemyParty) {
				Destroy(e.gameObject);
			}
			EnemyParty.Clear();


			i = 0;
			foreach (var def in enemyPartyDef) {

				int onColumns = enemyPartyDef.Count - (i / 3) * 3;
				if (onColumns > 3)
					onColumns = 3;
				float y = 0;


				if (onColumns == 1) {
					y = 2.75f;
				}
				else if (onColumns == 2) {
					y = 2.75f / 2.0f;
				}

				var p = Instantiate(def, EnemeyRoot.transform);
				p.transform.localPosition = new Vector3(i / 3 * 3f, (i % 3 - 1) * 2.75f + y, 0);
				p.SetToMaxHP();
				p.gameObject.SetActive(true);

				EnemyParty.Add(p);


				++i;
			}

			SetSelection(PlayerParty.FirstOrDefault());

			foreach (var p in PlayerParty)
				p.NewTurn();

			ToIdleState();
		}

		public void Start()
		{
			SetSelection(PlayerParty.FirstOrDefault());
		}

		public void EndTurn()
		{

			if (!IsEnemyTurn) {
				foreach (var actor in PlayerParty.Where(p => p.PlayerControllable))
					actor.RemoveActionPoints();

				if (PlayerParty.Any(p => !p.PlayerControllable && p.CanAIAct)) {
					AutoEndTurn = true;
					PlayerAIAct();
					return;
				}
			}

			switch (State) {
				case BattleGameStateType.AbilitySelected:
				case BattleGameStateType.PartyIdle: {

						IsEnemyTurn = !IsEnemyTurn;
						AutoEndTurn = false;

						ToIdleState();

						if (IsEnemyTurn) {
							foreach (var actor in EnemyParty) {
								actor.NewTurn();
							}

							SetSelection(null);

							EnemyAIAct();
						} else {

							foreach (var actor in PlayerParty) {
								actor.NewTurn();
							}

							SetSelection(PlayerParty.FirstOrDefault(s => s.CanAct));

						}
						break;
					}
			}
		}

		public void NextBattleClicked()
		{
			GameController.Instance.NewBattle();
		}

		public void OnActorDied(Actor actor)
		{
			if (EnemyParty.Contains(actor)) {
				GameController.Instance.Scrap += actor.ScrapReward;

			}
		}

		private bool GameOverCheck()
		{

			if (EnemyParty.All(p => !p.IsAlive)) {
				VictoryScreen.gameObject.SetActive(true);
				return true;
			}

			return false;
		}

		public void SetSelection(Actor selection)
		{
			if (SelectedActor == selection)
				return;

			SelectedActor?.SetNormalState();

			SelectedActor = selection;


			foreach (var ability in Abilities)
				Destroy(ability.gameObject);

			Abilities.Clear();


			if (selection) {
				SelectedActor.SetSelectedState();

				foreach (var ability in selection.Abilities) {
					var obj = Instantiate(AbilityPrefab, AbiltyBar);
					obj.SetAbility(ability);
					obj.OnClicked += AbilityClicked;
					Abilities.Add(obj);
				}
			}
		}

		private void AbilityClicked(UIAbility ability)
		{
			ToPlayerAbilitySelectedState(ability.Ability);
		}

		private void ActorSelected(Actor selection)
		{
			if (State == BattleGameStateType.PartyIdle) {
				if (PlayerParty.Contains(selection) && selection.PlayerControllable)
					SetSelection(selection);
			} else if (State == BattleGameStateType.AbilitySelected) {
				if (SelectedAbility.IsValidTarget(selection)) {
					ExecuteAbility(SelectedAbility, selection);
				}
			}
		}

		private void ExecuteAbility(Ability ability, Actor target)
		{
			foreach (var actor in Actors)
				actor.SetNormalState();

			State = BattleGameStateType.AbilityExecuting;

			EndTurnButton.interactable = false;

			foreach (var a in Abilities)
				a.Disable = true;

			ability.User.SetSelectedState();

			ability.ExecuteAbility(target, AbilityExecuted);
		}

		private void AbilityExecuted()
		{
			if (GameOverCheck())
				return;

			EndTurnButton.interactable = !IsEnemyTurn;

			foreach (var a in Abilities)
				a.Disable = false;

			ToIdleState();

			if (IsEnemyTurn) {
				EnemyAIAct();

			} else {
				if (!SelectedActor.CanAct)
					SetSelection(PlayerParty.FirstOrDefault(s => s.CanAct));

				if (PlayerParty.Where(p => p.PlayerControllable).All(p => !p.CanAct)) {

					PlayerAIAct();
				} else {
					if (AutoEndTurn && !PlayerParty.Any(p => p.CanAct))
						EndTurn();
				}

			}
		}

		private void EnemyAIAct()
		{
			var next = EnemyParty.FirstOrDefault(s => s.CanAIAct);
			if (next != null) {
				AIAct(next);
			} else {
				EndTurn();
			}
		}
		private void PlayerAIAct()
		{
			var next = PlayerParty.FirstOrDefault(s => s.CanAIAct && !s.PlayerControllable);
			if (next != null) {
				AIAct(next);
			} else {
				EndTurn();
			}
		}

		private void AIAct(Actor actor)
		{
			var ai = actor.GetComponent<ActorAI>();
			if (ai == null) {
				Debug.LogError($"Actor {actor} does not have AI attached to it");
				EndTurn();
			} else {
				var action = ai.Act();
				if (action == null) {
					Debug.LogError($"Actor {actor}'s AI returned null action!");
					EndTurn();
				} else {
					ExecuteAbility(action.Ability, action.Target);
				}
			}
		}
		private void ToIdleState()
		{
			EndTurnButton.interactable = !IsEnemyTurn;

			State = BattleGameStateType.PartyIdle;

			SelectedAbility = null;

			foreach (var a in Actors)
				a.SetNormalState();

			SelectedActor?.SetSelectedState();
		}

		private void ToPlayerAbilitySelectedState(Ability ability)
		{
			State = BattleGameStateType.AbilitySelected;

			SelectedAbility = ability;

			foreach (var actor in Actors) {
				if (SelectedAbility.IsValidTarget(actor))
					actor.SetValidTargetState(EnemyParty.Contains(actor));
				else if (ability.User == actor)
					actor.SetSelectedState();
				else
					actor.SetNormalState();
			}
		}

		static KeyCode[] _abilityKeys = new KeyCode[]
		{
			 KeyCode.Alpha1,
			 KeyCode.Alpha2,
			 KeyCode.Alpha3,
			 KeyCode.Alpha4,
			 KeyCode.Alpha5,
		};

		private void Update()
		{
			ScrapText.text = $"Scrap: {GameController.Instance.Scrap}";

			if (VictoryScreen.gameObject.activeInHierarchy)
				return;

			if (State == BattleGameStateType.PartyIdle || State == BattleGameStateType.AbilitySelected) {
				if (SelectedActor != null) {
					for (int i = 0; i < _abilityKeys.Length; ++i) {
						if (Input.GetKeyUp(_abilityKeys[i])) {
							if (Abilities.Count > i && Abilities[i].enabled && Abilities[i].Ability.CanExecute(Actors)) {
								AbilityClicked(Abilities[i]);
								break;
							}
						}
					}
				}

				if (Input.GetKeyUp(KeyCode.E)) {
					EndTurn();
				}
			}

			if (Input.GetMouseButtonUp(0)) {

				var mousePos = _camera.ScreenToWorldPoint(Input.mousePosition);

				var hit = Physics2D.Raycast(mousePos, Vector2.zero);

				Actor selection = null;

				if (hit.collider != null) {
					selection = hit.collider.gameObject.GetComponent<Actor>();
				}

				if (selection != null)
					ActorSelected(selection);
			}

			if (Input.GetMouseButtonUp(1) || Input.GetKeyUp(KeyCode.Escape)) {
				if (State == BattleGameStateType.AbilitySelected)
					ToIdleState();
			}
		}
	}
}