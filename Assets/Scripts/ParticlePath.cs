using PathCreation;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ParticlePath : MonoBehaviour
{
    [SerializeField] private Vector2 start, target;

    VertexPath path;

    private void Awake()
    {
        path = new VertexPath(new BezierPath(new Vector2[]{start, target}, false, PathSpace.xy), transform);
    }
    private void Update()
    {
        float t = Mathf.Sin(Time.time);
        transform.position = path.GetPointAtTime(t);
    }
}
