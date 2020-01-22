using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public sealed partial class CameraScript : MonoBehaviour
{
    #region 上みるカメラ追加ver
#if true
    // 初期化---------------------------------------------
    void Start()
    {
        // カメラの位置を設定
        transform.position = init_pos;

		// カメラの位置をプレイヤーの後ろにする為の方向ベクトル
		direction = -player.transform.forward.normalized;

        init_up_pos = player.transform.position.y;

        //camera_state = SCENE;
        //camera_state = 0;
        scene_camera_state = 0;

        // 演出初期化
        approach_state = 0;
        approach_timer = 0;
        init_zoom_out_spd = zoom_out_spd;
        enemy_hit_flg = false;
        enm_id = 0;
        clear_end = false;
		fall_can_move = true;
		scene = GameObject.FindGameObjectWithTag("GameManager");
        post = GameObject.FindGameObjectWithTag("PostProcess");
        // ブラーはずす
        SetBlur(false);
        // 敵を検索
        obj = GameObject.FindGameObjectsWithTag("Enemy");
        // 敵の数を初めに保存しておかないとLengthをそのまま使ったら
        // 4とか、初期値のままで処理されるからバグ出るかもしらん
        // 敵倒して時とかに保存しといた変数を-1とかしてやったらいけるかも
        fade_timer = 0;
    }

    // 更新-----------------------------------------------
    void Update()
    {
        CameraEnemyApproach();
        //debug();
    }

    // 処理が終わってから呼び出される---------------------
    void FixedUpdate()
    {
        CameraUpdate();
    }

    // カメラまとめ---------------------------------------
    void CameraUpdate()
    {
        switch (camera_state)
        {
            case NONE: CameraNone(); break;            // 演出なし
            case ENM_HIT: EnemyHitCamera(); break;    // 敵に近づく
            case SCENE: SceneCamera(); break;          // シーン開始カメラ
            case CLEAR: ClearCamera(); break;
        }

        // カメラが何を見るかまとめ
        Look();

        // クリア演出にはいる
        if (scene.GetComponent<Scene>().ClearFg())
        {
            camera_state = CLEAR;
        }



	}

	// カメラが何を見るかまとめ---------------------------
	void Look()
    {
        // 最終的に見る方向初期化
        Vector3 look_pos = Vector3.zero;

        // 演出時と通常時の注視点の切り替え
        switch (camera_state)
        {
            // 演出無いとき
            case NONE: look_pos = PlayerTarget(); break;
            case ENM_HIT: look_pos = EnemyTarget(look_pos); break;
            case SCENE: look_pos = WorldTarget(); break;
            case CLEAR: look_pos = ClearTarget(); break;
        }

        // 注視点の方に向く
        LookLerp(transform.position, look_pos, LOOK_SPD);
    }

    // NONE-----------------------------------------------
    #region プレイヤー追従カメラ(通常時)
    void CameraNone()
    {
		// カメラ位置(-60以上は落ちたら追従しない)
		if (fall_can_move) {
			cam_pos = new Vector3(player.transform.position.x, player.transform.position.y + init_up_pos + UP, player.transform.position.z);
		}

		// パッド情報を取得
		pad_rx = -Input.GetAxis("R_Stick_H");
        pad_ry = Input.GetAxis("R_Stick_V");
        pad_lx = Input.GetAxis("L_Stick_H");

        // カメラの位置変更
        RotateX(cam_pos, pad_rx);
        RotateY(cam_pos, pad_ry, pad_rx);

        // 左スティックで入力してる時に条件付でカメラ追従
        if (PadLxCheck(pad_lx)) FollowCamera();

        // 位置が戻ってきたらここに来て初期化
        ClearEnemyHitCamera();
    }

    // 右スティックでカメラ移動
    void RotateX(Vector3 cam_pos, float pad_rx)
    {
        // 正規化に使う平方根
        float x_len = Mathf.Sqrt(pad_rx * pad_rx);

        // 入力軸正規化
        if (x_len > 1.0f) pad_rx /= x_len;

        // アフィン変換
        float s = Mathf.Sin((TURN + TURN) * pad_rx);
        float c = Mathf.Cos((TURN + TURN) * pad_rx);
        float x = direction.x * c - direction.z * s;
        float z = direction.x * s + direction.z * c;
        direction.x = x;
        direction.z = z;

        // 正規化
        direction.Normalize();

        // pad_xで入力したらカメラの位置変更
        transform.position = cam_pos + (direction * DIST);
    }

    void RotateY(Vector3 cam_pos, float pad_ry, float pad_rx)
    {
        // スティックの入力が無いときは0に向かう
        if (pad_ry == 0f && pad_rx == 0)
        {
            direction.y += (0f - direction.y) * (Y_CLEAR_SPD * Time.deltaTime);
        }

        // pad_xで入力したらカメラの位置変更
        if (direction.y > 0.5f) direction.y = 0.5f;
        if (direction.y < -0.5f) direction.y = -0.5f;

        direction.y += pad_ry * TURN;

        // 正規化
        direction.Normalize();

	}

	// カメラの追従
	void FollowCamera()
    {
		StateCheck(player.transform.right.normalized - player.transform.forward.normalized, pad_lx);
    }

    // カメラとプレイヤーの角度によって追従を変更
    void StateCheck(Vector3 vec, float pad_lx)
    {
        switch (follow_state)
        {
            case 0:
                // 内積で角度がありすぎたらカメラ追跡
                if (AngleCheck()) follow_state = 1;
                break;
            case 1:
                // ベクトルを徐々にその方向に持っていく
                //direction = Vector3.Lerp(direction, vec, 1 * Time.deltaTime);
                direction.x += vec.x * (FOLLOW_SPD * Time.deltaTime);
                direction.z += vec.z * (FOLLOW_SPD * Time.deltaTime);

                // 入力をやめたらカメラの追跡をやめる
                if (Mathf.Abs(pad_lx) <= STATE_CHECK) follow_state = 0;
                break;
        }
    }

    // プレイヤーとカメラの角度チェック
    bool AngleCheck()
    {
        // 角度がANGLEとANGLE_MAXの間やったら、trueを返す、それを左右やってる
        if ((int)RotateAngle() > ANGLE && (int)RotateAngle() < ANGLE_MAX) return true;
        if ((int)RotateAngle() < -ANGLE && (int)RotateAngle() > -ANGLE_MAX) return true;

        // 角度が範囲外
        return false;
    }

    // 左スティックのパッド操作をしているか
    bool PadLxCheck(float pad_lx)
    {
        // 入力してるか
        if (pad_lx == 0) return false;
        return true;
    }

    // カメラの位置とプレイヤーの正面で回転角を取得
    float RotateAngle()
    {
        // 方向取得
        Vector3 vec = transform.position - player.transform.position;

        // 外積で横ベクトルをだす
        Vector3 axis = Vector3.Cross(-player.transform.forward, vec);

        // ＋とーの角度計算
        float angle = Vector3.Angle(-player.transform.forward, vec) * (axis.y < 0 ? -1 : 1);

        return angle;
    }

    #endregion

    // ENM_HIT--------------------------------------------
    #region エネミー演出カメラ(近づく)
    // カメラが敵に近づく演出するための処理---------------
    void CameraEnemyApproach()
    {
        // 敵を全て探す
        for (int i = 0; i < obj.Length; i++)
        {
            // そのオブジェクトが存在していたら
            if (obj[i])
            {
                // 敵の当たり判定がtrueになった敵の位置を取得
                if (obj[i].GetComponent<Enemy>().ShotTouchFlg)
                {
                    // 当たった敵の番号を保存
                    enm_id = i;

                    // 位置を保存
                    enm_pos = obj[i].GetComponent<Enemy>().TransformPosition;

                    // 一定値の位置を保存
                    if (adjust_pos.x == 0) adjust_pos = enm_pos;

                    // 演出させる
                    enemy_hit_flg = true;
                }
            }
        }

        // 演出処理する
        if (enemy_hit_flg)
        {
            camera_state = ENM_HIT;
        }
    }

    void EnemyHitCamera()
    {
        // どこまで近づいてどこまで遠ざかる(下がる)処理
        Approach(enm_pos, save_pos, zoom_len, zoom_in_spd, zoom_out_spd);
    }
    private int test_time = 0;
    private float sub;
    private float speed;
    // 近づいて遠ざかる処理
    void Approach(Vector3 near_pos, Vector3 back_pos, float len, float zoom_in_spd, float zoom_out_spd)
    {
        // 初期化
        Vector3 vec = Vector3.zero;

        // 60フレームで距離を割る(距離によって近づいたり遠ざかったりする速度を変える)
        // 60分割したからsubで加算したら60フレーム後にはvec.magnitudeになる
        sub = (adjust_pos - save_pos).magnitude * Time.deltaTime; 

        switch (approach_state)
        {
            case 0:              
                // 送られてきた位置(敵位置)とのベクトル取得
                vec = near_pos - transform.position;

                // 速度設定
                //speed = (zoom_in_spd * Time.deltaTime) * sub;
                speed = sub * zoom_in_spd;

                // 近づける(Lerpなしで)
                //transform.position += vec.normalized * zoom_in_spd * Time.deltaTime;
                transform.position += vec.normalized * speed;
                //transform.position = Vector3.Lerp(transform.position, near_pos, speed);

                // ブラーつける
                SetBlur(true);
                test_time++;
                // 近づいたら次のステート
                if (vec.magnitude < len) approach_state++;
                break;
            case 1:
                // 戻る位置とのベクトル取得
                vec = back_pos - transform.position;

                // 速度設定
                //speed = (zoom_out_spd * Time.deltaTime) * sub;
                speed = sub * zoom_out_spd;

                // 時間経ったら遠ざける
                if (TimerCheckEnemyHitCamera(approach_timer_max))
                {
                    //transform.position += vec.normalized * sub * ((zoom_out_spd) * Time.deltaTime);
                    transform.position += vec.normalized * speed;
                    //transform.position = Vector3.Lerp(transform.position, back_pos, speed);

                    // 遠ざかったらカメラステート変更（approach_stateを初期化するのはNONEで）
                    if (vec.magnitude < speed)
                    {
						// 近づいた敵の判定を初期化する
						if (obj[enm_id]) obj[enm_id].GetComponent<Enemy>().ShotTouchFlg = false;

                        // ブラーはずす
                        SetBlur(false);

                        // 演出用の判定を初期化
                        enemy_hit_flg = false;
                        adjust_pos = Vector3.zero;

                        // カメラ状態をプレイヤー追従に
                        camera_state = NONE;
                    }
                }
                break;
        }
    }

    // ブラーをonにしたり、offにしたり
    void SetBlur(bool on)
    {
        post.SetActive(on);
    }

    void debug()
    {
        if (Input.GetButtonDown("Shot_L")) post.SetActive(true);
        if (Input.GetButtonDown("Shot_R")) post.SetActive(false);
    }

    // 1回演出が終わったら初期化する
    void ClearEnemyHitCamera()
    {
        // ここで位置を保存してここの位置に戻らせる
        save_pos = transform.position;

        // ステートの初期化
        approach_state = 0;

        // 遠ざける速度の初期化
        zoom_out_spd = init_zoom_out_spd;

        approach_timer = 0;
    }

    #endregion

    // SCENE----------------------------------------------
    #region シーン始まったときのカメラ
    // シーンを見下ろすカメラ 改造
    void SceneCamera()
    {
        switch(select_scene_camera)
        {
            case 0:
                BeforeSceneCamera();
                break;
            case 1:
                AfterSceneCamera();
                break;
        }

        // スキップ
        if (Input.GetButtonDown("Jump") || (Input.GetMouseButtonDown(2)))
        {
            scene_camera_state = 3;
            scene_pos_no = 0;
            camera_state = NONE;
        }
    }

    void BeforeSceneCamera()
    {
        Vector3[] scene_camera_pos = { init_pos, new Vector3(init_pos.x, init_pos.y, init_pos.z * -1) };

        float len = (scene_camera_pos[scene_pos_no] - transform.position).magnitude;

        if (len < SCENE_LEN) scene_pos_no++;
        if (scene_pos_no >= SCENE_TARGET_MAX)
        {
            scene_pos_no = 0;
            camera_state = NONE;
        }

        Vector3 pos = scene_camera_pos[scene_pos_no] - transform.position;

        // 移動方法
        transform.position += pos.normalized * (scene_move_spd * Time.deltaTime);
    }

    // After
    void AfterSceneCamera()
    {
        /*
         * 空のオブジェクトを配置、その位置まで進む、それを
         * マップ全体見えるような感じで数回やる
         */

        // 目的地の場所
        Vector3 target_pos = target[scene_pos_no].transform.position;

        switch (scene_camera_state)
        {
            case 0:
                // 目的地の横にカメラを配置
                transform.position = target_pos + target[scene_pos_no].transform.right * pos_length[scene_pos_no];

                // どこを注視するか
                SetSceneLookPos(target_pos);

                scene_camera_state++;
                break;
            case 1:
                // 目的地まで移動
                float len = (target_pos - transform.position).magnitude;

                // 移動方法
                Vector3 pos = target_pos - transform.position;
                transform.position += pos.normalized * (scene_move_spd * Time.deltaTime);

                if (len < SCENE_LEN)
                {
                    // 目的地を変更
                    scene_pos_no++;

                    // フェードに送る情報
                    scene_camera_state++;
                }
                break;
            case 2:
                fade_timer += Time.deltaTime;
                // fadeやってる時間待機
                if(fade_timer > fade_timer_max)
                {
                    // 初期化
                    fade_timer = 0;
                    // また目的地の横にカメラ配置
                    scene_camera_state = 0;
                }
                break;
        }

        // 目的地最大になったらいつものゲームスタートに
        if (scene_pos_no >= SCENE_TARGET_MAX)
        {
            scene_camera_state = 3;
            scene_pos_no = 0;
            camera_state = NONE;
        }

    }

    void SetSceneLookPos(Vector3 pos)
    {
        float y = pos.y;
        Vector3 v1 = (Vector3.zero - pos);
        float l1 = v1.magnitude * 0.3f;
        v1.y = y;
        v1.Normalize();
        
        scene_look_pos = pos + (v1 + target[scene_pos_no].transform.forward * l1)/* 中心方向にちょっと伸ばした位置 */;
    }
        

    #endregion

    // CLEAR----------------------------------------------
    #region クリア演出
    // 敵を全滅させたらここに来る
    void ClearCamera()
    {
        // プレイヤーの前右斜め下(に向かっていく)
        Vector3 pos = player.transform.position + (player.transform.forward.normalized * 4) + (player.transform.right.normalized * 2) - (player.transform.up * 0.5f);
        transform.position = Vector3.Lerp(transform.position, pos, clear_cam_spd * Time.deltaTime);

        clear_wait_timer += Time.deltaTime;
        // 待機時間後にシーン移行
        if (clear_wait_timer > clear_wait_timer_max)
        {
            // 処理が全部終わったら
            clear_end = true;
            clear_wait_timer = 0;
        }
    }
    #endregion

    // カメラ関連カメラ-----------------------------------
    #region カメラ便利関数
    // 徐々に注視点の方を向くようにする
    void LookLerp(Vector3 cam_pos, Vector3 target, float spd)
    {
        // 注視点 - カメラ位置で注視点の方向を取得
        Vector3 vec = target - cam_pos;

        // 最終的に見る方向を取得
        Vector3 target_pos = cam_pos + vec.normalized;

        // 徐々に方向を変える
        Vector3 target_look = Vector3.Lerp(cam_pos + transform.forward.normalized, target_pos, spd * Time.deltaTime);

        // ここに設定したら角度が変わる
        TargetSet(target_look);
    }

    // セットしたほうを向く
    void TargetSet(Vector3 vec) { transform.LookAt(vec); }

    // 時間経過の判定
    bool TimerCheckEnemyHitCamera(float timer_max)
    {
        approach_timer += Time.deltaTime;
        if (approach_timer > timer_max) return true;
        return false;
    }

    // 注視点設定
    Vector3 PlayerTarget() { return new Vector3(player.transform.position.x, player.transform.position.y + UP_TARGET, player.transform.position.z); }

    Vector3 EnemyTarget(Vector3 look_pos) { return Vector3.Lerp(transform.position + look_pos, enm_pos, LOOK_SPD * Time.deltaTime); }

    Vector3 WorldTarget() { return scene_look_pos; }

    Vector3 ClearTarget() { return player.transform.position; }

    #endregion

    // クリア演出終了
    public bool ClearEnd()
    {
        return clear_end;
    }

    public int Scene_camera_state
    {
        get { return scene_camera_state; }
    }//camera_state

    public int Camera_state
    {
        get { return camera_state; }
    }//camera_state

    //GUI表示 -----------------------------------------------------
    private Vector2 left_scroll_pos = Vector2.zero;   //uGUIスクロールビュー用
    private float scroll_height = 330;
    void OnGUI()
    {
        if (gui.on)
        {
			//スクロール高さを変更
			//(出来ればmaximize on playがonならに変更したい)
			if (gui.all_view) {
				scroll_height = 700;
			}
			else scroll_height = 330;

			GUILayout.BeginVertical("box", GUILayout.Width(190));
			left_scroll_pos = GUILayout.BeginScrollView(left_scroll_pos, GUILayout.Width(180), GUILayout.Height(scroll_height));
			GUILayout.Box("Camera");


            #region ここに追加
            GUILayout.TextArea("sub\n" + sub);
            GUILayout.TextArea("speed\n" + speed);
            GUILayout.TextArea("adjust_pos\n" + adjust_pos);
            GUILayout.TextArea("test_time\n" + test_time);     
            //GUILayout.TextArea("pos\n" + pos);
            //GUILayout.TextArea("pos\n" + pos);
            //GUILayout.TextArea("pos\n" + pos);     
            //GUILayout.TextArea("pos\n" + pos);
            //GUILayout.TextArea("pos\n" + pos);
            //GUILayout.TextArea("pos\n" + pos);     

            // スペース
            GUILayout.Space(200);
            GUILayout.Space(10);
            #endregion

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }
    }

	//ギズモ表示 ------------------------------------------
	void OnDrawGizmos() {
		#region ※GUIの判定
		//※GUIの処理(ランタイム以外でも判定したいのでここに記述)
		if (!gui.on) {
			gui.all_view = false;
			gui.debug_view = false;
		}
		#endregion


	}



	public bool FallCanMove {
		get { return fall_can_move; }
		set { fall_can_move = value; }
	}

#endif
	#endregion

}
