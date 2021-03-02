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

		public UIBuff BuffPrefab;
		public Transform BuffRoot;

		public GameObject HUD;

		public Sprite Icon;

		[Header("Data")]

		public ClassType Class;

		public string Name;
		public int ScrapReward = 10;

		public int CurrentHealth;
		public int MaxHealth;
		public int MissingHealth => MaxHealth - CurrentHealth;

		public List<Ability> AbilityDefinisions;

		public List<Ability> Abilities { get; private set; } = new List<Ability>();

		public SpriteRenderer SpriteRenderer;

		public Dictionary<Buff, int> ActiveBuffs { get; private set; } = new Dictionary<Buff, int>();

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

		public int DealDamage(int damage, bool ignoreBlock = false)
		{
			int mod = 100;

			if (ActiveBuffs.Any(a => a.Key.MegaVulnerable)) {
				mod += 50;
			}

			if (ActiveBuffs.Any(a => a.Key.Vulnerable)) {
				mod += 25;
			}

			if (mod != 100)
				damage = (damage * mod) / 100;

			if (damage > 0 && ActiveBuffs.Any(b => b.Key.TakeNoDamage))
				damage = 1;

			if (!ignoreBlock) {
				int remain = Block - damage;
				if (remain > 0) {
					Block = remain;
					damage = 0;
				} else {
					Block = 0;
					damage = -remain;
				}
			}
			CurrentHealth -= damage;
			if (CurrentHealth <= 0)
				Kill();

			_damageAccumulator += damage;

			_damageTimer = 1.5f;

			return damage;
		}

		public void Kill()
		{
			if (_deathTimer == 0) {
				CurrentHealth = 0;
				_deathTimer = 1.5f;
			}
		}

		public int GainBlock(int block)
		{
			Block += block;

			_blockTimer = 1.5f;
			_blockAccumulator += block;

			return block;
		}

		public void Awake()
		{
			this.name = $"{Name}_Actor";

			BlockGainText.gameObject.SetActive(false);
			DamageText.gameObject.SetActive(false);

			SetToMaxHP();

		}

		internal void RemoveActionPoints()
		{
			ActionPoints = 0;
		}


		public void Init()
		{
			foreach (var a in Abilities)
				Destroy(a.gameObject);

			Abilities.Clear();

			ActiveBuffs.Clear();

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

			if (isPlayer)
				return BattleGameState.Instance.PlayerParty.Contains(actor);
			else
				return BattleGameState.Instance.EnemyParty.Contains(actor);

		}
		public bool IsEnemy(Actor actor) => !IsAlly(actor);

		public IEnumerable<Actor> GetEnemies()
		{
			bool isPlayer = BattleGameState.Instance.PlayerParty.Contains(this);

			IEnumerable<Actor> result = isPlayer ? BattleGameState.Instance.EnemyParty : BattleGameState.Instance.PlayerParty;

			return result.Where(t => t.IsAlive);

		}
		public IEnumerable<Actor> GetAllies(bool excludeSelf = false)
		{
			bool isPlayer = BattleGameState.Instance.PlayerParty.Contains(this);

			IEnumerable<Actor> result = isPlayer ? BattleGameState.Instance.PlayerParty : BattleGameState.Instance.EnemyParty;

			if (excludeSelf)
				result = result.Where(t => t != this);

			return result.Where(t => t.IsAlive);
		}

		public void Update()
		{
			HealthText.text = $"{CurrentHealth}/{MaxHealth}";
			BlockText.text = Block.ToString();



			if (_damageTimer > 0) {
				DamageText.gameObject.SetActive(true);
				DamageText.alpha = _damageTimer;
				DamageText.text = _damageAccumulator > 0 ? $"-{_damageAccumulator}" : "0";
				_damageTimer -= Time.deltaTime;
			} else if (DamageText.gameObject.activeSelf) {
				DamageText.gameObject.SetActive(false);
				_damageAccumulator = 0;
			}

			if (_blockTimer > 0) {
				BlockGainText.gameObject.SetActive(true);
				BlockGainText.alpha = _blockTimer;
				BlockGainText.text = _blockAccumulator > 0 ? $"+{_blockAccumulator}" : "0";
				_blockTimer -= Time.deltaTime;
			} else if (BlockGainText.gameObject.activeSelf) {
				BlockGainText.gameObject.SetActive(false);
				_blockAccumulator = 0;
			}

			if (_deathTimer > 0) {
				HUD.gameObject.SetActive(false);

				SpriteRenderer.color = new Color(1, 1, 1, _deathTimer / 1.5f);

				_deathTimer -= Time.deltaTime;

				if (_deathTimer <= 0) {
					gameObject.SetActive(false);
					BattleGameState.Instance.OnActorDied(this);
				}
			}
		}

		public void NewTurn()
		{
			ActionPoints = ActionPointsPerTurn;


			bool keepBlock = ActiveBuffs.Any(s => s.Key.KeepBlockNextTurn);

			foreach (var buff in ActiveBuffs.Keys.Where(b => b.TickOnStartTurn).ToList()) {
				ActiveBuffs[buff]--;
				if (ActiveBuffs[buff] <= 0 || buff.RemoveAllStacksOnTick)
					ActiveBuffs.Remove(buff);
			}

			UpdateBuffsGUI();

			if (keepBlock) {
				// Nothing we have to keep block
			} else
				Block = 0;


		}

		public void EndTurn()
		{
			ActionPoints = 0;

			foreach (var buff in ActiveBuffs.Keys.Where(b => !b.TickOnStartTurn).ToList()) {
				ActiveBuffs[buff]--;
				if (ActiveBuffs[buff] <= 0 || buff.RemoveAllStacksOnTick)
					ActiveBuffs.Remove(buff);
			}

			UpdateBuffsGUI();
		}

		public void RefreshBuffs()
		{
			foreach (var buff in ActiveBuffs.Where(b => b.Value == 0).ToList()) {
				ActiveBuffs.Remove(buff.Key);
			}

			UpdateBuffsGUI();
		}
		public void UpdateBuffsGUI()
		{
			var buffs = GetComponentsInChildren<UIBuff>(BuffRoot);

			for (int i = 0; i < ActiveBuffs.Count; ++i) {
				var b = ActiveBuffs.ElementAt(i);

				if (i >= buffs.Length) {
					var buff = GameObject.Instantiate(BuffPrefab, BuffRoot);
					buff.Init(b.Key, b.Value);
					buff.transform.localPosition = new Vector3(i * 0.5f, 0, 0);
				} else {
					buffs[i].Init(b.Key, b.Value);
					buffs[i].transform.localPosition = new Vector3(i * 0.5f, 0, 0);
				}
			}
			for (int j = ActiveBuffs.Count; j < buffs.Length; ++j) {
				Destroy(buffs[j].gameObject);
			}
		}

	}
}
