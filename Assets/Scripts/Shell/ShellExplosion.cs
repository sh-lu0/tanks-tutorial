using UnityEngine;

public class ShellExplosion : MonoBehaviour
{
    public LayerMask m_TankMask;                        // 爆発が影響するものをフィルタリングするために使用。ここでは、"プレイヤー" に設定されます。
    public ParticleSystem m_ExplosionParticles;         // 爆発時に再生するパーティクルへの参照
    public AudioSource m_ExplosionAudio;                // 爆発時に再生するオーディオへの参照
    public float m_MaxDamage = 100f;                    // タンクが爆心にある場合に、タンクに与えられるダメージ量
    public float m_ExplosionForce = 1000f;              // タンクが爆心にある場合に、タンクに与えられる力の量
    public float m_MaxLifeTime = 2f;                    // 砲弾が削除されるまでの秒数
    public float m_ExplosionRadius = 5f;                // タンクに影響を及ぼすことが可能な爆発からの最大距離


    private void Start ()
    {
        //これまでに破棄されていない場合は、生存期間が過ぎたら砲弾を破棄します。
        Destroy (gameObject, m_MaxLifeTime);
    }


    private void OnTriggerEnter (Collider other)
    {
        // 砲弾の現在の位置から爆破半径内にあるコライダーすべてを集めます
        Collider[] colliders = Physics.OverlapSphere (transform.position, m_ExplosionRadius, m_TankMask);

        // すべてのコライダーを確認します...
        for (int i = 0; i < colliders.Length; i++)
        {
            // ... そして、リジッドボディを見つけます
            Rigidbody targetRigidbody = colliders[i].GetComponent<Rigidbody> ();

            // リジッドボディがなければ、次のコライダーをチェックします
            if (!targetRigidbody)
                continue;

            // 爆発の力を加えます
            targetRigidbody.AddExplosionForce (m_ExplosionForce, transform.position, m_ExplosionRadius);

            // リジッドボディに関連する TankHealth スクリプトを見つけます
            TankHealth targetHealth = targetRigidbody.GetComponent<TankHealth> ();

            // ゲームオブジェクトにアタッチされた TankHealth スクリプトがなければ、次のコライダーをチェックします
            if (!targetHealth)
                continue;

            // 砲弾からの距離に基づいて、ターゲットが受けるダメージ量を計算
            float damage = CalculateDamage (targetRigidbody.position);

            // このダメージをタンクに適用
            targetHealth.TakeDamage (damage);
        }

        // 砲弾とパーティクルの親子関係を解除
        m_ExplosionParticles.transform.parent = null;

        // パーティクルシステムを再生
        m_ExplosionParticles.Play();

        // 爆発のサウンドエフェクトを再生
        m_ExplosionAudio.Play();

        // パーティクルが終了したら、パーティクルを伴っていたゲームオブジェクトを破棄します
        Destroy (m_ExplosionParticles.gameObject, m_ExplosionParticles.duration);

        // 砲弾を破棄
        Destroy (gameObject);
    }


    private float CalculateDamage (Vector3 targetPosition)
    {
        // 砲弾からターゲットまでのベクトルを作成
        Vector3 explosionToTarget = targetPosition - transform.position;

        // 砲弾からターゲットまでの距離を計算
        float explosionDistance = explosionToTarget.magnitude;

        //最大距離 (爆破半径) に対するターゲットの距離の比率を計算
        float relativeDistance = (m_ExplosionRadius - explosionDistance) / m_ExplosionRadius;

        // ダメージの最大値と距離の比率に基づいて、ダメージを計算
        float damage = relativeDistance * m_MaxDamage;

        // ダメージの最小値は常に 0 です。
        damage = Mathf.Max (0f, damage);

        return damage;
    }
}
