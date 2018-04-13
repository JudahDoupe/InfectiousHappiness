using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    private bool _isActive;
    private bool _isDyed;

    public string Class { get; set; }
    public string Type { get; set; }
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

    public Voxel Voxel;

    public virtual void UpdateMaterial()
    {
        var mat = Resources.Load<Material>("Entities/Materials/" + Class + "/" + Type);
        if (mat == null)return;
        transform.Find("Model").GetComponent<Renderer>().material = mat;
    }
}

