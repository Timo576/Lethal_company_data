// Decompiled with JetBrains decompiler
// Type: DunGen.UnityUtil
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;
using UnityEngine.Tilemaps;

#nullable disable
namespace DunGen
{
  public static class UnityUtil
  {
    public static System.Type ProBuilderMeshType { get; private set; }

    public static PropertyInfo ProBuilderPositionsProperty { get; private set; }

    static UnityUtil() => UnityUtil.FindProBuilderObjectType();

    public static void FindProBuilderObjectType()
    {
      if (UnityUtil.ProBuilderMeshType != (System.Type) null)
        return;
      foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
      {
        if (assembly.FullName.Contains("ProBuilder"))
        {
          UnityUtil.ProBuilderMeshType = assembly.GetType("UnityEngine.ProBuilder.ProBuilderMesh");
          if (UnityUtil.ProBuilderMeshType != (System.Type) null)
          {
            UnityUtil.ProBuilderPositionsProperty = UnityUtil.ProBuilderMeshType.GetProperty("positions");
            if (UnityUtil.ProBuilderPositionsProperty != (PropertyInfo) null)
              break;
          }
        }
      }
    }

    public static void Restart(this Stopwatch stopwatch)
    {
      if (stopwatch == null)
      {
        stopwatch = Stopwatch.StartNew();
      }
      else
      {
        stopwatch.Reset();
        stopwatch.Start();
      }
    }

    public static bool Contains(this Bounds bounds, Bounds other)
    {
      return (double) other.min.x >= (double) bounds.min.x && (double) other.min.y >= (double) bounds.min.y && (double) other.min.z >= (double) bounds.min.z && (double) other.max.x <= (double) bounds.max.x && (double) other.max.y <= (double) bounds.max.y && (double) other.max.z <= (double) bounds.max.z;
    }

    public static Bounds TransformBounds(this Transform transform, Bounds localBounds)
    {
      Vector3 center = transform.TransformPoint(localBounds.center);
      Vector3 vector3 = transform.rotation * localBounds.size;
      vector3.x = Mathf.Abs(vector3.x);
      vector3.y = Mathf.Abs(vector3.y);
      vector3.z = Mathf.Abs(vector3.z);
      Vector3 size = vector3;
      return new Bounds(center, size);
    }

    public static Bounds InverseTransformBounds(this Transform transform, Bounds worldBounds)
    {
      Vector3 center = transform.InverseTransformPoint(worldBounds.center);
      Vector3 vector3 = Quaternion.Inverse(transform.rotation) * worldBounds.size;
      vector3.x = Mathf.Abs(vector3.x);
      vector3.y = Mathf.Abs(vector3.y);
      vector3.z = Mathf.Abs(vector3.z);
      Vector3 size = vector3;
      return new Bounds(center, size);
    }

    public static void SetLayerRecursive(GameObject gameObject, int layer)
    {
      gameObject.layer = layer;
      for (int index = 0; index < gameObject.transform.childCount; ++index)
        UnityUtil.SetLayerRecursive(gameObject.transform.GetChild(index).gameObject, layer);
    }

    public static void Destroy(UnityEngine.Object obj)
    {
      if (Application.isPlaying)
      {
        GameObject gameObject = obj as GameObject;
        if ((UnityEngine.Object) gameObject != (UnityEngine.Object) null)
          gameObject.SetActive(false);
        UnityEngine.Object.Destroy(obj);
      }
      else
        UnityEngine.Object.DestroyImmediate(obj);
    }

    public static string GetUniqueName(string name, IEnumerable<string> usedNames)
    {
      if (string.IsNullOrEmpty(name))
        return UnityUtil.GetUniqueName("New", usedNames);
      string str = name;
      int result = 0;
      bool flag = false;
      int length = name.LastIndexOf(' ');
      if (length > -1)
      {
        str = name.Substring(0, length);
        flag = int.TryParse(name.Substring(length + 1), out result);
        ++result;
      }
      foreach (string usedName in usedNames)
      {
        if (usedName == name)
          return flag ? UnityUtil.GetUniqueName(str + " " + result.ToString(), usedNames) : UnityUtil.GetUniqueName(name + " 2", usedNames);
      }
      return name;
    }

