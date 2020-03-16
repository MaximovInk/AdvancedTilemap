using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace AdvancedTilemap.Extra
{
	public class FPS_UI : MonoBehaviour
	{
		public Text Text;

		private void Update()
		{
			Text.text = "FPS : " + (int)(1f / Time.smoothDeltaTime) + " (" + Time.smoothDeltaTime.ToString("F2") + "ms)";
		}

	}
}
