using PathCreation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ParticlePath : MonoBehaviour
{
    [SerializeField] private float curveSize = 1f;
    [SerializeField] private AnimationCurve curve;

    public Action OnPathCompleted;

    float time;
    VertexPath path;

    float timeSincePathSet = 0;

    public void SetPath(Vector2 target, float time)
    {
        timeSincePathSet = 0;
        this.time = time;
        Vector2 normal = ((Vector2)transform.localPosition - target).normalized;
        Vector2 impulse = (normal + Vector2.Perpendicular(normal) * UnityEngine.Random.Range(-1f, 1f)).normalized * curveSize;
        path = new VertexPath(new BezierPath(new Vector2[] { transform.position, (Vector2)transform.position + impulse, target}, false, PathSpace.xy), transform.parent);
    }

    private void Update()
    {
        if (path != null)
        {
            timeSincePathSet += Time.deltaTime / time;
            transform.position = path.GetPointAtTime(curve.Evaluate(Mathf.Clamp01(timeSincePathSet)));
            if (timeSincePathSet >= 1f)
            {
                Destroy(gameObject);
                OnPathCompleted?.Invoke();
            }
        }
    }
}
