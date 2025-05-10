using Sirenix.OdinInspector.Editor.Validation;
using UnityEngine;
public interface ITauntable
{
    bool IsActive { get; }          // 도발 지속 여부
    Transform TauntPoint { get; }   // 위치
}