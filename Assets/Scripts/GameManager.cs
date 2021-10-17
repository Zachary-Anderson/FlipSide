using UnityEngine;
using UnityEngine.SceneManagement;

namespace FlipSide.Gameplay
{
	public class GameManager : MonoBehaviour
	{
		public static GameManager instance;

		private void Awake()
		{
			if (instance == null)
			{
				instance = this;
			}
			else if (instance != this)
			{
				Destroy(this);
				return;
			}

			DontDestroyOnLoad(this);
		}

		public void NextScene()
		{
			LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
		}

		public void LoadScene(int index)
		{
			SceneManager.LoadScene(index);
		}
	}
}