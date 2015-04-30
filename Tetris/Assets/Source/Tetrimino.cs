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

	public Score score;
	public GameObject text;
	public SceneTransition st;
	public GameObject texture;

	bool generat;			// 落下防止
	bool description;		// 操作説明

	bool[,,] form = new bool[7, HEIGHT, WIDE];	// それぞれの形

	LinkedList<int> order = new LinkedList<int> ();		// ブロックが出てくる順番

	public Material redMaterial;
	public Material blueMaterial;
	public Material lightBlueMaterial;
	public Material yelloMaterial;
	public Material yelloGreenMaterial;
	public Material purpleMaterial;
	public Material orangeMaterial;
	public Material ghostMaterial;
	public Material whiteMaterial;
	public Material blackMaterial;

	Block block;			// 落下してくるブロック
	Blockmass blockMass;	// ブロックの塊
	public GameObject ghost1;
	public GameObject ghost2;
	public GameObject ghost3;
	public GameObject ghost4;
	public GameObject nextBlock1;
	public GameObject nextBlock2;
	public GameObject nextBlock3;
	public GameObject nextBlock4;
	int blockCount;			// 落下していったブロックの数
	float spd;				// 
	float spdTime;
	public bool gameOver;

	float moveSpd;			// 左右キー押しっぱなし時の移動スピード
	float moveSpdTime;

	float playTime;

	bool blackCube;			// 一列そろった時の黒テトリミノの有無

	float deleteTime = 0.0f;
	bool deleteFlag;

	// Use this for initialization
	void Start () {
		FormInit ();
		OederDecision ();

		spd = 1.0f;
		GeneratTetrimino ();

		description = true;

		moveSpd = 0.12f;
		moveSpdTime = 0;

		playTime = 0.0f;

		blackCube = false;
	}
	
	// Update is called once per frame
	void Update () {
		if (description) {
			if(Input.GetKey(KeyCode.Return)) {
				description = false;
				texture.transform.position = new Vector3(100, 0, 0);
				spdTime = 0.0f;
				moveSpdTime = 0.0f;
			}
			return;
		}

		if (!deleteFlag) {
			if (CollisionBlocks (block, 0, 1)) {
				playTime += Time.deltaTime;
			} else {
				playTime = 0.0f;
			}

			if (!generat && Input.GetKey (KeyCode.DownArrow)) {
				block.spd = 0.01f;
			}

			if (spdTime >= block.spd) {
				FallTetrimino ();
				spdTime = 0.0f;
			}
			spdTime += Time.deltaTime;

			BlockMove ();

			LeftRotateBlock ();
			RightRotateBlock ();
			
			SetPosCube ();
			if(!gameOver) {
				BlockMassSetPosition ();
			}
		}

		if (playTime > 0.0f) {
			playTime += Time.deltaTime;
			if(playTime >= 0.5) {
				playTime = 0.0f;
				deleteTime += Time.deltaTime;
			}
		}

		if (!deleteFlag && (deleteTime > 0.0f)) {
			SetPosCube ();
			if (CollisionBlocks (block, 0, 1)) {
				int x;
				int y;
				for (int i = 0; i < 4; i++) {
					x = (int)(block.cubes[i].cubePos.x + blockMass.blockPos.x);
					y = (int)(block.cubes[i].cubePos.y + blockMass.blockPos.y);
					blockMass.there [y, x] = true;
					if (blockMass.top > y) {
						blockMass.top = y;
					}
					blockMass.blocks [y, x] = block.cubes [i];
				}
				ChangeColorRemoveBlock ();
				if(!deleteFlag) {
					GeneratTetrimino ();
				}
			} else {
				playTime = 0.0f;
				deleteTime = 0.0f;
			}
		}

		if (deleteFlag) {
			ChangeColorRemoveBlock ();
		}

		if (gameOver) {
			text.transform.position = new Vector3(0.12f, 0.7f, 0);
			if(Input.GetKeyDown (KeyCode.Return)) {
				st.LoadLevel();
			}
			return;
		}

		if (blockCount >= 10) {
			if (spd > 0.01f) {
				spd -= 0.01f;
			} else {
				spd = 0.01f;
			}
			blockCount = 0;
		}

		if (!deleteFlag) {
			SetGhost ();
		} else {
			ghost1.transform.position = new Vector3(100, 100, 0);
			ghost2.transform.position = new Vector3(100, 100, 0);
			ghost3.transform.position = new Vector3(100, 100, 0);
			ghost4.transform.position = new Vector3(100, 100, 0);
		}

		if (deleteTime >= 0.8f) {
			RemoveBlock ();
			
			SetPosCube ();
			
			GeneratTetrimino ();
			deleteFlag = false;
			deleteTime = 0.0f;
		}
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
		blockMass.top = 22;
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
		generat = true;
		block = new Block ();
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

		block.tetrimino = order.First.Value;

		// 座標
		block.pos.x = -3;
		block.pos.y = 8;
		// スピード
		block.spd = spd;

		blockMass.blockPos.x = 4;
		blockMass.blockPos.y = 0;

		if (CollisionBlocks (block, 0, 0)) {
			if(gameOver)
				return;
			deleteTime += Time.deltaTime;
			FallLastBlocks();
			goto end;
		}

		// オブジェクト配置
		for (int i = 0; i < 4; i++) {
			block.cubes[i].cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
			block.cubes[i].cube.transform.localScale = new Vector3(0.97f,0.97f,1);
			SetColor(block.cubes[i].cube);
		}

		SetPosCube ();

		order.RemoveFirst ();

		int rnd;
		for (int i = 0; i < 5; i++) {
			rnd = Random.Range (I_TETRIMINO, T_TETRIMINO + 1);
			if(order.Last.Value != rnd) {
				order.AddLast (rnd);
				break;
			}
		}
		SetNextBlock ();
		SetGhost ();
	end:;
	}

	// キューブの色をブロックに合わせて設定
	void SetColor(GameObject cube)
	{
		Resources.UnloadUnusedAssets ();

		Renderer renderer = cube.GetComponent<Renderer> ();
		renderer.material = new Material (renderer.material);
		switch(order.First.Value)
		{
			// 水色
		case 0:
			renderer.material = lightBlueMaterial;
			break;

			// 黄
		case 1:
			renderer.material = yelloMaterial;
			break;

			// 黄緑
		case 2:
			renderer.material = yelloGreenMaterial;
			break;

			// 赤
		case 3:
			renderer.material = redMaterial;
			break;

			// 青
		case 4:
			renderer.material = blueMaterial;
			break;

			// オレンジ
		case 5:
			renderer.material = orangeMaterial;
			break;

			// 紫
		case 6:
			renderer.material = purpleMaterial;
			break;
		default:
			break;
		}
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
					index++;
				}
			}
		}

		return b;
	}

	// キューブ配置
	void SetPosCube() {
		if (gameOver) {
			return;
		}
		for (int i = 0; i < 4; i++) {
			float x = block.pos.x + block.cubes [i].cubePos.x;
			float y = block.pos.y + 3.5f - block.cubes [i].cubePos.y;
			block.cubes [i].placePos.x = x;
			block.cubes [i].placePos.y = y;
			block.cubes [i].cube.transform.position = new Vector3 (x, y, 8);
			block.spd = spd;
		}
	}

	// ブロックの移動
	void BlockMove() {
		if(	Input.GetKey(KeyCode.RightArrow) &&
			(moveSpdTime >= moveSpd))
		{
			if (!CollisionBlocks (block, 1, 0)) {
				block.pos.x += 1.0f;
				blockMass.blockPos.x += 1;
			}
			moveSpdTime = 0.0f;
		}
		if(	Input.GetKey(KeyCode.LeftArrow) &&
		   (moveSpdTime >= moveSpd))
		{
			if (!CollisionBlocks (block, -1, 0)) {
				block.pos.x -= 1.0f;
				blockMass.blockPos.x -= 1;
			}
			moveSpdTime = 0.0f;
		}

		moveSpdTime += Time.deltaTime;
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
			if (y < 1 || y > 21) {
				return true;
			}
			if (blockMass.there [y, x]) {
				return true;
			}
		}

		return false;
	}

	// 出口から1列分の隙間があるときの処理
	void FallLastBlocks()
	{
		for(int i=0;i<4;i++)
		{
			if(blockMass.there[0,3+i])
			{
				gameOver = true;
				break;
			}
			if(i==3)
			{
				for(int j=0;j<4;j++)
				{
					if(block.cubes[j].cubePos.y == 3 && gameOver!=true)
					{

						block.cubes[j].cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
						block.cubes[j].cube.transform.localScale = new Vector3(0.97f,0.97f,1);
						SetColor(block.cubes[j].cube);

						float x = block.pos.x + block.cubes [j].cubePos.x;
						float y = block.pos.y + 3.5f - block.cubes [j].cubePos.y;
						block.cubes [j].placePos.x = x;
						block.cubes [j].placePos.y = y;
						block.cubes [j].cube.transform.position = new Vector3 (x, y+1, 8);
					}
				}
			}
		}

		gameOver = true;
	}

	// ブロックの壁際での当たり判定
	int CollisionKickWallBlocks(Block cBlock)
	{
		int x;
		int y;
		int moveX = 0;
		for (int i = 0; i < 4; i++) {
			x = (int)(cBlock.cubes[i].cubePos.x + blockMass.blockPos.x);
			y = (int)(cBlock.cubes[i].cubePos.y + blockMass.blockPos.y);
			if(x < 2) {
				for(int j = 0; j < 4; j++) {
					int cx = (int)(blockMass.blockPos.x + cBlock.cubes[j].cubePos.x + (2-x));
					int cy = (int)(cBlock.cubes[j].cubePos.y + blockMass.blockPos.y);
					if(blockMass.there[cy, cx]) {
						return 0;
					}
				}
				if(moveX < 2-x) {
					moveX = 2-x;
				}
				continue;
			}
			if(x > 11) {
				for(int j = 0; j < 4; j++) {
					int cx = (int)(blockMass.blockPos.x + cBlock.cubes[j].cubePos.x + (11-x));
					int cy = (int)(cBlock.cubes[j].cubePos.y + blockMass.blockPos.y);
					if(blockMass.there[cy, cx]) {
						return 0;
					}
				}
				if(moveX > 11-x) {
					moveX = 11-x;
				}
				continue;
			}
			if (blockMass.there [y, x]) {
				if((blockMass.blockPos.x+1) >= x) {
					for(int j = moveX; j < 3; j++) {
						if(blockMass.there[y, x+j]) {
							if(j >= 2) {
								return 0;
							}
							continue;
						} else {
							bool hit = false;
							for(int k = 0; k < 4; k++) {
								int cx = (int)(blockMass.blockPos.x + cBlock.cubes[k].cubePos.x + j);
								int cy = (int)(cBlock.cubes[k].cubePos.y + blockMass.blockPos.y);
								if(cx > 11) {
									return 0;
								}
								if(blockMass.there[cy, cx]) {
									hit = true;
								}
							}
							if(moveX < j && !hit) {
								moveX = j;
								break;
							}
						}
					}
				} else {
					for(int j = moveX; j > -3; j--) {
						if(blockMass.there[y, x+j]) {
							if(j <= -2) {
								return 0;
							}
							continue;
						} else {
							bool hit = false;
							for(int k = 0; k < 4; k++) {
								int cx = (int)(blockMass.blockPos.x + cBlock.cubes[k].cubePos.x + j);
								int cy = (int)(cBlock.cubes[k].cubePos.y + blockMass.blockPos.y);
								if(cx < 2) {
									return 0;
								}
								if(blockMass.there[cy, cx]) {
									hit = true;
								}
							}
							if(moveX > j && !hit) {
								moveX = j;
								break;
							}
						}
					}
				}
			}
		}

		return moveX;
	}

	// ブロックを左回転させる
	void LeftRotateBlock() {
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

				// ブロックが生成されてすぐの時、回転でブロックが突き出てしまわないようにyの値を補正する
				/*if(block.tetrimino == I_TETRIMINO && blockMass.blockPos.y == 0)
				{
					for (int i = 0; i < 4; i++) {
						after.cubes [i].cubePos.y += 2;
					}
					blockMass.blockPos.y += 2;
				}
				else if(((block.tetrimino == I_TETRIMINO && blockMass.blockPos.y == 1) ||
				         (block.tetrimino != I_TETRIMINO && blockMass.blockPos.y == 0)))
				{
					for (int i = 0; i < 4; i++) {
						after.cubes [i].cubePos.y += 1;
					}
					blockMass.blockPos.y += 1;
				}*/
		
				if (!CollisionBlocks (after, 0, 0)) {
					block = after;
				}
				int moveX = CollisionKickWallBlocks (after);
				if(moveX != 0) {
					block = after;
					blockMass.blockPos.x += moveX;
					block.pos.x += moveX;
				}

				if(playTime > 0.0f) {
					if(!CollisionBlocks (block, 0, 1)) {
						playTime = 0.0f;
					}
				}
			}
		}
	}

	// ブロックを右回転させる
	void RightRotateBlock() {
		if (Input.GetKeyDown (KeyCode.S)){
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
				
				// ブロックが生成されてすぐの時、回転でブロックが突き出てしまわないようにyの値を補正する
				/*if(block.tetrimino == I_TETRIMINO && blockMass.blockPos.y == 0)
				{
					for (int i = 0; i < 4; i++) {
						after.cubes [i].cubePos.y += 2;
					}
				}
				else if(((block.tetrimino == I_TETRIMINO && blockMass.blockPos.y == 1) ||
				         (block.tetrimino != I_TETRIMINO && blockMass.blockPos.y == 0)))
				{
					for (int i = 0; i < 4; i++) {
						after.cubes [i].cubePos.y += 1;
					}
				}*/
				
				if (!CollisionBlocks (after, 0, 0)) {
					block = after;
				}
				int moveX = CollisionKickWallBlocks (after);
				if(moveX != 0) {
					block = after;
					blockMass.blockPos.x += moveX;
					block.pos.x += moveX;
				}

				if(playTime > 0.0f) {
					if(!CollisionBlocks (block, 0, 1)) {
						playTime = 0.0f;
					}
				}
			}
		}
	}

	// ブロックの落下
	void FallTetrimino() {
		if (!CollisionBlocks (block, 0, 1)) {
			block.pos.y -= 1.0f;
			blockMass.blockPos.y += 1;
			if(generat) {
				generat = false;
			}
		}
	}


	// 消えるブロックを黒くする
	void ChangeColorRemoveBlock()
	{
		deleteTime += Time.deltaTime;
		for (int i = 0; i < 4; i++) {
			int y = (int)(block.cubes[i].cubePos.y + blockMass.blockPos.y);
			for (int j = 2; j < 12; j++) {
				if (!blockMass.there [y, j]) {
					break;
				}
				if (j == 11) {
					deleteFlag = true;
					Renderer renderer;
					for (int x = 2; x < 12; x++) {
						renderer = blockMass.blocks[y, x].cube.GetComponent<Renderer> ();
						if((deleteTime < 0.15f) || 
						   (deleteTime >= 0.3f && deleteTime > 0.45f)) {
							renderer.material = whiteMaterial;
						} else {
							renderer.material = blackMaterial;
						}
					}
				}
			}
		}
	}

	// ブロック消去
	void RemoveBlock() {
		int num = 0;
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
						blockMass.blocks[y, x].cube = null;
						blockMass.blocks[y, x].cubePos = new Vector2(0, 0);
						blockMass.blocks[y, x].placePos = new Vector2(0, 0);
					}

					for (int fy = y; fy > blockMass.top; fy--) {
						for(int x = 2; x < 12; x++) {
							blockMass.there [fy, x] = blockMass.there [fy - 1, x];
							blockMass.there [fy - 1, x] = false;
							blockMass.blocks [fy, x] = blockMass.blocks [fy - 1, x];
							blockMass.blocks [fy - 1, x].cube = null;
							blockMass.blocks [fy - 1, x].cubePos = new Vector2(0, 0);
							blockMass.blocks [fy - 1, x].placePos = new Vector2(0, 0);
							if (!blockMass.there [fy, x]) {
								continue;
							}
							float px = blockMass.blocks[fy, x].placePos.x;
							float py = blockMass.blocks[fy, x].placePos.y-1;
							blockMass.blocks [fy, x].placePos.y = py;
							blockMass.blocks [fy, x].cube.transform.position = new Vector3 (px, py, 8);
						}
					}
					blockMass.top++;
					num++;
				}
			}
		}
		if (num > 0) {
			score.AddScore (block.spd, num);
		}

		blackCube = false;
	}

	// ゴースト生成
	void SetGhost() {
		int dy;
		for (dy = 0; dy < 22; dy++) {
			if (CollisionBlocks (block, 0, dy)) {
				break;
			}
		}

		float x = block.pos.x + block.cubes [0].cubePos.x;
		float y = block.pos.y + 3.5f - block.cubes [0].cubePos.y - dy + 1;
		ghost1.transform.position = new Vector3 (x, y, 8);
		Renderer renderer1 = ghost1.GetComponent<Renderer> ();
		renderer1.material = ghostMaterial;

		x = block.pos.x + block.cubes [1].cubePos.x;
		y = block.pos.y + 3.5f - block.cubes [1].cubePos.y - dy + 1;
		ghost2.transform.position = new Vector3 (x, y, 8);
		Renderer renderer2 = ghost2.GetComponent<Renderer> ();
		renderer2.material = ghostMaterial;

		x = block.pos.x + block.cubes [2].cubePos.x;
		y = block.pos.y + 3.5f - block.cubes [2].cubePos.y - dy + 1;
		ghost3.transform.position = new Vector3 (x, y, 8);
		Renderer renderer3 = ghost3.GetComponent<Renderer> ();
		renderer3.material = ghostMaterial;

		x = block.pos.x + block.cubes [3].cubePos.x;
		y = block.pos.y + 3.5f - block.cubes [3].cubePos.y - dy + 1;
		ghost4.transform.position = new Vector3 (x, y, 8);
		Renderer renderer4 = ghost4.GetComponent<Renderer> ();
		renderer4.material = ghostMaterial;
	}

	// 次のブロック
	void SetNextBlock() {
		int[] ix = new int[4];
		int[] iy = new int[4];
		float x = 8;
		float y = 5;
		int index = 0;

		for (int j = 0; j < HEIGHT; j++) {
			for(int i = 0; i < WIDE; i++) {
				if(form [order.First.Value, j, i]) {
					ix[index] = i;
					iy[index] = j;
					index++;
				}
			}
		}

		nextBlock1.transform.position = new Vector3 (x + ix [0], y - iy [0], 8);
		nextBlock2.transform.position = new Vector3 (x + ix [1], y - iy [1], 8);
		nextBlock3.transform.position = new Vector3 (x + ix [2], y - iy [2], 8);
		nextBlock4.transform.position = new Vector3 (x + ix [3], y - iy [3], 8);

		// 色の設定
		SetColor(nextBlock1);
		SetColor(nextBlock2);
		SetColor(nextBlock3);
		SetColor(nextBlock4);
	}

	void BlockMassSetPosition () {
		for (int j = 2; j < 22; j++) {
			for(int i = 2; i < 12; i++) {
				if(blockMass.there[j, i]) {
					float x = blockMass.blocks[j, i].placePos.x;
					float y = blockMass.blocks[j, i].placePos.y;
					blockMass.blocks[j, i].cube.transform.position = new Vector3 (x, y, 8);
				}
			}
		}
	}
}
