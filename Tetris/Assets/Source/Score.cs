using UnityEngine;
using System.Collections;

public class Score : MonoBehaviour {
	public GUIText scoreText;
	long score;
	float speedBonus;
	float erasesBonus;
	public bool result;

	// Use this for initialization
	void Start () {
		DontDestroyOnLoad (this);
		Initialize ();
	}
	
	// Update is called once per frame
	void Update () {
		scoreText.text = score.ToString();
		if (result) {
			if (score < 100) {
				scoreText.transform.position = new Vector3 (0.465f, 0.5f, 0);
			} else if (score >= 100 && score < 1000) {
				scoreText.transform.position = new Vector3 (0.39f, 0.5f, 0);
			} else if (score >= 1000 && score < 10000) {
				scoreText.transform.position = new Vector3 (0.35f, 0.5f, 0);
			} else if (score >= 10000 && score < 100000) {
				scoreText.transform.position = new Vector3 (0.324f, 0.5f, 0);
			} else if (score >= 100000 && score < 1000000) {
				scoreText.transform.position = new Vector3 (0.29f, 0.5f, 0);
			} else if (score >= 1000000 && score < 10000000) {
				scoreText.transform.position = new Vector3 (0.255f, 0.5f, 0);
			} else if (score >= 10000000 && score < 100000000) {
				scoreText.transform.position = new Vector3 (0.22f, 0.5f, 0);
			} else if (score >= 100000000 && score < 100000000) {
				scoreText.transform.position = new Vector3 (0.19f, 0.5f, 0);
			}
			scoreText.color = new Vector4 (238, 235, 133, 255);
			scoreText.fontSize = 100;
		} else {
			if (score < 100) {
				scoreText.transform.position = new Vector3 (0.24f, 0.67f, 0);
			} else if (score >= 100 && score < 1000) {
				scoreText.transform.position = new Vector3 (0.16f, 0.67f, 0);
			} else if (score >= 1000 && score < 10000) {
				scoreText.transform.position = new Vector3 (0.13f, 0.67f, 0);
			} else if (score >= 10000 && score < 100000) {
				scoreText.transform.position = new Vector3 (0.10f, 0.67f, 0);
			} else if (score >= 100000 && score < 1000000) {
				scoreText.transform.position = new Vector3 (0.07f, 0.67f, 0);
			} else if (score >= 1000000 && score < 10000000) {
				scoreText.transform.position = new Vector3 (0.04f, 0.67f, 0);
			} else if (score >= 10000000 && score < 100000000) {
				scoreText.transform.position = new Vector3 (0.01f, 0.67f, 0);
			} else if (score >= 100000000 && score < 100000000) {
				scoreText.transform.position = new Vector3 (-0.02f, 0.67f, 0);
			}
		}
	}

	// 初期化
	void Initialize() {
		score = 0;
		speedBonus = 1;
		erasesBonus = 1;
	}

	// スコアの追加
	public void AddScore(float spd, int num) {
		ChangeSpdBonus (spd);
		ChangeEraBonus (num);
		score = (int)(score + ( speedBonus * 100 * erasesBonus ));
		erasesBonus = 1;
	}

	// スピードボーナス変更
	public void ChangeSpdBonus(float spd) {
		// 現在のスピードにより変更
		if (spd <= 1 && spd > 0.8f) {
			speedBonus = 1;
		} else if (spd <= 0.8f && spd > 0.6f) {
			speedBonus = 1.5f;
		} else if (spd <= 0.6f && spd > 0.5f) {
			speedBonus = 2;
		} else if (spd <= 0.5f && spd > 0.4f) {
			speedBonus = 2.5f;
		} else if (spd <= 0.4f && spd > 0.3f) {
			speedBonus = 3;
		} else if (spd <= 0.3f && spd > 0.2f) {
			speedBonus = 3.5f;
		} else if (spd <= 0.2f && spd >= 0.1f) {
			speedBonus = 4;
		} else if (spd <= 0.1) {
			speedBonus = 6;
		}
	}

	// 同時消しボーナス変更
	public void ChangeEraBonus(int num) {
		// 同時に消した数により変更
		switch (num) {
		case 1:
			erasesBonus = 1;
			break;
		case 2:
			erasesBonus = 2.5f;
			break;
		case 3:
			erasesBonus = 4;
			break;
		case 4:
			erasesBonus = 6;
			break;
		}
	}
}
