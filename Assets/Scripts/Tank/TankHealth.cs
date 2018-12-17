using UnityEngine;
using UnityEngine.UI;

public class TankHealth : MonoBehaviour
{
    public float m_StartingHealth = 100f;               // 開始時の各タンクの体力の値
    public Slider m_Slider;                             // 現在のタンクの体力を示すスライダー
    public Image m_FillImage;                           // スライダーの Image コンポーネント
    public Color m_FullHealthColor = Color.green;       // 体力が満タンのときの体力バーの色
    public Color m_ZeroHealthColor = Color.red;         // 体力が 0 になったときの体力バーの色
    public GameObject m_ExplosionPrefab;                // Awake でインスタンスにされ、その後、タンクが倒されると常に使用されるプレハブ


    private AudioSource m_ExplosionAudio;               // タンクが爆発したときに再生するオーディオ
    private ParticleSystem m_ExplosionParticles;        // タンクが破壊されたときに再生するパーティクルシステム
    private float m_CurrentHealth;                      // 現在のタンクの体力値
    private bool m_Dead;                                // タンクの体力値が 0 を下回ったかどうか


    private void Awake ()
    {
        //爆発のプレハブをインスタンスにして、そのパーティクルシステムへの参照を取得
        m_ExplosionParticles = Instantiate (m_ExplosionPrefab).GetComponent<ParticleSystem> ();

        // インスタンスにしたプレハブのオーディオソースへの参照を取得
        m_ExplosionAudio = m_ExplosionParticles.GetComponent<AudioSource> ();

        // プレハブを無効にし、必要な時に有効にできるようにします。
        m_ExplosionParticles.gameObject.SetActive (false);
    }


    private void OnEnable()
    {
        // タンクが有効にされるとき、タンクの体力をリセットし、倒されていない状態にリセットします。
        m_CurrentHealth = m_StartingHealth;
        m_Dead = false;

        // 体力スライダーの値と色を更新
        SetHealthUI();
    }


    public void TakeDamage (float amount)
    {
        // 受けたダメージに基づいて現在の体力を削減
        m_CurrentHealth -= amount;

        // 適切な UI 要素に変更
        SetHealthUI ();

        // 現在の体力が 0 を下回り、かつ、まだ登録されていなければ、 OnDeath を呼び出します。
        if (m_CurrentHealth <= 0f && !m_Dead)
        {
            OnDeath ();
        }
    }


    private void SetHealthUI ()
    {
        // スライダーに適切な値を設定
        m_Slider.value = m_CurrentHealth;

        // 開始時に対する現在の体力のパーセントに基づいて、選択した色でバーを満たします。
        m_FillImage.color = Color.Lerp (m_ZeroHealthColor, m_FullHealthColor, m_CurrentHealth / m_StartingHealth);
    }


    private void OnDeath ()
    {
        // フラグを設定して、この関数が1 度しか呼び出されないようにします。
        m_Dead = true;

        // インスタンスにした爆発のプレハブをタンクの位置に移動し、有効にします。
        m_ExplosionParticles.transform.position = transform.position;
        m_ExplosionParticles.gameObject.SetActive (true);

        // タンクの爆発のパーティクルシステムを再生します。
        m_ExplosionParticles.Play ();

        // タンクの爆発のサウンドエフェクトを再生します。
        m_ExplosionAudio.Play();

        // タンクをオフにします。
        gameObject.SetActive (false);
    }
}
