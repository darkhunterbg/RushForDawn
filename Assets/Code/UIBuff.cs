using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Code
{
	public class UIBuff : MonoBehaviour 
	{
		public SpriteRenderer Renderer;

		public Buff Buff { get; private set; }

		public int Stack { get; private set; }

		public void Init(Buff buff,int stack)
		{
			Renderer.sprite = buff.Icon;
			Buff = buff;
			Stack = stack;
		}
	}
}
