// Decompiled with JetBrains decompiler
// Type: DigitalRuby.ThunderAndLightning.LightningBoltTransformTrackerScript
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#nullable disable
namespace DigitalRuby.ThunderAndLightning
{
  public class LightningBoltTransformTrackerScript : MonoBehaviour
  {
    [Tooltip("The lightning script to track.")]
    public LightningBoltPrefabScript LightningScript;
    [Tooltip("The transform to track which will be where the bolts are emitted from.")]
    public Transform StartTarget;
    [Tooltip("(Optional) The transform to track which will be where the bolts are emitted to. If no end target is specified, lightning will simply move to stay on top of the start target.")]
    public Transform EndTarget;
    [SingleLine("Scaling limits.")]
    public RangeOfFloats ScaleLimit = new RangeOfFloats()
    {
      Minimum = 0.1f,
      Maximum = 10f
    };
    private readonly Dictionary<Transform, LightningCustomTransformStateInfo> transformStartPositions = new Dictionary<Transform, LightningCustomTransformStateInfo>();

    private void Start()
    {
      if (!((Object) this.LightningScript != (Object) null))
        return;
      this.LightningScript.CustomTransformHandler.RemoveAllListeners();
      this.LightningScript.CustomTransformHandler.AddListener(new UnityAction<LightningCustomTransformStateInfo>(this.CustomTransformHandler));
    }

    private static float AngleBetweenVector2(Vector2 vec1, Vector2 vec2)
    {
      return Vector2.Angle(Vector2.right, (vec2 - vec1).normalized) * Mathf.Sign(vec2.y - vec1.y);
    }

    private static void UpdateTransform(
      LightningCustomTransformStateInfo state,
      LightningBoltPrefabScript script,
      RangeOfFloats scaleLimit)
    {
      if ((Object) state.Transform == (Object) null || (Object) state.StartTransform == (Object) null)
        return;
      if ((Object) state.EndTransform == (Object) null)
      {
        state.Transform.position = state.StartTransform.position - state.BoltStartPosition;
      }
      else
      {
        if (script.CameraMode == CameraMode.Auto && script.Camera.orthographic || script.CameraMode == CameraMode.OrthographicXY)
        {
          float num = LightningBoltTransformTrackerScript.AngleBetweenVector2((Vector2) state.BoltStartPosition, (Vector2) state.BoltEndPosition);
          Quaternion.AngleAxis(LightningBoltTransformTrackerScript.AngleBetweenVector2((Vector2) state.StartTransform.position, (Vector2) state.EndTransform.position) - num, Vector3.forward);
        }
        Quaternion quaternion;
        if (script.CameraMode == CameraMode.OrthographicXZ)
        {
          float num = LightningBoltTransformTrackerScript.AngleBetweenVector2(new Vector2(state.BoltStartPosition.x, state.BoltStartPosition.z), new Vector2(state.BoltEndPosition.x, state.BoltEndPosition.z));
          quaternion = Quaternion.AngleAxis(LightningBoltTransformTrackerScript.AngleBetweenVector2(new Vector2(state.StartTransform.position.x, state.StartTransform.position.z), new Vector2(state.EndTransform.position.x, state.EndTransform.position.z)) - num, Vector3.up);
        }
        else
        {
          Quaternion rotation = Quaternion.LookRotation((state.BoltEndPosition - state.BoltStartPosition).normalized);
          quaternion = Quaternion.LookRotation((state.EndTransform.position - state.StartTransform.position).normalized) * Quaternion.Inverse(rotation);
        }
        state.Transform.rotation = quaternion;
        float num1 = Vector3.Distance(state.BoltStartPosition, state.BoltEndPosition);
        float num2 = Vector3.Distance(state.EndTransform.position, state.StartTransform.position);
        float num3 = Mathf.Clamp((double) num1 < (double) Mathf.Epsilon ? 1f : num2 / num1, scaleLimit.Minimum, scaleLimit.Maximum);
        state.Transform.localScale = new Vector3(num3, num3, num3);
        Vector3 vector3 = quaternion * (num3 * state.BoltStartPosition);
        state.Transform.position = state.StartTransform.position - vector3;
      }
    }

    public void CustomTransformHandler(LightningCustomTransformStateInfo state)
    {
      if (!this.enabled)
        return;
      if ((Object) this.LightningScript == (Object) null)
        Debug.LogError((object) "LightningScript property must be set to non-null.");
      else if (state.State == LightningCustomTransformState.Executing)
        LightningBoltTransformTrackerScript.UpdateTransform(state, this.LightningScript, this.ScaleLimit);
      else if (state.State == LightningCustomTransformState.Started)
      {
        state.StartTransform = this.StartTarget;
        state.EndTransform = this.EndTarget;
        this.transformStartPositions[this.transform] = state;
      }
      else
        this.transformStartPositions.Remove(this.transform);
    }
  }
}
