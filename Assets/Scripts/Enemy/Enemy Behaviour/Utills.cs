using UnityEngine;

public static class Utills
{
    /// <summary>
    /// ���� �ѷ� ��ġ�� ���ϴ� �޼ҵ�
    /// </summary>
    /// <param name="radius"></param>
    /// <param name="angle"></param>
    /// <returns></returns>
    public static Vector3 GetPositionFromAngle(float radius, float angle)
    {
        Vector3 position = Vector3.zero;

        angle = DegreeToRadian(angle);
        position.x = radius * Mathf.Cos(angle);
        position.z = radius * Mathf.Sin(angle);
        return position;
    }
    /// <summary>
    /// Degree ���� Radian������
    /// 1���� PI/180 ����
    /// angle���� angle * PI/180 ����
    /// </summary>
    /// <param name="angle"></param>
    public static float DegreeToRadian(float angle)
    {
        return Mathf.PI * angle / 180.0f;
    }
}