    public static Bounds CombineBounds(params Bounds[] bounds)
    {
      if (bounds.Length == 0)
        return new Bounds();
      if (bounds.Length == 1)
        return bounds[0];
      Bounds bound = bounds[0];
      for (int index = 1; index < bounds.Length; ++index)
        bound.Encapsulate(bounds[index]);
      return bound;
    }

    public static Bounds CalculateProxyBounds(
      GameObject prefab,
      bool ignoreSpriteRendererBounds,
      Vector3 upVector)
    {
      Bounds objectBounds = UnityUtil.CalculateObjectBounds(prefab, true, ignoreSpriteRendererBounds);
      if (UnityUtil.ProBuilderMeshType != (System.Type) null && UnityUtil.ProBuilderPositionsProperty != (PropertyInfo) null)
      {
        foreach (Component componentsInChild in prefab.GetComponentsInChildren(UnityUtil.ProBuilderMeshType))
        {
          Vector3 vector3 = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
          Vector3 lhs = new Vector3(float.MinValue, float.MinValue, float.MinValue);
          foreach (Vector3 rhs in (IEnumerable<Vector3>) UnityUtil.ProBuilderPositionsProperty.GetValue((object) componentsInChild, (object[]) null))
          {
            vector3 = Vector3.Min(vector3, rhs);
            lhs = Vector3.Max(lhs, rhs);
          }
          Vector3 size = prefab.transform.TransformDirection(lhs - vector3);
          Vector3 center = prefab.transform.TransformPoint(vector3) + size / 2f;
          objectBounds.Encapsulate(new Bounds(center, size));
        }
      }
      return objectBounds;
    }

    public static Bounds CalculateObjectBounds(
      GameObject obj,
      bool includeInactive,
      bool ignoreSpriteRenderers,
      bool ignoreTriggerColliders = true)
    {
      Bounds objectBounds = new Bounds();
      bool flag = false;
      foreach (Tilemap componentsInChild in obj.GetComponentsInChildren<Tilemap>(includeInactive))
        componentsInChild.CompressBounds();
      foreach (Renderer componentsInChild in obj.GetComponentsInChildren<Renderer>(includeInactive))
      {
        if ((!ignoreSpriteRenderers || !(componentsInChild is SpriteRenderer)) && !(componentsInChild is ParticleSystemRenderer))
        {
          if (flag)
            objectBounds.Encapsulate(componentsInChild.bounds);
          else
            objectBounds = componentsInChild.bounds;
          flag = true;
        }
      }
      foreach (Collider componentsInChild in obj.GetComponentsInChildren<Collider>(includeInactive))
      {
        if (!ignoreTriggerColliders || !componentsInChild.isTrigger)
        {
          if (flag)
            objectBounds.Encapsulate(componentsInChild.bounds);
          else
            objectBounds = componentsInChild.bounds;
          flag = true;
        }
      }
      Vector3 extents = objectBounds.extents;
      if ((double) extents.x == 0.0)
        extents.x = 0.01f;
      else if ((double) extents.x < 0.0)
        extents.x *= -1f;
      if ((double) extents.y == 0.0)
        extents.y = 0.01f;
      else if ((double) extents.y < 0.0)
        extents.y *= -1f;
      if ((double) extents.z == 0.0)
        extents.z = 0.01f;
      else if ((double) extents.z < 0.0)
        extents.z *= -1f;
      objectBounds.extents = extents;
      return objectBounds;
    }

    public static void PositionObjectBySocket(
      GameObject objectA,
      GameObject socketA,
      GameObject socketB)
    {
      UnityUtil.PositionObjectBySocket(objectA.transform, socketA.transform, socketB.transform);
    }

    public static void PositionObjectBySocket(
      Transform objectA,
      Transform socketA,
      Transform socketB)
    {
      Quaternion quaternion = Quaternion.LookRotation(-socketB.forward, socketB.up);
      objectA.rotation = quaternion * Quaternion.Inverse(Quaternion.Inverse(objectA.rotation) * socketA.rotation);
      Vector3 position = socketB.position;
      objectA.position = position - (socketA.position - objectA.position);
    }

