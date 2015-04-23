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

	Block block;			// 落下してくるブロック
	Blockmass blockMass;	// ブロックの塊
	public GameObject gost1;
	public GameObject gost2;
	public GameObject gost3;
	public GameObject gost4;
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

	bool blackCube;			// 一列そろった時の黒テトリミノの有無

	// Use this for initialization
	void Start () {
		FormInit ();
		OederDecision ();

		spd = 1.0f;
		GeneratTetrimino ();

		description = true;

		moveSpd = 0.1f;
		moveSpdTime = 0;

		blackCube = false;
	}
	
	// Update is called once per frame
	void Update () {
		if (description) {
			if(Input.GetKey(KeyCode.Return)) {
				description = false;
				texture.transform.position = new Vector3(100, 0, 0);
				spdTime = Time.time;
				moveSpdTime = Time.time;
			}
			return;
		}

		if (!generat && Input.GetKey (KeyCode.DownArrow)) {
			block.spd = 0.01f;
		}

		if((Time.time - spdTime) >= block.spd) {
			FallTetrimino ();
			spdTime = Time.time;
		}

		BlockMove ();
		LeftRotateBulock ();
		RightRotateBulock ();

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

		SetPosCube ();
		if(!blackCube)
			SetGost ();
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
		SetGost ();
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
			renderer.material.color = new Color (0.0f, 0.8f, 1.0f);
			break;

			// 黄
		case 1:
			renderer.material.color = new Color (0.8f, 0.8f, 0.0f);
			break;

			// 黄緑
		case 2:
			renderer.material.color = new Color (0.0f, 0.8f, 0.0f);
			break;

			// 赤
		case 3:
			renderer.material.color = new Color (0.8f, 0.0f, 0.0f);
			break;

			// 青
		case 4:
			renderer.material.color = new Color (0.0f, 0.2f, 0.8f);
			break;

			// オレンジ
		case 5:
			renderer.material.color = new Color (1.0f, 0.5f, 0.0f);
			break;

			// 紫
		case 6:
			renderer.material.color = new Color (0.5f, 0.0f, 0.5f);
			break;
		default:
			break;
		}
	}

	// ゴーストの色を半透過して設定
	void SetGhostColor(GameObject ghostCube)
	{
		Renderer renderer = ghostCube.GetComponent<Renderer> ();
		switch(block.tetrimino)
		{
			// 水色
		case I_TETRIMINO:
			renderer.material.color = new Color (0.0f, 0.8f, 1.0f, 0.5f);
			break;

			// 黄
		case O_TETRIMINO:
			renderer.material.color = new Color (0.8f, 0.8f, 0.0f, 0.5f);
			break;

			// 黄緑
		case S_TETRIMINO:
			renderer.material.color = new Color (0.0f, 0.8f, 0.0f, 0.5f);
			break;

			// 赤
		case Z_TETRIMINO:
			renderer.material.color = new Color (0.8f, 0.0f, 0.0f, 0.5f);
			break;

			// 青
		case J_TETRIMINO:
			renderer.material.color = new Color (0.0f, 0.2f, 0.8f, 0.5f);
			break;

			// オレンジ
		case L_TETRIMINO:
			renderer.material.color = new Color (1.0f, 0.5f, 0.0f, 0.5f);
			break;

			// 紫
		case T_TETRIMINO:
			renderer.material.color = new Color (0.5f, 0.0f, 0.5f, 0.5f);
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
			((Time.time - moveSpdTime) >= moveSpd))
		{
			if (!CollisionBlocks (block, 1, 0)) {
				block.pos.x += 1.0f;
				blockMass.blockPos.x += 1;
			}
			moveSpdTime = Time.time;
		}
		if(	Input.GetKey(KeyCode.LeftArrow) &&
		   ((Time.time - moveSpdTime) >= moveSpd))
		{
			if (!CollisionBlocks (block, -1, 0)) {
				block.pos.x -= 1.0f;
				blockMass.blockPos.x -= 1;
			}
			moveSpdTime = Time.time;
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
	bool CollisionKickWallBlocks(Block cBlock, int mx, int my)
	{
		int x;
		int y;
		for (int i = 0; i < 4; i++) {
			x = (int)(cBlock.cubes[i].cubePos.x + blockMass.blockPos.x + mx);
			y = (int)(cBlock.cubes[i].cubePos.y + blockMass.blockPos.y + my);
			if (y < 1 || y > 21) {
				return true;
			}
			if (blockMass.there [y, x]) {
				return true;
			}
		}
		
		return false;
	}

	// ブロックを壁際で回転をさせる
	Block KickWall(Block b)
	{
		int x;
		for (int i = 0; i < 4; i++) {
			x = (int)(b.cubes[i].cubePos.x + blockMass.blockPos.x);

			if (x <= 2) 
			{
				for(int j=0;j<4;j++)
				{
					if(b.tetrimino != I_TETRIMINO)
					{
						b.cubes[j].cubePos.x+=1;
					}
					else if(b.form[2,0]==true)
					{
						b.cubes[j].cubePos.x+=2;
					}
					else if(b.form[2,0]==false)
					{
						b.cubes[j].cubePos.x+=1;
					}
				}
				break;
			}
			else if(x == 11)
			{
				for(int j=0;j<4;j++)
				{
					if(b.tetrimino != I_TETRIMINO)
						b.cubes[j].cubePos.x-=1;
					else if(b.form[2,0]==false)
					{
						b.cubes[j].cubePos.x-=2;
					}
					else if(b.form[2,0]==true)
					{
						b.cubes[j].cubePos.x-=1;
					}
				}
				break;
			}
		}
		
		return b;
	}

	// ブロックを左回転させる
	void LeftRotateBulock() {
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
				if(block.tetrimino == I_TETRIMINO && blockMass.blockPos.y == 0)
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
				}
		
				if (!CollisionBlocks (after, 0, 0)) {
					block = after;
				}
				else if(!CollisionKickWallBlocks(after, 0, 0))
				{
					block = KickWall(after);
				}
			}
		}
	}

	// ブロックを右回転させる
	void RightRotateBulock() {
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
				if(block.tetrimino == I_TETRIMINO && blockMass.blockPos.y == 0)
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
				}
				
				if (!CollisionBlocks (after, 0, 0)) {
					block = after;
				}
				else if(!CollisionKickWallBlocks(after, 0, 0))
				{
					block = KickWall(after);
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
				blockMass.blocks [y, x] = block.cubes [i];
			}
			if(blackCube)
				RemoveBlock ();

			ChangeColorRemoveBlock();

			if(!blackCube)
				GeneratTetrimino ();
		}
	}


	// 消えるブロックを黒くする
	void ChangeColorRemoveBlock()
	{
		for (int i = 0; i < 4; i++) {
			int y = (int)(block.cubes[i].cubePos.y + blockMass.blockPos.y);
			for (int j = 2; j < 12; j++) {
				if (!blockMass.there [y, j]) {
					break;
				}
				if (j == 11) {
					Renderer renderer;
					for (int x = 2; x < 12; x++) {
						renderer = blockMass.blocks[y, x].cube.GetComponent<Renderer> ();
						renderer.material.color = new Color (0.0f, 0.0f, 0.0f);
					}
					blackCube =true;

					// ゴーストを透明にする
					renderer = gost1.GetComponent<Renderer> ();
					renderer.material.color = new Color (0.0f, 0.0f, 0.0f, 0.0f);
					renderer = gost2.GetComponent<Renderer> ();
					renderer.material.color = new Color (0.0f, 0.0f, 0.0f, 0.0f);
					renderer = gost3.GetComponent<Renderer> ();
					renderer.material.color = new Color (0.0f, 0.0f, 0.0f, 0.0f);
					renderer = gost4.GetComponent<Renderer> ();
					renderer.material.color = new Color (0.0f, 0.0f, 0.0f, 0.0f);
				}
			}
		}
	}

	// ブロック消去
	void RemoveBlock() {
		System.Threading.Thread.Sleep (400);
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
	void SetGost() {
		int dy;
		for (dy = 0; dy < 22; dy++) {
			if (CollisionBlocks (block, 0, dy)) {
				break;
			}
		}

		float x = block.pos.x + block.cubes [0].cubePos.x;
		float y = block.pos.y + 3.5f - block.cubes [0].cubePos.y - dy + 1;
		gost1.transform.position = new Vector3 (x, y, 8);
		SetGhostColor (gost1);

		x = block.pos.x + block.cubes [1].cubePos.x;
		y = block.pos.y + 3.5f - block.cubes [1].cubePos.y - dy + 1;
		gost2.transform.position = new Vector3 (x, y, 8);
		SetGhostColor (gost2);

		x = block.pos.x + block.cubes [2].cubePos.x;
		y = block.pos.y + 3.5f - block.cubes [2].cubePos.y - dy + 1;
		gost3.transform.position = new Vector3 (x, y, 8);
		SetGhostColor (gost3);

		x = block.pos.x + block.cubes [3].cubePos.x;
		y = block.pos.y + 3.5f - block.cubes [3].cubePos.y - dy + 1;
		gost4.transform.position = new Vector3 (x, y, 8);
		SetGhostColor (gost4);
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
}
