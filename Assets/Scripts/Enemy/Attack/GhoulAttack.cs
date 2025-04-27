using UnityEngine;

public class GhoulAttack : MonoBehaviour, IMonsterAttack
{
    public int damage = 10;
    private Transform target;
    public GameObject weaponColliderObject;
    private Collider weaponCollider;
    private Vector3 defaultCenter;

    public void Attack(Transform target)
    {
        this.target = target;
    }

    private void Awake()
    {
        if (weaponColliderObject != null)
        {
            weaponCollider = weaponColliderObject.GetComponent<Collider>();
            if (weaponCollider is BoxCollider box)
                defaultCenter = box.center;
            else if (weaponCollider is SphereCollider sphere)
                defaultCenter = sphere.center;
        }
    }

    // 애니메이션 이벤트용
    public void EnableAttack()
    {
        if (weaponColliderObject != null)
            weaponColliderObject.SetActive(true);
    }

    public void DisableAttack()
    {
        if (weaponColliderObject != null)
            weaponColliderObject.SetActive(false);
    }

    public void SetColliderRight()
    {
        if (weaponCollider == null) return;

        if (weaponCollider is BoxCollider box)
            box.center = new Vector3(Mathf.Abs(defaultCenter.x), defaultCenter.y, defaultCenter.z);
        else if (weaponCollider is SphereCollider sphere)
            sphere.center = new Vector3(Mathf.Abs(defaultCenter.x), defaultCenter.y, defaultCenter.z);
    }

    public void SetColliderLeft()
    {
        if (weaponCollider == null) return;

        if (weaponCollider is BoxCollider box)
            box.center = new Vector3(-Mathf.Abs(defaultCenter.x), defaultCenter.y, defaultCenter.z);
        else if (weaponCollider is SphereCollider sphere)
            sphere.center = new Vector3(-Mathf.Abs(defaultCenter.x), defaultCenter.y, defaultCenter.z);
    }
}
