using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Code
{
	[DefaultExecutionOrder(1000)]
	public class Tooltip : MonoBehaviour
	{
		[Serializable]
		public class Keyword
		{
			public string Word;
			public Color Color = Color.white;

		}

		public TMPro.TMP_Text TitleText;
		public TMPro.TMP_Text ContentText;

		public TMPro.TMP_Text HealthCost;
		public GameObject HealthCostRoot;


		public List<Keyword> Keywords;

		private Canvas _canvas;

		public object Object { get; private set; }

		public void Start()
		{
			_canvas = GetComponentInParent<Canvas>();
		}

		public void ShowAbility(Ability ability)
		{
			TitleText.text = ability.Name;
			string text = string.Format(ability.Description, ability.Effects.Select(s => s.Value.ToString()).ToArray());
			foreach (var word in Keywords) {
				text = text.Replace($"[{word.Word}]", $"<color=#{ColorUtility.ToHtmlStringRGB(word.Color)}>{word.Word}</color>");
			}

			ContentText.text = text;
			HealthCost.text = ability.Cost.ToString();
			HealthCostRoot.gameObject.SetActive(true);

			Object = ability;

			UpdatePosition();

			gameObject.SetActive(true);
		}
		public void ShowBuff(Buff buff)
		{
			TitleText.text = buff.Name;
			string text = buff.Description;
			foreach (var word in Keywords) {
				text = text.Replace($"[{word.Word}]", $"<color=#{ColorUtility.ToHtmlStringRGB(word.Color)}>{word.Word}</color>");
			}

			ContentText.text = text;

			HealthCostRoot.gameObject.SetActive(false);

			Object = buff;


			UpdatePosition();

			gameObject.SetActive(true);
		}

		public void Hide()
		{
			Object = null;

			gameObject.SetActive(false);
		}

		public void Update()
		{
			UpdatePosition();
		}

		private void UpdatePosition()
		{
			if(_canvas==null)
				_canvas = GetComponentInParent<Canvas>();

			Vector2 movePos;
			Vector2 size = ((RectTransform)transform).rect.size;

			RectTransformUtility.ScreenPointToLocalPointInRectangle(
	_canvas.transform as RectTransform,
	Input.mousePosition, _canvas.worldCamera,
	out movePos);

			transform.position = _canvas.transform.TransformPoint(movePos + new Vector2(size.x / 2, size.y / 2) + new Vector2(16, -16));// - new Vector3(0, 2, 0);

		}

	}
}
