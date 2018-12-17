using UnityEngine;

public class UIDirectionControl : MonoBehaviour
{
    // 体力バーなどのワールド空間の UI 要素が確実に
    //正しい方向を向くように、このクラスを使います。

    public bool m_UseRelativeRotation = true;       // このゲームオブジェクトに相対的回転を使うかどうか。


    private Quaternion m_RelativeRotation;          // シーンの開始時のローカルの回転


    private void Start ()
    {
        m_RelativeRotation = transform.parent.localRotation;
    }


    private void Update ()
    {
        if (m_UseRelativeRotation)
            transform.rotation = m_RelativeRotation;
    }
}
