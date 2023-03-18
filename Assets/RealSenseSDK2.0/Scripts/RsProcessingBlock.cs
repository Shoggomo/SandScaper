using System;
using System.Collections.Generic;
using UnityEngine;
using Intel.RealSense;
using System.Linq;

public interface IProcessingBlock
{
    Frame Process(Frame frame, FrameSource frameSource);
}

[Serializable]
public abstract class RsProcessingBlock : ScriptableObject, IProcessingBlock
{
    public bool fixedEnabled = true;
    public bool FixedEnabled {get {return Enabled;} set {Enabled = value;}}

    // public bool enabled = true;

    public bool Enabled
    {
        get
        {
            return fixedEnabled;
        }

        set
        {
            // enabled = value;
            fixedEnabled = value;
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetObjectEnabled(this, fixedEnabled);
#endif
        }
    }

    public abstract Frame Process(Frame frame, FrameSource frameSource);

    public virtual void Reset()
    {
        this.name = GetType().Name;

#if UNITY_EDITOR
        var p = UnityEditor.AssetDatabase.GetAssetPath(this);
        var names = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(p).Where(a => a).Select(a => a.name).ToList();
        names.Remove(GetType().Name);
        this.name = UnityEditor.ObjectNames.GetUniqueName(names.ToArray(), GetType().Name);
        UnityEditor.AssetDatabase.SaveAssets();
#endif
    }
}