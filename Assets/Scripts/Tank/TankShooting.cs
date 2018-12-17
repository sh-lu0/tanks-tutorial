using UnityEngine;
using UnityEngine.UI;

public class TankShooting : MonoBehaviour
{
    public int m_PlayerNumber = 1;              // 異なるプレイヤーを識別するのに使用します。
    public Rigidbody m_Shell;                   // 砲弾のプレハブ
    public Transform m_FireTransform;           // タンクの子。砲弾を生成します。
    public Slider m_AimSlider;                  // タンクの子。現在の発射力を表示します。
    public AudioSource m_ShootingAudio;         // 砲撃オーディオを再生するために使用されるオーディオソースへの参照。動きのオーディオソースとは異なることに注意。
    public AudioClip m_ChargingClip;            // 各射撃前の充填するときに再生するオーディオ
    public AudioClip m_FireClip;                // 各砲撃時に再生するオーディオ
    public float m_MinLaunchForce = 15f;        // 砲撃ボタンが押されない場合の砲撃力
    public float m_MaxLaunchForce = 30f;        // 充填時間いっぱいになるまで射撃ボタンを押しつづけた場合の砲撃力
    public float m_MaxChargeTime = 0.75f;       // 砲撃力が最大になるまで充填するのに必要な時間


    private string m_FireButton;                // 砲弾の発射に使用する入力軸
    private float m_CurrentLaunchForce;         // 発射ボタンを離したときの砲撃力
    private float m_ChargeSpeed;                // 最大の充填時間に基づいた充填速度
    private bool m_Fired;                       // このボタンが押され砲弾が発射されたかどうか


    private void OnEnable()
    {
        // タンクをオンにする時、発射力と UI をリセット
        m_CurrentLaunchForce = m_MinLaunchForce;
        m_AimSlider.value = m_MinLaunchForce;
    }


    private void Start ()
    {
        // 発射ボタンはプレイヤー番号に基づきます。
        m_FireButton = "Fire" + m_PlayerNumber;

        // 発射力の充填スピードは、可能な力の範囲を最大充填時間で割ったものです。
        m_ChargeSpeed = (m_MaxLaunchForce - m_MinLaunchForce) / m_MaxChargeTime;
    }


    private void Update ()
    {
        // スライダーに最小発射力をデフォルト値として設定
        m_AimSlider.value = m_MinLaunchForce;

        // 最大力が増加していて砲弾がまだ発射されていない場合...
        if (m_CurrentLaunchForce >= m_MaxLaunchForce && !m_Fired)
        {
            // ... 最大力を使って砲弾を発射します
            m_CurrentLaunchForce = m_MaxLaunchForce;
            Fire ();
        }
        // そうでない場合は、発射ボタンをちょうど押し始めた場合...
        else if (Input.GetButtonDown (m_FireButton))
        {
            // ...発射フラグと発射力をリセット
            m_Fired = false;
            m_CurrentLaunchForce = m_MinLaunchForce;

            // クリップを充填中のクリップに変えて再生
            m_ShootingAudio.clip = m_ChargingClip;
            m_ShootingAudio.Play ();
        }
        // そうでない場合は、発射ボタンを押していて、かつ、砲弾がまだ発射されていない場合...
        else if (Input.GetButton (m_FireButton) && !m_Fired)
        {
            // 発射力を増加してスライダーを更新
            m_CurrentLaunchForce += m_ChargeSpeed * Time.deltaTime;

            m_AimSlider.value = m_CurrentLaunchForce;
        }
        // そうでない場合は、発射ボタンを離し、かつ、砲弾がまだ発射されていない場合...
        else if (Input.GetButtonUp (m_FireButton) && !m_Fired)
        {
            // ... 砲弾を発射
            Fire ();
        }
    }


    private void Fire ()
    {
        // 発射フラグを設定して、Fire が1 度しか呼び出されないようにします。
        m_Fired = true;

        // 砲弾のインスタンスを作成して、そのリジッドボディへの参照を格納
        Rigidbody shellInstance =
            Instantiate (m_Shell, m_FireTransform.position, m_FireTransform.rotation) as Rigidbody;

        // 射撃位置の前方方向で、砲弾速度を発射力に設定
        shellInstance.velocity = m_CurrentLaunchForce * m_FireTransform.forward; ;

        // 砲撃クリップに変えて再生します。
        m_ShootingAudio.clip = m_FireClip;
        m_ShootingAudio.Play ();

        // 発射力をリセット。これは、ボタンのイベントを取得できない、もしもの時のために設定します。
        m_CurrentLaunchForce = m_MinLaunchForce;
    }
}
