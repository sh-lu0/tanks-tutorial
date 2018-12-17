using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public int m_NumRoundsToWin = 5;            // 1 人のプレイヤーがゲームを勝利するために、勝たなくてはならないラウンド数。
    public float m_StartDelay = 3f;             // RoundStarting の開始と RoundPlaying 段階の間の遅延
    public float m_EndDelay = 3f;               // RoundPlaying の終わりと RoundEnding 段階の間の遅延
    public CameraControl m_CameraControl;       // 異なる段階間を制御するための CameraControl スクリプトへの参照
    public Text m_MessageText;                  // 勝利成績などを表示するテキストへの参照
    public GameObject m_TankPrefab;             // プレイヤーが制御するプレハブへの参照
    public TankManager[] m_Tanks;               // 異なるタンクの様相を有効/無効にするマネージャーの集合。


    private int m_RoundNumber;                  // 現在、ゲームがどのラウンドか
    private WaitForSeconds m_StartWait;         // ラウンドの開始を遅延させる
    private WaitForSeconds m_EndWait;           // ラウンド/ゲームの終了を遅延させる
    private TankManager m_RoundWinner;          // 現在のラウンドの勝者への参照。誰が勝利したかの発表に使用します。
    private TankManager m_GameWinner;           // ゲームの勝者への参照。誰が勝利したかの発表に使用します。


    private void Start()
    {
        // 遅延を作成。1 度作れば、繰り返し作る必要がありません。
        m_StartWait = new WaitForSeconds (m_StartDelay);
        m_EndWait = new WaitForSeconds (m_EndDelay);

        SpawnAllTanks();
        SetCameraTargets();

        // タンクを作ってカメラがそれをターゲットにしたら、ゲームを開始します。
        StartCoroutine (GameLoop ());
    }


    private void SpawnAllTanks()
    {
        // すべてのタンクに必要...
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            // ... タンクを作って、制御に必要なプレイヤー番号と参照を設定します。
            m_Tanks[i].m_Instance =
                Instantiate(m_TankPrefab, m_Tanks[i].m_SpawnPoint.position, m_Tanks[i].m_SpawnPoint.rotation) as GameObject;
            m_Tanks[i].m_PlayerNumber = i + 1;
            m_Tanks[i].Setup();
        }
    }


    private void SetCameraTargets()
    {
        //Transform の集合を作成。集合は、タンクの数と同じサイズ。
        Transform[] targets = new Transform[m_Tanks.Length];

        // 各 Transform に対し...
        for (int i = 0; i < targets.Length; i++)
        {
            // ... それぞれ適切なタンクの transform を設定
            targets[i] = m_Tanks[i].m_Instance.transform;
        }

        // カメラが追うターゲットを設定
        m_CameraControl.m_Targets = targets;
    }


    // これは最初に呼び出され、ゲームの各段階で順に実行されます
    private IEnumerator GameLoop ()
    {
        // 'RoundStarting' コルーチンを実行することによって開始しますが、それが終了するまで戻りません
        yield return StartCoroutine (RoundStarting ());

        //  'RoundStarting' コルーチンが終わったら, 'RoundPlaying' コルーチンを実行しますが、'RoundPlaying' が終了するまで戻りません
        yield return StartCoroutine (RoundPlaying());

        // 実行が終わったら、 'RoundEnding' コルーチンを実行しますが、それが終了するまで戻りません
        yield return StartCoroutine (RoundEnding());

        // このコードは 'RoundEnding' が終了するまで実行されません。終了時点で、ゲーム勝者がいるかをチェックします。
        if (m_GameWinner != null)
        {
            // ゲーム勝者がいる場合は、そのレベルをリスタートします。
            Application.LoadLevel (Application.loadedLevel);
        }
        else
        {
            // ゲーム勝者がいない場合は、このコルーチンをリスタートし、ループを継続けます。
            // このコルーチンは待機しないことに注意してください。つまり、現在行っている GameLoop は終了します。
            StartCoroutine (GameLoop ());
        }
    }


    private IEnumerator RoundStarting ()
    {
        // ラウンドが開始するとすぐに、タンクをリセットし動かないようにします。
        ResetAllTanks ();
        DisableTankControl ();

        //カメラのズームと位置を、リセットしたタンクを適切に捕らえるように瞬時に変更します。
        m_CameraControl.SetStartPositionAndSize ();

        // ラウンド数を増加し、プレイヤーにラウンド数を表示します。
        m_RoundNumber++;
        m_MessageText.text = "ROUND " + m_RoundNumber;

        //ゲームのループに戻るまで、指定した時間を待機します。
        yield return m_StartWait;
    }


    private IEnumerator RoundPlaying ()
    {
        // ラウンドが開始するとすぐにプレイヤーがタンクを制御します。
        EnableTankControl ();

        // 画面からテキストを消去
        m_MessageText.text = string.Empty;

        // タンクが 1 台だけでないとき...
        while (!OneTankLeft())
        {
            // ... 次のフレームで戻ります
            yield return null;
        }
    }


    private IEnumerator RoundEnding ()
    {
        // タンクを停止します
        DisableTankControl ();

        // 前のラウンドの勝者を消去
        m_RoundWinner = null;

        // ラウンドが終了した時点で勝者がいるかを確認
        m_RoundWinner = GetRoundWinner ();

        // 勝者がいる場合は、スコアを増加
        if (m_RoundWinner != null)
            m_RoundWinner.m_Wins++;

        // 勝者のスコアが増加された今、ゲームの勝者になったプレイヤーがいるかを確認
        m_GameWinner = GetGameWinner ();

        // スコアに基づいてゲーム勝者がいるかとうかのメッセージを取得し、それを表示します。
        string message = EndMessage ();
        m_MessageText.text = message;

        // ゲームのループに制御が戻るまで、指定した時間を待機します。
        yield return m_EndWait;
    }


    // これを使って、タンクが1 台以下の場合はラウンドを終わらせます。
    private bool OneTankLeft()
    {
        //タンク数を 0 にして数え始めます。
        int numTanksLeft = 0;

        // すべてのタンクを確認します...
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            // ... タンクがアクティブであるときは、カウンターに増加します。
            if (m_Tanks[i].m_Instance.activeSelf)
                numTanksLeft++;
        }

        //タンクが1 台以下の場合は true を返し、それ以外は false を返します。
        return numTanksLeft <= 1;
    }


    // この関数でラウンドの勝者がいるかを確認します。
    // この関数は、1 台以下のタンクが現在アクティブであるという仮定で呼び出されます。
    private TankManager GetRoundWinner()
    {
        // すべてのタンクをチェック...
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            // ... アクティブなタンクが 1 台あれば、それが勝者なのでそれを返します。
            if (m_Tanks[i].m_Instance.activeSelf)
                return m_Tanks[i];
        }

        // アクティブなタンクがなければ引き分けなので、null を返します。
        return null;
    }


    // この関数によって、ゲームの勝者があるかを確認します。
    private TankManager GetGameWinner()
    {
        // すべてのタンクを確認します...
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            // ...タンクのうち 1 台がゲームの勝者になるのに十分な勝ちラウンド数に達している場合は、そのタンクを返します。
            if (m_Tanks[i].m_Wins == m_NumRoundsToWin)
                return m_Tanks[i];
        }

        // 十分な勝ちラウンド数に達しているタンクがない場合は、null を返します。
        return null;
    }


    // 各ラウンドの最後に表示するメッセージの文字列を返します。
    private string EndMessage()
    {
        // デフォルトで、ラウンドが終了するとき、勝者がいないときはデフォルトの終了メッセージが表示されます。
        string message = "DRAW!";

        //勝者がいる場合は、それを示す表示をします。
        if (m_RoundWinner != null)
            message = m_RoundWinner.m_ColoredPlayerText + " WINS THE ROUND!";

        // 最初のメッセージの後に、数行改行を入れます。
        message += "\n\n\n\n";

        // すべてのタンクを確認し、メッセージにそれらのスコアを加えます。
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            message += m_Tanks[i].m_ColoredPlayerText + ": " + m_Tanks[i].m_Wins + " WINS\n";
        }

        //ゲーム勝者がいる場合、それを表すようにメッセージ全体を変更します。
        if (m_GameWinner != null)
            message = m_GameWinner.m_ColoredPlayerText + " WINS THE GAME!";

        return message;
    }


    // この関数は、すべてのタンクをアクティブに戻し、位置とプロパティーをリセットします。
    private void ResetAllTanks()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            m_Tanks[i].Reset();
        }
    }


    private void EnableTankControl()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            m_Tanks[i].EnableControl();
        }
    }


    private void DisableTankControl()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            m_Tanks[i].DisableControl();
        }
    }
}
