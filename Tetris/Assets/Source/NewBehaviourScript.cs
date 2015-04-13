﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NewBehaviourScript : MonoBehaviour {
	const int I_TETRIMINO = 0;
	const int O_TETRIMINO = 1;
	const int S_TETRIMINO = 2;
	const int Z_TETRIMINO = 3;
	const int J_TETRIMINO = 4;
	const int L_TETRIMINO = 5;
	const int T_TETRIMINO = 6;

	const int WIDE = 5;
	const int HEIGHT = 5;

	struct Cube {
		public GameObject cube;		// キューブ
		public Vector2 placePos;	// 配置する座標
		public Vector2 cubePos;		// 存在するブロックの位置
	};

	struct Block {
		public Vector2 pos;			// 左端の位置
		public bool[,] form;		// 形
		public int tetrimino;		// 形の種類
		public float spd;			// 落下するスピード
		public Cube[] cubes;		// キューブ４個
	};

	struct Blockmass {
		public Cube[,] blocks;		// ブロックの塊
		public bool[,] there;		// 有無
		public Vector2 blockPos;	// 落下しているブロックの位置
		public int top;				// ブロックの一番高い位置
	};

	bool[,,] form = new bool[7, HEIGHT, WIDE];	// それぞれの形

	LinkedList<int> order = new LinkedList<int> ();		// ブロックが出てくる順番

	Block block;			// 落下してくるブロック
	Blockmass blockMass;	// ブロックの塊
	int blockCount;			// 落下していったブロックの数
	float spd;				// 
	float spdTime;

	// Use this for initialization
	void Start () {
		FormInit ();
		OederDecision ();

		spd = 1.0f;
		spdTime = Time.time;

		GeneratTetrimino ();
	}
	
	// Update is called once per frame
	void Update () {
		BlockMove ();
		RotateBulock ();

		if (Input.GetKey (KeyCode.DownArrow)) {
			block.spd = 0.01f;
		}
		if((Time.time - spdTime) >= block.spd) {
			FallTetrimino ();
			spdTime = Time.time;
		}

		if (blockCount >= 20) {
			if (spd > 0.01f) {
				spd /= 2;
			} else {
				spd = 0.01f;
			}
			blockCount = 0;
		}
		SetPosCube ();
	}

	// 形状の初期化
	void FormInit() {
		for (int i = 0; i <= T_TETRIMINO ; i++) {
			for (int y = 0; y < HEIGHT; y++) {
				for (int x = 0; x < WIDE; x++) {
					form [i, y, x] = false;
				}
			}
		}

		// I_TETRIMINO
		form [I_TETRIMINO, 2, 1] = true;
		form [I_TETRIMINO, 2, 2] = true;
		form [I_TETRIMINO, 2, 3] = true;
		form [I_TETRIMINO, 2, 4] = true;

		// O_TETRIMINO
		form [O_TETRIMINO, 2, 1] = true;
		form [O_TETRIMINO, 3, 1] = true;
		form [O_TETRIMINO, 2, 2] = true;
		form [O_TETRIMINO, 3, 2] = true;

		// S_TETRIMINO
		form [S_TETRIMINO, 3, 1] = true;
		form [S_TETRIMINO, 2, 2] = true;
		form [S_TETRIMINO, 3, 2] = true;
		form [S_TETRIMINO, 2, 3] = true;

		// Z_TETRIMINO
		form [Z_TETRIMINO, 2, 1] = true;
		form [Z_TETRIMINO, 2, 2] = true;
		form [Z_TETRIMINO, 3, 2] = true;
		form [Z_TETRIMINO, 3, 3] = true;

		// J_TETRIMINO
		form [J_TETRIMINO, 2, 1] = true;
		form [J_TETRIMINO, 2, 2] = true;
		form [J_TETRIMINO, 2, 3] = true;
		form [J_TETRIMINO, 3, 3] = true;

		// L_TETRIMINO
		form [L_TETRIMINO, 2, 1] = true;
		form [L_TETRIMINO, 3, 1] = true;
		form [L_TETRIMINO, 2, 2] = true;
		form [L_TETRIMINO, 2, 3] = true;

		// T_TETRIMINO
		form [T_TETRIMINO, 2, 1] = true;
		form [T_TETRIMINO, 2, 2] = true;
		form [T_TETRIMINO, 3, 2] = true;
		form [T_TETRIMINO, 2, 3] = true;

		blockMass.blocks = new Cube[24, 14];
		blockMass.there = new bool[24, 14];
		for (int y = 0; y < 24; y++) {
			for (int x = 0; x < 14; x++) {
				blockMass.there [y, x] = false;
			}
		}
		blockMass.blockPos.x = 4;
		blockMass.blockPos.y = 0;
		blockMass.top = 0;
	}

	// １０個先までの出てくるパターンを決める
	void OederDecision() {
		int rnd;
		for (int i = 0; i < 10; i++) {
			rnd = 6/*Random.Range (I_TETRIMINO, T_TETRIMINO + 1)*/;
			order.AddLast (rnd);
		}
	}

	// ブロック生成
	void GeneratTetrimino() {
		block = new Block();
		block.cubes = new Cube[4];
		block.form = new bool[5, 5];
		blockCount++;

		// 形
		for (int y = 0; y < HEIGHT; y++) {
			for (int x = 0; x < WIDE; x++) {
				block.form [y, x] = form [order.First.Value, y, x];
			}
		}
		block = CubePosDecision (block);
		/*for(int i = 0; i < 4; i++) {
			Debug.Log ("X:" + block.cubes [i].cubePos.x + "Y:" + block.cubes [i].cubePos.y);
		}*/

		block.tetrimino = order.First.Value;

		// 座標
		block.pos.x = -3;
		block.pos.y = 8;
		// スピード
		block.spd = spd;

		// オブジェクト配置
		for (int i = 0; i < 4; i++) {
			block.cubes[i].cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
		}
		SetPosCube ();
		blockMass.blockPos.x = 4;
		blockMass.blockPos.y = 0;

		order.RemoveFirst ();
		int rnd = Random.Range (I_TETRIMINO, T_TETRIMINO + 1);
		order.AddLast (6);

		//SetGost ();
	}

	// キューブを配置する座標変換
	Block CubePosDecision(Block b) {
		b.cubes = new Cube[4];
		int index = 0;
		for (int y = 0; y < HEIGHT; y++) {
			for (int x = 0; x < WIDE; x++) {
				if (b.form [y, x]) {
					b.cubes [index].cubePos.x = x;
					b.cubes [index].cubePos.y = y;
					//Debug.Log ("i:"+index+"X:"+b.cubes [index].cubePos.x+"Y:"+b.cubes [index].cubePos.y);
					index++;
				}
			}
		}

		return b;
	}

	// キューブ配置
	void SetPosCube() {
		for (int i = 0; i < 4; i++) {
			float x = block.pos.x + block.cubes [i].cubePos.x;
			float y = block.pos.y + 3.5f - block.cubes [i].cubePos.y;
			//Debug.Log (i+"X:"+block.cubes[i].cubePos.x+"Y:"+block.cubes[i].cubePos.y);
			block.cubes [i].cube.transform.position = new Vector3 (x, y, 8);
			block.spd = spd;
		}
	}

	// ブロックの移動
	void BlockMove() {
		if (Input.GetKeyDown (KeyCode.RightArrow)) {
			if (!CollisionBlocks (block, 1, 0)) {
				block.pos.x += 1.0f;
				for (int i = 0; i < 4; i++) {
					float x = block.pos.x + block.cubes[i].cubePos.x;
					float y = block.pos.y + block.cubes[i].cubePos.y;
					block.cubes[i].cube.transform.position = new Vector3 (x, y, 8);
				}
				blockMass.blockPos.x += 1;

				/*for (int i = 0; i < 4; i++) {
					Destroy (gost [i]);
				}
				SetGost ();*/
			}
		}

		if (Input.GetKeyDown (KeyCode.LeftArrow)) {
			if (!CollisionBlocks (block, -1, 0)) {
				block.pos.x -= 1.0f;
				for (int i = 0; i < 4; i++) {
					float x = block.pos.x + block.cubes[i].cubePos.x;
					float y = block.pos.y + block.cubes[i].cubePos.y;
					block.cubes[i].cube.transform.position = new Vector3 (x, y, 8);
				}
				blockMass.blockPos.x -= 1;

				/*for (int i = 0; i < 4; i++) {
					Destroy (gost [i]);
				}
				SetGost ();*/
			}
		}
	}

	// ブロック塊との当たり判定
	bool CollisionBlocks(Block cBlock, int mx, int my) {
		int x;
		int y;
		for (int i = 0; i < 4; i++) {
			x = (int)(cBlock.cubes[i].cubePos.x + blockMass.blockPos.x + mx);
			y = (int)(cBlock.cubes[i].cubePos.y + blockMass.blockPos.y + my);
			if (x < 2 || x > 11) {
				return true;
			}
			if (y < 2 || y > 21) {
				return true;
			}
			if (blockMass.there [y, x]) {
				return true;
			}
		}

		return false;
	}

	// ブロックを回転させる
	void RotateBulock() {
		if (Input.GetKeyDown (KeyCode.A)) {
			if (block.tetrimino != O_TETRIMINO) {
				Block after = new Block ();
				after.cubes = new Cube[4];
				after.form = new bool[5, 5];

				after.pos.x = block.pos.x;
				after.pos.y = block.pos.y;
				after.spd = block.spd;
				after.tetrimino = block.tetrimino;

				for (int y = 0; y < 5; y++) {
					for (int x = 0; x < 5; x++) {
						after.form [4 - x, y] = block.form [y, x];
					}
				}

				after = CubePosDecision (after);

				for (int i = 0; i < 4; i++) {
					after.cubes [i].cube = block.cubes [i].cube;
				}

				if (!CollisionBlocks (after, 0, 0)) {
					block = after;
				}

				/*for (int i = 0; i < 4; i++) {
					Destroy (gost [i]);
				}
				SetGost ();*/
			}
		}

		if (Input.GetKeyDown (KeyCode.S)) {
			if (block.tetrimino != O_TETRIMINO) {
				Block after = new Block ();
				after.cubes = new Cube[4];
				after.form = new bool[5, 5];

				after.pos.x = block.pos.x;
				after.pos.y = block.pos.y;
				after.spd = block.spd;
				after.tetrimino = block.tetrimino;

				for (int y = 0; y < 5; y++) {
					for (int x = 0; x < 5; x++) {
						after.form [x, 4 - y] = block.form [y, x];
					}
				}

				after = CubePosDecision (after);

				for (int i = 0; i < 4; i++) {
					after.cubes [i].cube = block.cubes [i].cube;
				}

				if (!CollisionBlocks (after, 0, 0)) {
					block = after;
				}

				/*for (int i = 0; i < 4; i++) {
					Destroy (gost [i]);
				}
				SetGost ();*/
			}
		}
	}

	// ブロックの落下
	void FallTetrimino() {
		if (!CollisionBlocks (block, 0, 1)) {
			block.pos.y -= 1.0f;
			for (int i = 0; i < 4; i++) {
				float x = block.pos.x + block.cubes[i].cubePos.x;
				float y = block.pos.y + block.cubes[i].cubePos.y;
				block.cubes[i].cube.transform.position = new Vector3 (x, y, 8);
			}
			blockMass.blockPos.y += 1;
		} else {
			int x;
			int y;
			for (int i = 0; i < 4; i++) {
				x = (int)(block.cubes[i].cubePos.x + blockMass.blockPos.x);
				y = (int)(block.cubes[i].cubePos.y + blockMass.blockPos.y);
				blockMass.there [y, x] = true;
				if (blockMass.top > y) {
					blockMass.top = y;
				}
				Debug.Log ("x:"+x+"y:"+y);
				blockMass.blocks [y, x] = block.cubes [i];
			}
			RemoveBlock ();
			GeneratTetrimino ();
		}
	}

	// ブロック消去
	void RemoveBlock() {
		for (int i = 0; i < 4; i++) {
			int y = (int)(block.cubes[i].cubePos.y + blockMass.blockPos.y);
			for (int j = 2; j < 12; j++) {
				if (!blockMass.there [y, j]) {
					break;
				}
				if (j == 11) {
					for (int x = 2; x < 12; x++) {
						Destroy (blockMass.blocks [y, x].cube);
						blockMass.there [y, x] = false;
					}
					for (int fy = y; fy > blockMass.top; fy--) {
						for(int x = 2; x < 12; x++) {
							blockMass.there [fy, x] = blockMass.there [fy - 1, x];
							blockMass.there [fy - 1, x] = false;
							blockMass.blocks [fy, x] = blockMass.blocks [fy - 1, x];
							float px = blockMass.blocks[fy, x].placePos.x;
							float py = blockMass.blocks[fy, x].placePos.y;
							blockMass.blocks [fy, x].cube.transform.position = new Vector3 (px, py, 8);
						}
					}
					blockMass.top--;

					/*for (int ty = 2; ty < 22; ty++) {
						for (int tx = 2; tx < 12; tx++) {
							if (blockMass.there [ty, tx]) {
								Debug.Log ("X:"+tx+"Y:"+ty);
							}
						}
					}*/
				}
			}
		}
	}
}