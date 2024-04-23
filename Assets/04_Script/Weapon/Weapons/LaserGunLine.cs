using UnityEngine;

public class LaserGunLine : MonoBehaviour
{
    [SerializeField] GameObject _impact;

    [SerializeField] LineRenderer _lineRenderer;
    [SerializeField] EdgeCollider2D _edgeCollider;

    public LineRenderer LineRenderer => _lineRenderer;
    public EdgeCollider2D EdgeCollider => _edgeCollider;

    private float damage;

    public void SetLine(Vector2 startPos, Vector2 endPos, float damage)
    {
        _lineRenderer.SetPosition(0, startPos);
        _lineRenderer.startWidth = 0.5f;
        _lineRenderer.SetPosition(1, endPos);
        _lineRenderer.endWidth = 0.5f;
        this.damage = damage;

        Instantiate(_impact, endPos, Quaternion.identity);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player") && collision.TryGetComponent<IHitAble>(out var h))
        {
            h.Hit(damage);
        }
    }
}