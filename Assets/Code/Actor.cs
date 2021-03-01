using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Code
{
	public class Actor : MonoBehaviour
	{
		[Header("GUI")]

		public TMPro.TMP_Text HealthText;
		public TMPro.TMP_Text BlockText;
		public TMPro.TMP_Text DamageText;
		public TMPro.TMP_Text BlockGainText;

		public GameObject HUD;

		[Header("Data")]

		public string Name;
		public int ScrapReward = 10;

		public int CurrentHealth;
		public int MaxHealth;

		public List<Ability> AbilityDefinisions;

		public List<Ability> Abilities { get; private set; } = new List<Ability>();

		public SpriteRenderer SpriteRenderer;

		public int ActionPointsPerTurn;
		public int ActionPoints;

		public int Block;

		public bool PlayerControllable = true;

		public bool CanAct => IsAlive && ActionPoints > 0;

		public bool IsAlive => CurrentHealth > 0;

		public bool CanAIAct => CanAct && Abilities.Any(a => a.CanExecute(BattleGameState.Instance.Actors));

		private float _damageTimer = 0;
		private int _damageAccumulator = 0;

		private float _blockTimer = 0;
		private int _blockAccumulator = 0;

		private float _deathTimer = 0.0f;

		public void DealDamage(int damage, bool ignoreBlock = false)
		{
			if (!ignoreBlock) {
				int remain = Block - damage;
				if (remain > 0)
					return;

				Block = 0;
				damage = -remain;
			}
			CurrentHealth -= damage;
			if (CurrentHealth <= 0)
				Kill();

			_damageAccumulator += damage;

			_damageTimer = 1.5f;
		}

		public void Kill()
		{
			if (_deathTimer == 0) {
				CurrentHealth = 0;
				_deathTimer = 1.5f;
			}
		}

		public void GainBlock(int block)
		{
			Block += block;

			_blockTimer = 1.5f;
			_blockAccumulator += block;
		}

		public void Awake()
		{
			this.name = $"{Name}_Actor";

			BlockGainText.gameObject.SetActive(false);
			DamageText.gameObject.SetActive(false);
		}

		internal void RemoveActionPoints()
		{
			ActionPoints = 0;
		}

		public void OnEnable()
		{
			foreach (var a in Abilities)
				Destroy(a);

			Abilities.Clear();

			foreach (var a in AbilityDefinisions) {
				Abilities.Add(Instantiate(a, this.transform));
			}

			foreach (var a in Abilities)
				a.User = this;

			_damageTimer = _damageAccumulator = 0;
			_blockTimer = _damageAccumulator = 0;
			_blockTimer = _blockAccumulator = 0;
			_deathTimer = 0;

			NewTurn();
		}

		public void SetToMaxHP()
		{
			CurrentHealth = MaxHealth;
		}

		public void SetNormalState()
		{
			SpriteRenderer.color = Color.gray;
		}
		public void SetSelectedState()
		{
			SpriteRenderer.color = Color.white;
		}

		public void SetValidTargetState(bool enemy)
		{
			SpriteRenderer.color = enemy ? Color.red : Color.green;
		}

		public bool IsAlly(Actor actor)
		{
			bool isPlayer = BattleGameState.Instance.PlayerParty.Contains(this);

			if(isPlayer)
				return BattleGameState.Instance.PlayerParty.Contains(actor);
			else
				return BattleGameState.Instance.EnemyParty.Contains(actor);

		}
		public bool IsEnemy(Actor actor) => !IsAlly(actor);

		public void Update()
		{
			HealthText.text = $"{CurrentHealth}/{MaxHealth}";
			BlockText.text = Block.ToString();

			if (_damageTimer > 0) {
				DamageText.gameObject.SetActive(true);
				DamageText.alpha = _damageTimer / 1.5f;
				DamageText.text = _damageAccumulator > 0 ? $"-{_damageAccumulator}" : "0";
				_damageTimer -= Time.deltaTime;
			} else if (DamageText.gameObject.activeSelf) {
				DamageText.gameObject.SetActive(false);
				_damageAccumulator = 0;
			}

			if (_blockTimer > 0) {
				BlockGainText.gameObject.SetActive(true);
				BlockGainText.alpha = _blockTimer / 1.5f;
				BlockGainText.text = _blockAccumulator > 0 ? $"+{_blockAccumulator}" : "0";
				_blockTimer -= Time.deltaTime;
			} else if (BlockGainText.gameObject.activeSelf) {
				BlockGainText.gameObject.SetActive(false);
				_blockAccumulator = 0;
			}

			if(_deathTimer > 0) {
				HUD.gameObject.SetActive(false);

				SpriteRenderer.color = new Color(1, 1, 1, _deathTimer / 1.5f);

				_deathTimer -= Time.deltaTime;

				if(_deathTimer <= 0) {
					gameObject.SetActive(false);
					BattleGameState.Instance.OnActorDied(this);
				}
			}
		}

		public void NewTurn()
		{
			ActionPoints = ActionPointsPerTurn;
			Block = 0;
		}
	}
}
