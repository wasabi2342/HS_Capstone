using Sirenix.OdinInspector.Editor.Validation;
using UnityEngine;
public interface ITauntable
{
    bool IsActive { get; }          // ���� ���� ����
    Transform TauntPoint { get; }   // ��ġ
}