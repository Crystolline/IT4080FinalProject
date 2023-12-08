using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectablePointMedal : BaseCollectable
{
    protected override bool ApplyToPlayer(Player thePickerUpper)
    {
        thePickerUpper.scoreNetVar.Value += 1;
        return true;
    }
}
