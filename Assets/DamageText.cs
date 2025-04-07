using UnityEngine;

public class DamageText : MonoBehaviour
{
    public float floatSpeed = 1f;     // 위로 떠오르는 속도
    public float lifeTime = 1.0f;     // 텍스트 유지 시간

    private TextMesh textMesh;        // TextMesh 컴포넌트 (또는 TextMeshPro)

    void Awake()
    {
        // 자기 자신에게 있는 TextMesh를 찾아서 참조
        textMesh = GetComponent<TextMesh>();
    }

    // 생성된 후 일정 시간 동안 위로 떠오르다가 사라짐
    void Update()
    {
        transform.Translate(Vector3.up * floatSpeed * Time.deltaTime);
        lifeTime -= Time.deltaTime;
        if (lifeTime <= 0f)
        {
            Destroy(gameObject);
        }
    }

    // 외부에서 데미지 값을 넣어 텍스트를 갱신하는 메서드
    public void SetDamage(float damage)
    {
        if (textMesh != null)
        {
            textMesh.text = damage.ToString();
        }
    }
}
