using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class VFXEventReciever : MonoBehaviour
{
    private VisualEffect _vfx;

    private void Awake()
    {
        _vfx = GetComponentInChildren<VisualEffect>();
    }

    public void SendEvent(string eventName)
    {
        if (_vfx)
        {
            _vfx.SendEvent(eventName);
        }
    }
}
