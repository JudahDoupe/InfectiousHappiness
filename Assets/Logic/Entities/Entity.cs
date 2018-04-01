using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    private bool _isActive;
    private bool _isDyed;

    public string Class;
    public string Type;
    public bool IsActive
    {
        get { return _isActive; }
        set
        {
            if (_isActive == value) return;
            _isActive = value;
            UpdateMaterial();
        }
    }
    public bool IsDyed
    {
        get { return _isDyed; }
        set
        {
            if (_isDyed == value) return;
            _isDyed = value;
            UpdateMaterial();
        }
    }

    public Voxel Voxel;

    public virtual void UpdateMaterial()
    {
        var mat = Resources.Load<Material>("Entities/Materials/" + Class + "/" + (IsDyed ? Type : "Undyed") + (IsActive ? "" : "Inactive"));
        if (mat == null)return;
        transform.Find("Model").GetComponent<Renderer>().material = mat;
    }
}

