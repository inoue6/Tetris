using UnityEngine;
using System.Collections;

public class Sceneransition : MonoBehaviour {
	public enum TransitionScene {
		TITLE,
		GAMEMAIN,
		RESULT,
	}
	public TransitionScene scene;
	float fadeAlpha = 0;
	bool isFading = false;
	public Color fadeColor = Color.black;


	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	public void OnGUI () {
		if (this.isFading) {
			//色と透明度を更新して白テクスチャを描画 .
			this.fadeColor.a = this.fadeAlpha;
			GUI.color = this.fadeColor;
			GUI.DrawTexture (new Rect (0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
		}
		if (Input.GetKeyDown (KeyCode.Return)) {
			LoadLevel ();
		}
	}

	public void LoadLevel ()
	{
		StartCoroutine (TransScene ());
	}

	IEnumerator TransScene ()
	{
		//だんだん暗く
		this.isFading = true;
		float time = 0;
		while (time <= 1.0f) {
			this.fadeAlpha = Mathf.Lerp (0f, 1f, time / 1.0f);      
			time += Time.deltaTime;
			yield return 0;
		}

		//シーン切替
		switch (scene) {
		case TransitionScene.TITLE:
			Application.LoadLevel ("Title");
			Debug.Log ("Title");
			break;
		case TransitionScene.GAMEMAIN:
			Application.LoadLevel ("GameMain");
			Debug.Log ("GameMain");
			break;
		case TransitionScene.RESULT:
			Application.LoadLevel ("Result");
			Debug.Log ("Result");
			break;
		}

		//だんだん明るく
		time = 0;
		while (time <= 1.0f) {
			this.fadeAlpha = Mathf.Lerp (1f, 0f, time / 1.0f);
			time += Time.deltaTime;
			yield return 0;
		}

		//this.isFading = false;
	}
}