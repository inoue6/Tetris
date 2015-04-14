﻿using UnityEngine;
using System.Collections;

public class Score : MonoBehaviour {
	public GUIText scoreText;
	int score;
	float speedBonus;
	float erasesBonus;
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
	public void AddScore(float spd, int num) {
		ChangeSpdBonus (spd);
		ChangeEraBonus (num);
		score = (int)(score + ( speedBonus * 100 * erasesBonus ));
		erasesBonus = 1;
	}

	// スピードボーナス変更
	public void ChangeSpdBonus(float spd) {
		// 現在のスピードにより変更
		if (spd <= 1 && spd > 0.5f) {
			speedBonus = 1;
		} else if (spd <= 0.5f && spd > 0.25f) {
			speedBonus = 1.5f;
		} else if (spd <= 0.25f && spd > 0.125f) {
			speedBonus = 2;
		} else if (spd <= 0.125f && spd > 0.0625f) {
			speedBonus = 2.5f;
		} else if (spd <= 0.0625f && spd > 0.03125f) {
			speedBonus = 3;
		} else if (spd <= 0.03125f && spd > 0.015625f) {
			speedBonus = 3.5f;
		} else if (spd <= 0.015625f && spd >= 0.01f) {
			speedBonus = 4;
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
