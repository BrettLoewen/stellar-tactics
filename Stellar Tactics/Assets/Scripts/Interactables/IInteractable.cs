using System;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    #region Function Signatures

    void Interact(Action onInteractComplete);

    #endregion //end Function Signatures
}
