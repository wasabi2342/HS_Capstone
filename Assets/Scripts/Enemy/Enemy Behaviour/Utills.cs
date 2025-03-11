using UnityEngine;

public static class Utills
{
    /// <summary>
    /// 원의 둘레 위치를 구하는 메소드
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
    /// Degree 값을 Radian값으로
    /// 1도는 PI/180 라디안
    /// angle도는 angle * PI/180 라디안
    /// </summary>
    /// <param name="angle"></param>
    public static float DegreeToRadian(float angle)
    {
        return Mathf.PI * angle / 180.0f;
    }
}
