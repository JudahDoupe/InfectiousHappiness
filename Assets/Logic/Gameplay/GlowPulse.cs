using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class GlowPulse : MonoBehaviour
{
    public float PulseSpeed = 1f;
    public float MaxGlowWidth = 1.15f;

    private int _sign = 1;
    private float _glowWidth = 1f;

	void Update ()
	{
	    if (_glowWidth < 1) _sign = 1;
        else if (_glowWidth > MaxGlowWidth) _sign = -1;

	    _glowWidth += _sign * Time.deltaTime * PulseSpeed;

	    gameObject.GetComponent<Renderer>().material.SetFloat("_GlowWidth", _glowWidth);
	}
}
