using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBucketable
{
    public bool IsBucketable();

    public void EquipItem(ItemBase item);

    public void UnequipItem();
}
