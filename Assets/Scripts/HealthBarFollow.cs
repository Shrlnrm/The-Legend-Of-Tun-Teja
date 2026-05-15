using UnityEngine;

public class HealthBarFollow : MonoBehaviour
{
    [Header("Target to Follow")]
    public Transform target;
    public Vector3 offset = new Vector3(0, 1.5f, 0);

    [Header("Settings")]
    public bool keepScalePositive = true;

    void LateUpdate()
    {
        if (target != null)
        {
            transform.position = target.position + offset;

            transform.rotation = Quaternion.identity;

            if (keepScalePositive)
            {
                Vector3 s = transform.localScale;
                s.x = Mathf.Abs(s.x);
                transform.localScale = s;
            }
        }
        else
        {}
    }
}