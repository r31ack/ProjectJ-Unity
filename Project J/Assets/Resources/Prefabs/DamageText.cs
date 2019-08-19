using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SPECIAL_STATE      // 특수 상태
{
    NONE,       // 없음
    CRITICAL,   // 크리티컬
    STUN,       // 스턴
    KNOCK_BACK  // 넉백
}

public class DamageText : MonoBehaviour
{
    private float lifeTime;
    public string damageAmount;
    public Vector3 targetTransform;
    UILabel damageLabel;
    private Camera m_uiCamera;           // UI 카메라

    void Start()
    {
        damageLabel = transform.Find("DamageLabel").GetComponent<UILabel>();
        lifeTime = 1.0f;
        m_uiCamera = GameObject.Find("NGUICamera").GetComponent<Camera>();
        transform.localScale = Vector3.one;
        damageLabel.transform.localScale = Vector3.one;                             // 스케일이 이상해지므로 1로 변경
    }

    // Update is called once per frame
    void Update()
    {
        damageLabel.text = damageAmount;
        targetPositionSync();

        lifeTime -= Time.deltaTime;

        if (lifeTime < 0)
        {
            ObjectPoolManager.Instance.PushToPool("DamageText", this.gameObject);
            lifeTime = 1.0f;
        }
    }

    void targetPositionSync()       // 타겟의 위치를 UI 좌표에 동기화
    {
        Vector3 position = Camera.main.WorldToViewportPoint(targetTransform);    // 아이템의 위치를 메인 카메라의 viewPort좌표로 변환
        damageLabel.transform.position = m_uiCamera.ViewportToWorldPoint(position); // 해당 viewPort좌표를 UI카메라의 world좌표로 변환
        position = damageLabel.transform.localPosition;                             // 포지션 위치 재조립
        position.y = damageLabel.transform.localPosition.y + 250.0f - lifeTime*100;
        position.z = 0.0f;                                                          // Z축을 없애고 재조립
        damageLabel.transform.localPosition = position;                             // 재조립
    }
}