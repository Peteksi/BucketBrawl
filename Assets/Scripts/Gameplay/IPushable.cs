
using System.Numerics;


public interface IPushable
{
    public bool IsPushable();

    public void SetPushVelocity(Vector3 direction, float scalar);
}