    public static Vector3 GetCardinalDirection(Vector3 direction, out float magnitude)
    {
      float num1 = Math.Abs(direction.x);
      float num2 = Math.Abs(direction.y);
      float num3 = Math.Abs(direction.z);
      float x = direction.x / num1;
      float y = direction.y / num2;
      float z = direction.z / num3;
      if ((double) num1 > (double) num2 && (double) num1 > (double) num3)
      {
        magnitude = x;
        return new Vector3(x, 0.0f, 0.0f);
      }
      if ((double) num2 > (double) num1 && (double) num2 > (double) num3)
      {
        magnitude = y;
        return new Vector3(0.0f, y, 0.0f);
      }
      if ((double) num3 > (double) num1 && (double) num3 > (double) num2)
      {
        magnitude = z;
        return new Vector3(0.0f, 0.0f, z);
      }
      magnitude = x;
      return new Vector3(x, 0.0f, 0.0f);
    }

    public static Vector3 VectorAbs(Vector3 vector)
    {
      return new Vector3(Math.Abs(vector.x), Math.Abs(vector.y), Math.Abs(vector.z));
    }

    public static void SetVector3Masked(ref Vector3 input, Vector3 value, Vector3 mask)
    {
      if ((double) mask.x != 0.0)
        input.x = value.x;
      if ((double) mask.y != 0.0)
        input.y = value.y;
      if ((double) mask.z == 0.0)
        return;
      input.z = value.z;
    }

    public static Bounds CondenseBounds(Bounds bounds, IEnumerable<Doorway> doorways)
    {
      Vector3 input1 = bounds.center - bounds.extents;
      Vector3 input2 = bounds.center + bounds.extents;
      foreach (Doorway doorway in doorways)
      {
        float magnitude;
        Vector3 cardinalDirection = UnityUtil.GetCardinalDirection(doorway.transform.forward, out magnitude);
        if ((double) magnitude < 0.0)
          UnityUtil.SetVector3Masked(ref input1, doorway.transform.position, cardinalDirection);
        else
          UnityUtil.SetVector3Masked(ref input2, doorway.transform.position, cardinalDirection);
      }
      Vector3 size = input2 - input1;
      return new Bounds(input1 + size / 2f, size);
    }

    public static IEnumerable<T> GetComponentsInParents<T>(GameObject obj, bool includeInactive = false) where T : Component
    {
      if (obj.activeSelf | includeInactive)
      {
        T[] objArray = obj.GetComponents<T>();
        for (int index = 0; index < objArray.Length; ++index)
          yield return objArray[index];
        objArray = (T[]) null;
      }
      if ((UnityEngine.Object) obj.transform.parent != (UnityEngine.Object) null)
      {
        foreach (T componentsInParent in UnityUtil.GetComponentsInParents<T>(obj.transform.parent.gameObject, includeInactive))
          yield return componentsInParent;
      }
    }

    public static T GetComponentInParents<T>(GameObject obj, bool includeInactive = false) where T : Component
    {
      if (obj.activeSelf | includeInactive)
      {
        T[] components = obj.GetComponents<T>();
        int index = 0;
        if (index < components.Length)
          return components[index];
      }
      return (UnityEngine.Object) obj.transform.parent != (UnityEngine.Object) null ? UnityUtil.GetComponentInParents<T>(obj.transform.parent.gameObject, includeInactive) : default (T);
    }

    public static float CalculateOverlap(Bounds boundsA, Bounds boundsB)
    {
      return Mathf.Min(boundsA.max.x - boundsB.min.x, boundsB.max.x - boundsA.min.x, boundsA.max.y - boundsB.min.y, boundsB.max.y - boundsA.min.y, boundsA.max.z - boundsB.min.z, boundsB.max.z - boundsA.min.z);
    }

    public static Vector3 CalculatePerAxisOverlap(Bounds boundsA, Bounds boundsB)
    {
      double a1 = (double) boundsA.max.x - (double) boundsB.min.x;
      float num = boundsB.max.x - boundsA.min.x;
      float a2 = boundsA.max.y - boundsB.min.y;
      float b1 = boundsB.max.y - boundsA.min.y;
      float a3 = boundsA.max.z - boundsB.min.z;
      float b2 = boundsB.max.z - boundsA.min.z;
      double b3 = (double) num;
      return new Vector3(Mathf.Min((float) a1, (float) b3), Mathf.Min(a2, b1), Mathf.Min(a3, b2));
    }
  }
}
