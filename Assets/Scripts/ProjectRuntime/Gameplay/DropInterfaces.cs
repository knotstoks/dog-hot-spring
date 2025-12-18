using System.Collections;
using System.Collections.Generic;
using ProjectRuntime.Gameplay;
using UnityEngine;

public class DropInterfaces 
{
    public interface IDroppableTile
    {
        void Drop(BathSlideTile bathSlideTile);

        void CancelDrop();
    }
}
