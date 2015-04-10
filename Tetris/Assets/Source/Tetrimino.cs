using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Tetrimino : MonoBehaviour {
	const int I_TETRIMINO = 0;
	const int O_TETRIMINO = 1;
	const int S_TETRIMINO = 2;
	const int Z_TETRIMINO = 3;
	const int J_TETRIMINO = 4;
	const int L_TETRIMINO = 5;
	const int T_TETRIMINO = 6;

	const int WIDE = 5;
	const int HEIGHT = 5;

	public int testx1;
	public int testy1;
	public int testx2;
	public int testy2;
	public int testx3;
	public int testy3;
	public int testx4;
	public int testy4;
	public bool test;

	struct Block {
		public float pos_x;
		public float pos_y;
		public bool[,] form;
		public Vector2[] cubePos;
		public int tetrimino;
		public float spd;
	};

	struct Blockmass {
		public bool[,] blocks;
		public Vector2 blockPos;
	};

	bool[,,] form = new bool[7, HEIGHT, WIDE];

	LinkedList<int> order = new LinkedList<int> ();
	Dictionary<Vector2, GameObject> cubes = new Dictionary<Vector2, GameObject> ();

	Block block;
	GameObject[] cube;
	GameObject[] gost;
	Blockmass blockMass;

	int top;
	float spd;
	float spdTime;
	//float debugTime;
	int blockCount;

	// Use this for initialization
	void Start () {
		FormInit ();
		OederDecision ();

		top = 0;
		spd = 1.0f;
		spdTime = Time.time;
		//debugTime = Time.time;

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

		blockMass.blocks = new bool[24, 14];
		for (int y = 0; y < 24; y++) {
			for (int x = 0; x < 14; x++) {
				blockMass.blocks [y, x] = false;
			}
		}
		blockMass.blockPos.x = 4;
		blockMass.blockPos.y = 0;
	}

	// １０個先までの出てくるパターンを決める
	void OederDecision() {
		int rnd;
		for (int i = 0; i < 10; i++) {
			rnd = Random.Range (I_TETRIMINO, T_TETRIMINO + 1);
			order.AddLast (rnd);
		}
	}

	// ブロック生成
	void GeneratTetrimino() {
		block = new Block();
		block.form = new bool[5, 5];
		block.cubePos = new Vector2[4];
		blockCount++;

		// 形
		for (int y = 0; y < HEIGHT; y++) {
			for (int x = 0; x < WIDE; x++) {
				block.form [y, x] = form [order.First.Value, y, x];
			}
		}
		block = CubePosDecision (block);

		block.tetrimino = order.First.Value;

		// 座標
		block.pos_x = -3;
		block.pos_y = 8;

		// スピード
		block.spd = spd;

		// オブジェクト配置
		cube = new GameObject [4];
		for (int i = 0; i < 4; i++) {
			cube[i] = GameObject.CreatePrimitive(PrimitiveType.Cube);
		}
		SetPosCube ();
		blockMass.blockPos.x = 4;
		blockMass.blockPos.y = 0;

		order.RemoveFirst ();
		int rnd = Random.Range (I_TETRIMINO, T_TETRIMINO + 1);
		order.AddFirst (rnd);

		//SetGost ();
	}

	// キューブ配置
	void SetPosCube() {
		for (int i = 0; i < 4; i++) {
			float x = block.pos_x + block.cubePos [i].x;
			float y = block.pos_y + 3.5f - block.cubePos [i].y;
			cube [i].transform.position = new Vector3 (x, y, 8);
			block.spd = spd;
		}
	}

	// キューブを配置する座標変換
	Block CubePosDecision(Block cube) {
		int index = 0;
		for (int y = 0; y < HEIGHT; y++) {
			for (int x = 0; x < WIDE; x++) {
				if (cube.form [y, x]) {
					cube.cubePos [index].x = x;
					cube.cubePos [index].y = y;
					index++;
				}
			}
		}

		return cube;
	}

	// ブロックを回転させる
	void RotateBulock() {
		int dx, dy;
		if (Input.GetKeyDown (KeyCode.S)) {
			if (block.tetrimino != O_TETRIMINO) {
				Block after = new Block ();
				after.form = new bool[5, 5];
				after.cubePos = new Vector2[4];
				for (int y = 0; y < 5; y++) {
					for (int x = 0; x < 5; x++) {
						after.form [4 - x, y] = block.form [y, x];
					}
				}
				
				float pos_x = block.pos_x;
				float pos_y = block.pos_y;

				after = CubePosDecision (after);

				if (!CollisionBlocks (after, 0, 0)) {
					block = after;
				
					block.pos_x = pos_x;
					block.pos_y = pos_y;
					block.spd = spd;
				}

				/*for (int i = 0; i < 4; i++) {
					Destroy (gost [i]);
				}
				SetGost ();*/
			}
		}

		if (Input.GetKeyDown (KeyCode.A)) {
			if (block.tetrimino != O_TETRIMINO) {
				Block after = new Block ();
				after.form = new bool[5, 5];
				after.cubePos = new Vector2[4];
				for (int y = 0; y < 5; y++) {
					for (int x = 0; x < 5; x++) {
						after.form [x, 4 - y] = block.form [y, x];
					}
				}

				float pos_x = block.pos_x;
				float pos_y = block.pos_y;


				after = CubePosDecision (after);

				if (!CollisionBlocks (after, 0, 0)) {
					block = after;

					block.pos_x = pos_x;
					block.pos_y = pos_y;
					block.spd = spd;
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
			block.pos_y -= 1.0f;
			for (int i = 0; i < 4; i++) {
				float x = block.pos_x + block.cubePos [i].x;
				float y = block.pos_y + block.cubePos [i].y;
				cube [i].transform.position = new Vector3 (x, y, 8);
			}
			blockMass.blockPos.y += 1;
		} else {
			int x;
			int y;
			for (int i = 0; i < 4; i++) {
				x = (int)(block.cubePos [i].x + blockMass.blockPos.x);
				y = (int)(block.cubePos [i].y + blockMass.blockPos.y);
				blockMass.blocks [y, x] = true;
				if (top > y) {
					top = y;
				}
				//Debug.Log ("x:"+x+"y:"+y);
				cubes.Add (new Vector2 (x, y), cube [i]);
			}
			RemoveBlock ();
			GeneratTetrimino ();
		}
	}

	// ブロックの移動
	void BlockMove() {
		if (Input.GetKeyDown (KeyCode.RightArrow)) {
			if (!CollisionBlocks (block, 1, 0)) {
				block.pos_x += 1.0f;
				for (int i = 0; i < 4; i++) {
					float x = block.pos_x + block.cubePos [i].x;
					float y = block.pos_y + block.cubePos [i].y;
					cube [i].transform.position = new Vector3 (x, y, 8);
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
				block.pos_x -= 1.0f;
				for (int i = 0; i < 4; i++) {
					float x = block.pos_x + block.cubePos [i].x;
					float y = block.pos_y + block.cubePos [i].y;
					cube [i].transform.position = new Vector3 (x, y, 8);
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
			x = (int)(cBlock.cubePos [i].x + blockMass.blockPos.x + mx);
			y = (int)(cBlock.cubePos [i].y + blockMass.blockPos.y + my);
			if (x < 2 || x > 11) {
				return true;
			}
			if (y < 2 || y > 21) {
				return true;
			}
			if (blockMass.blocks [y, x]) {
				test = true;
				return true;
			}
		}

		return false;
	}

	// ブロック消去
	void RemoveBlock() {
		for (int i = 0; i < 4; i++) {
			int y = (int)(block.cubePos [i].y + blockMass.blockPos.y);
			for (int j = 2; j < 12; j++) {
				if (!blockMass.blocks [y, j]) {
					break;
				}
				if (j == 11) {
					for (int x = 2; x < 12; x++) {
						Vector2 key = new Vector2 (x, y);
						if (cubes.ContainsKey (key)) {
							Destroy (cubes[key]);
							cubes.Remove (key);
						}
					}
					for (int my = 20; my >= top; my--) {
						for (int mx = 2; mx < 12; mx++) {
							Debug.Log ("x:"+mx+"y:"+my);
							Vector2 key = new Vector2 (mx, my);
							if (cubes.ContainsKey (key)) {
								blockMass.blocks [my, mx] = false;
								blockMass.blocks [my, mx + 1] = true;
								cubes [key].transform.position = new Vector3 (mx, my + 1, 8);
							}
						}
					}
				}
			}
		}
	}

	// ゴースト
	void SetGost() {
		int dy;
		for (dy = 2; dy < 22; dy++) {
			CollisionBlocks (block, 0, dy);
		}

		gost = new GameObject [4];
		for (int i = 0; i < 4; i++) {
			gost[i] = GameObject.CreatePrimitive(PrimitiveType.Cube);
			float x = block.pos_x + block.cubePos [i].x;
			float y = dy + 3.5f - block.cubePos [i].y;
			cube [i].transform.position = new Vector3 (x, y, 8);
		}
	}
}
