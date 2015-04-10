using UnityEngine;
using System.Collections;

public class Sceneransition : MonoBehaviour {
	public enum TransitionScene {
		TITLE,
		GAMEMAIN,
		RESULT,
	}
	public TransitionScene scene;
	/*float time;
	public GameObject blackTexture;
	float alpha;*/

	// Use this for initialization
	void Start () {
		/*time = 0.0f;
		alpha = 0;*/
	}

	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.Space)) {

			/*time = 0.0f;
			while (true) {
				this.alpha = Mathf.Lerp (0.0f, 1.0f, time / 1.0f);
				blackTexture.
				time += Time.deltaTime;
				if (time >= 1.0f) {
					break;
				}
			}*/

			switch (scene) {
			case TransitionScene.TITLE:
				Application.LoadLevel ("Title");
				break;
			case TransitionScene.GAMEMAIN:
				Application.LoadLevel ("GameMain");
				break;
			case TransitionScene.RESULT:
				Application.LoadLevel ("Result");
				break;
			}

			/*time = 0.0f;
			while (true) {
				this.alpha = Mathf.Lerp (1.0f, 0.0f, time / 1.0f);
				time += Time.deltaTime;

				if (time >= 1.0f) {
					break;
				}
			}*/
		}
	}
}