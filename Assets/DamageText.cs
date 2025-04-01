using UnityEngine;

public class DamageText : MonoBehaviour
{
    public float floatSpeed = 1f;     // ���� �������� �ӵ�
    public float lifeTime = 1.0f;     // �ؽ�Ʈ ���� �ð�

    private TextMesh textMesh;        // TextMesh ������Ʈ (�Ǵ� TextMeshPro)

    void Awake()
    {
        // �ڱ� �ڽſ��� �ִ� TextMesh�� ã�Ƽ� ����
        textMesh = GetComponent<TextMesh>();
    }

    // ������ �� ���� �ð� ���� ���� �������ٰ� �����
    void Update()
    {
        transform.Translate(Vector3.up * floatSpeed * Time.deltaTime);
        lifeTime -= Time.deltaTime;
        if (lifeTime <= 0f)
        {
            Destroy(gameObject);
        }
    }

    // �ܺο��� ������ ���� �־� �ؽ�Ʈ�� �����ϴ� �޼���
    public void SetDamage(float damage)
    {
        if (textMesh != null)
        {
            textMesh.text = damage.ToString();
        }
    }
}
