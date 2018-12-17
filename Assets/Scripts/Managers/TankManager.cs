using System;
using UnityEngine;

[Serializable]
public class TankManager
{
    // このクラスはタンクの様々な設定を管理するためのものです。
    // このクラスは GameManager クラスと一緒に使い、タンクの挙動と、プレイヤーがゲームのさまざまな段階で
    // タンクを制御するかを支配します。

    public Color m_PlayerColor;                             // タンクの色
    public Transform m_SpawnPoint;                          // タンクが生成されたときのタンクの場所と向き
    [HideInInspector] public int m_PlayerNumber;            // この番号によってどのプレイヤーのマネージャーかを認識します。
    [HideInInspector] public string m_ColoredPlayerText;    // プレイヤーを示す文字列。タンクと一致するように番号は同じ色になっています。
    [HideInInspector] public GameObject m_Instance;         // タンクが作成されるとき、そのインスタンスへの参照
    [HideInInspector] public int m_Wins;                    // このプレイヤーの今までの勝ち数


    private TankMovement m_Movement;                        // タンクの動きのスクリプトへの参照。制御を無効にしたり有効にしたりするために使用。
    private TankShooting m_Shooting;                        // タンクの砲撃スクリプトへの参照。制御を無効にしたり有効にしたりするために使用。
    private GameObject m_CanvasGameObject;                  // 各ラウンドの開始と終了の段階でワールド空間 UI を無効にするために使用


    public void Setup ()
    {
        // コンポーネントへの参照を取得
        m_Movement = m_Instance.GetComponent<TankMovement> ();
        m_Shooting = m_Instance.GetComponent<TankShooting> ();
        m_CanvasGameObject = m_Instance.GetComponentInChildren<Canvas> ().gameObject;

        // スクリプトをまたいで、プレイヤー番号が一定であるように設定します。
        m_Movement.m_PlayerNumber = m_PlayerNumber;
        m_Shooting.m_PlayerNumber = m_PlayerNumber;

        // タンクの色とプレイヤー番号に基づいた適切な色を使って 'PLAYER 1'  などと表示された文字列を作成
        m_ColoredPlayerText = "<color=#" + ColorUtility.ToHtmlStringRGB(m_PlayerColor) + ">PLAYER " + m_PlayerNumber + "</color>";

        // タンクのレンダラーすべてを取得
        MeshRenderer[] renderers = m_Instance.GetComponentsInChildren<MeshRenderer> ();

        // すべてのレンダラーを確認します...
        for (int i = 0; i < renderers.Length; i++)
        {
            // ... マテリアルの色をこのタンク指定の色に設定します
            renderers[i].material.color = m_PlayerColor;
        }
    }


    // プレイヤーがタンクを制御できないゲーム状態に使用。
    public void DisableControl ()
    {
        m_Movement.enabled = false;
        m_Shooting.enabled = false;

        m_CanvasGameObject.SetActive (false);
    }


    // プレイヤーがタンクを制御可能なゲーム状態に使用。
    public void EnableControl ()
    {
        m_Movement.enabled = true;
        m_Shooting.enabled = true;

        m_CanvasGameObject.SetActive (true);
    }


    // 各ラウンド開始時に使い、タンクをデフォルト状態にします。
    public void Reset ()
    {
        m_Instance.transform.position = m_SpawnPoint.position;
        m_Instance.transform.rotation = m_SpawnPoint.rotation;

        m_Instance.SetActive (false);
        m_Instance.SetActive (true);
    }
}
