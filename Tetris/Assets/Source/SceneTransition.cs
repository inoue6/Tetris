﻿using UnityEngine;
using System.Collections;

public class SceneTransition : MonoBehaviour {
	public enum TransitionScene {
		TITLE,
		GAMEMAIN,
		RESULT,
	}
	public GameObject obj;
	public TransitionScene scene;
	float fadeAlpha = 0;
	bool isFading = false;
	public Color fadeColor = Color.black;
	public Score score;
	bool enter;

	// Use this for initialization
	void Start () {
		DontDestroyOnLoad (this);
	}

	// Update is called once per frame
	public void OnGUI () {
		if (this.isFading) {
			//色と透明度を更新して白テクスチャを描画 .
			this.fadeColor.a = this.fadeAlpha;
			GUI.color = this.fadeColor;
			GUI.DrawTexture (new Rect (0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
		}
		if (scene != TransitionScene.RESULT) {
			if (Input.GetKeyDown (KeyCode.Return) && !enter) {
				enter = true;
				LoadLevel ();
			}
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
			Destroy (GameObject.Find ("ScoreNum"));
			Application.LoadLevel ("Title");
			Debug.Log ("Title");
			break;
		case TransitionScene.GAMEMAIN:
			Application.LoadLevel ("GameMain");
			Debug.Log ("GameMain");
			break;
		case TransitionScene.RESULT:
			score.result = true;
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
		Destroy (obj);

		//this.isFading = false;
	}
}