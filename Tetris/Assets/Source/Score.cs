using UnityEngine;
using System.Collections;

public class Score : MonoBehaviour {
	public GUIText scoreText;
	int score;
	int speedBonus;
	int erasesBonus;
	// Use this for initialization
	void Start () {
		Initialize ();
	}
	
	// Update is called once per frame
	void Update () {
		scoreText.text = score.ToString();
	}

	// 初期化
	void Initialize() {
		score = 0;
		speedBonus = 1;
		erasesBonus = 1;
	}

	// スコアの追加
	public void AddScore() {
		score = score + ( speedBonus * 100 * erasesBonus );
		erasesBonus = 1;
	}

	// スピードボーナス変更
	public void ChangeSpdBonus() {
		// 現在のスピードにより変更
	}

	// 同時消しボーナス変更
	public void ChangeEraBonus() {
		// 同時に消した数により変更
	}
}
