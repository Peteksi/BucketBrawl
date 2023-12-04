using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BucketBrawl
{
    public static class CustomGizmos
    {
        public static void DrawCircle(Vector3 center, Vector3 normal, float radius, int segments = 32)
        {
            Vector3 radiusVector = Mathf.Abs(Vector3.Dot(normal, Vector3.right)) - 1f <= Mathf.Epsilon
                ? Vector3.Cross(normal, Vector3.forward).normalized
                : Vector3.Cross(normal, Vector3.right).normalized;
            radiusVector *= radius;
            float angleBetweenSegments = 360f / segments;
            Vector3 previousCircumferencePoint = center + radiusVector;
            for (int i = 0; i < segments; ++i)
            {
                radiusVector = Quaternion.AngleAxis(angleBetweenSegments, normal) * radiusVector;
                Vector3 newCircumferencePoint = center + radiusVector;
                Gizmos.DrawLine(previousCircumferencePoint, newCircumferencePoint);
                previousCircumferencePoint = newCircumferencePoint;
            }
        }

        public static void DrawWireCapsule(Vector3 point1, Vector3 point2, float radius)
        {
            Gizmos.DrawWireSphere(point1, radius);
            Gizmos.DrawWireSphere(point2, radius);

            Gizmos.DrawLine(point1 + Vector3.up * radius, point2 + Vector3.up * radius);
            Gizmos.DrawLine(point1 - Vector3.up * radius, point2 - Vector3.up * radius);
            Gizmos.DrawLine(point1 + Vector3.right * radius, point2 + Vector3.right * radius);
            Gizmos.DrawLine(point1 - Vector3.right * radius, point2 - Vector3.right * radius);
            Gizmos.DrawLine(point1 + Vector3.forward * radius, point2 + Vector3.forward * radius);
            Gizmos.DrawLine(point1 - Vector3.forward * radius, point2 - Vector3.forward * radius);
        }
    }
}