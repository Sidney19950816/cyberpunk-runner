using UnityEngine;

public class WeaponLaser : MonoBehaviour
{
    [SerializeField] private float laserMaxLength = 10f;

    private void Update()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, laserMaxLength))
        {
            float distance = Vector3.Distance(transform.position, hit.point);
            SetLaserLength(distance);
        }
        else
        {
            SetLaserLength(laserMaxLength);
        }
    }

    private void SetLaserLength(float length)
    {
        Vector3 scale = transform.localScale;
        scale.z = length;
        transform.localScale = scale;
    }
}
