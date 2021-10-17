using UnityEngine;

namespace FlipSide.Gameplay
{
	public class CompleteLevel : MonoBehaviour
	{
		private void OnTriggerEnter2D(Collider2D collider)
		{
			if (collider.gameObject.layer == gameObject.layer)
			{
				GameManager.instance.NextScene();
			}
		}
	}
}