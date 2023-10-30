using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBucketable
{
    public bool IsBucketable { get; protected set; }

    public void EquipBucket();
}
