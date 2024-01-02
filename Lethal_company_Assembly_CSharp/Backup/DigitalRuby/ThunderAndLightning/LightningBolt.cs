// Decompiled with JetBrains decompiler
// Type: DigitalRuby.ThunderAndLightning.LightningBolt
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

#nullable disable
namespace DigitalRuby.ThunderAndLightning
{
  public class LightningBolt
  {
    public static int MaximumLightCount = 128;
    public static int MaximumLightsPerBatch = 8;
    private DateTime startTimeOffset;
    private LightningBoltDependencies dependencies;
    private float elapsedTime;
    private float lifeTime;
    private float maxLifeTime;
    private bool hasLight;
    private float timeSinceLevelLoad;
    private readonly List<LightningBoltSegmentGroup> segmentGroups = new List<LightningBoltSegmentGroup>();
    private readonly List<LightningBoltSegmentGroup> segmentGroupsWithLight = new List<LightningBoltSegmentGroup>();
    private readonly List<LightningBolt.LineRendererMesh> activeLineRenderers = new List<LightningBolt.LineRendererMesh>();
    private static int lightCount;
    private static readonly List<LightningBolt.LineRendererMesh> lineRendererCache = new List<LightningBolt.LineRendererMesh>();
    private static readonly List<LightningBoltSegmentGroup> groupCache = new List<LightningBoltSegmentGroup>();
    private static readonly List<Light> lightCache = new List<Light>();

    public float MinimumDelay { get; private set; }

    public bool HasGlow { get; private set; }

    public bool IsActive => (double) this.elapsedTime < (double) this.lifeTime;

    public CameraMode CameraMode { get; private set; }

    public void SetupLightningBolt(LightningBoltDependencies dependencies)
    {
      if (dependencies == null || dependencies.Parameters.Count == 0)
        Debug.LogError((object) "Lightning bolt dependencies must not be null");
      else if (this.dependencies != null)
      {
        Debug.LogError((object) "This lightning bolt is already in use!");
      }
      else
      {
        this.dependencies = dependencies;
        this.CameraMode = dependencies.CameraMode;
        this.timeSinceLevelLoad = LightningBoltScript.TimeSinceStart;
        this.CheckForGlow((IEnumerable<LightningBoltParameters>) dependencies.Parameters);
        this.MinimumDelay = float.MaxValue;
        if (dependencies.ThreadState.multiThreaded)
        {
          this.startTimeOffset = DateTime.UtcNow;
          dependencies.ThreadState.AddActionForBackgroundThread(new Action(this.ProcessAllLightningParameters));
        }
        else
          this.ProcessAllLightningParameters();
      }
    }

    public bool Update()
    {
      this.elapsedTime += LightningBoltScript.DeltaTime;
      if ((double) this.elapsedTime > (double) this.maxLifeTime)
        return false;
      if (this.hasLight)
        this.UpdateLights();
      return true;
    }

    public void Cleanup()
    {
      foreach (LightningBoltSegmentGroup boltSegmentGroup in this.segmentGroupsWithLight)
      {
        foreach (Light light in boltSegmentGroup.Lights)
          this.CleanupLight(light);
        boltSegmentGroup.Lights.Clear();
      }
      lock (LightningBolt.groupCache)
      {
        foreach (LightningBoltSegmentGroup segmentGroup in this.segmentGroups)
          LightningBolt.groupCache.Add(segmentGroup);
      }
      this.hasLight = false;
      this.elapsedTime = 0.0f;
      this.lifeTime = 0.0f;
      this.maxLifeTime = 0.0f;
      if (this.dependencies != null)
      {
        this.dependencies.ReturnToCache(this.dependencies);
        this.dependencies = (LightningBoltDependencies) null;
      }
      foreach (LightningBolt.LineRendererMesh activeLineRenderer in this.activeLineRenderers)
      {
        if (activeLineRenderer != null)
        {
          activeLineRenderer.Reset();
          LightningBolt.lineRendererCache.Add(activeLineRenderer);
        }
      }
      this.segmentGroups.Clear();
      this.segmentGroupsWithLight.Clear();
      this.activeLineRenderers.Clear();
    }

    public LightningBoltSegmentGroup AddGroup()
    {
      LightningBoltSegmentGroup boltSegmentGroup;
      lock (LightningBolt.groupCache)
      {
        if (LightningBolt.groupCache.Count == 0)
        {
          boltSegmentGroup = new LightningBoltSegmentGroup();
        }
        else
        {
          int index = LightningBolt.groupCache.Count - 1;
          boltSegmentGroup = LightningBolt.groupCache[index];
          boltSegmentGroup.Reset();
          LightningBolt.groupCache.RemoveAt(index);
        }
      }
      this.segmentGroups.Add(boltSegmentGroup);
      return boltSegmentGroup;
    }

    public static void ClearCache()
    {
      foreach (LightningBolt.LineRendererMesh lineRendererMesh in LightningBolt.lineRendererCache)
      {
        if (lineRendererMesh != null)
          UnityEngine.Object.Destroy((UnityEngine.Object) lineRendererMesh.GameObject);
      }
      foreach (Light light in LightningBolt.lightCache)
      {
        if ((UnityEngine.Object) light != (UnityEngine.Object) null)
          UnityEngine.Object.Destroy((UnityEngine.Object) light.gameObject);
      }
      LightningBolt.lineRendererCache.Clear();
      LightningBolt.lightCache.Clear();
      lock (LightningBolt.groupCache)
        LightningBolt.groupCache.Clear();
    }

    private void CleanupLight(Light l)
    {
      if (!((UnityEngine.Object) l != (UnityEngine.Object) null))
        return;
      this.dependencies.LightRemoved(l);
      LightningBolt.lightCache.Add(l);
      l.gameObject.SetActive(false);
      --LightningBolt.lightCount;
    }

    private void EnableLineRenderer(LightningBolt.LineRendererMesh lineRenderer, int tag)
    {
      if ((lineRenderer == null || !((UnityEngine.Object) lineRenderer.GameObject != (UnityEngine.Object) null) || lineRenderer.Tag != tag ? 0 : (this.IsActive ? 1 : 0)) == 0)
        return;
      lineRenderer.PopulateMesh();
    }

    private IEnumerator EnableLastRendererCoRoutine()
    {
      LightningBolt.LineRendererMesh lineRenderer = this.activeLineRenderers[this.activeLineRenderers.Count - 1];
      int tag = ++lineRenderer.Tag;
      yield return (object) new WaitForSecondsLightning(this.MinimumDelay);
      this.EnableLineRenderer(lineRenderer, tag);
    }

    private LightningBolt.LineRendererMesh GetOrCreateLineRenderer()
    {
      LightningBolt.LineRendererMesh lineRenderer;
      while (LightningBolt.lineRendererCache.Count != 0)
      {
        int index = LightningBolt.lineRendererCache.Count - 1;
        lineRenderer = LightningBolt.lineRendererCache[index];
        LightningBolt.lineRendererCache.RemoveAt(index);
        if (lineRenderer != null && !((UnityEngine.Object) lineRenderer.Transform == (UnityEngine.Object) null))
          goto label_4;
      }
      lineRenderer = new LightningBolt.LineRendererMesh(this.dependencies);
label_4:
      this.dependencies.ThreadState.AddActionForMainThread((Action<bool>) (b =>
      {
        lineRenderer.Transform.parent = (Transform) null;
        lineRenderer.Transform.rotation = Quaternion.identity;
        lineRenderer.Transform.localScale = Vector3.one;
        lineRenderer.Transform.parent = this.dependencies.Parent.transform;
        lineRenderer.GameObject.layer = lineRenderer.MeshRendererBolt.gameObject.layer = lineRenderer.MeshRendererGlow.gameObject.layer = this.dependencies.Parent.layer;
        if (this.dependencies.UseWorldSpace)
          lineRenderer.GameObject.transform.position = Vector3.zero;
        else
          lineRenderer.GameObject.transform.localPosition = Vector3.zero;
        lineRenderer.MaterialGlow = this.dependencies.LightningMaterialMesh;
        lineRenderer.MaterialBolt = this.dependencies.LightningMaterialMeshNoGlow;
        if (!string.IsNullOrEmpty(this.dependencies.SortLayerName))
        {
          lineRenderer.MeshRendererGlow.sortingLayerName = lineRenderer.MeshRendererBolt.sortingLayerName = this.dependencies.SortLayerName;
          lineRenderer.MeshRendererGlow.sortingOrder = lineRenderer.MeshRendererBolt.sortingOrder = this.dependencies.SortOrderInLayer;
        }
        else
        {
          lineRenderer.MeshRendererGlow.sortingLayerName = lineRenderer.MeshRendererBolt.sortingLayerName = (string) null;
          lineRenderer.MeshRendererGlow.sortingOrder = lineRenderer.MeshRendererBolt.sortingOrder = 0;
        }
      }), true);
      this.activeLineRenderers.Add(lineRenderer);
      return lineRenderer;
    }

    private void RenderGroup(LightningBoltSegmentGroup group, LightningBoltParameters p)
    {
      if (group.SegmentCount == 0)
        return;
      float num1 = !this.dependencies.ThreadState.multiThreaded ? 0.0f : (float) (DateTime.UtcNow - this.startTimeOffset).TotalSeconds;
      float x = this.timeSinceLevelLoad + group.Delay + num1;
      Vector4 fadeLifeTime = new Vector4(x, x + group.PeakStart, x + group.PeakEnd, x + group.LifeTime);
      float radius = group.LineWidth * 0.5f * LightningBoltParameters.Scale;
      int lineCount = group.Segments.Count - group.StartIndex;
      float num2 = (radius - radius * group.EndWidthMultiplier) / (float) lineCount;
      float num3;
      float num4;
      if ((double) p.GrowthMultiplier > 0.0)
      {
        num3 = group.LifeTime / (float) lineCount * p.GrowthMultiplier;
        num4 = 0.0f;
      }
      else
      {
        num3 = 0.0f;
        num4 = 0.0f;
      }
      LightningBolt.LineRendererMesh currentLineRenderer = this.activeLineRenderers.Count == 0 ? this.GetOrCreateLineRenderer() : this.activeLineRenderers[this.activeLineRenderers.Count - 1];
      if (!currentLineRenderer.PrepareForLines(lineCount))
      {
        if (currentLineRenderer.CustomTransform != null)
          return;
        if (this.dependencies.ThreadState.multiThreaded)
        {
          this.dependencies.ThreadState.AddActionForMainThread((Action<bool>) (inDestroy =>
          {
            if (inDestroy)
              return;
            this.EnableCurrentLineRenderer();
            currentLineRenderer = this.GetOrCreateLineRenderer();
          }), true);
        }
        else
        {
          this.EnableCurrentLineRenderer();
          currentLineRenderer = this.GetOrCreateLineRenderer();
        }
      }
      currentLineRenderer.BeginLine(group.Segments[group.StartIndex].Start, group.Segments[group.StartIndex].End, radius, group.Color, p.Intensity, fadeLifeTime, p.GlowWidthMultiplier, p.GlowIntensity);
      for (int index = group.StartIndex + 1; index < group.Segments.Count; ++index)
      {
        radius -= num2;
        if ((double) p.GrowthMultiplier < 1.0)
        {
          num4 += num3;
          fadeLifeTime = new Vector4(x + num4, x + group.PeakStart + num4, x + group.PeakEnd, x + group.LifeTime);
        }
        currentLineRenderer.AppendLine(group.Segments[index].Start, group.Segments[index].End, radius, group.Color, p.Intensity, fadeLifeTime, p.GlowWidthMultiplier, p.GlowIntensity);
      }
    }

    private static IEnumerator NotifyBolt(
      LightningBoltDependencies dependencies,
      LightningBoltParameters p,
      Transform transform,
      Vector3 start,
      Vector3 end)
    {
      float delaySeconds = p.delaySeconds;
      float lifeTime = p.LifeTime;
      yield return (object) new WaitForSecondsLightning(delaySeconds);
      if (dependencies.LightningBoltStarted != null)
        dependencies.LightningBoltStarted(p, start, end);
      LightningCustomTransformStateInfo state = p.CustomTransform == null ? (LightningCustomTransformStateInfo) null : LightningCustomTransformStateInfo.GetOrCreateStateInfo();
      if (state != null)
      {
        state.Parameters = p;
        state.BoltStartPosition = start;
        state.BoltEndPosition = end;
        state.State = LightningCustomTransformState.Started;
        state.Transform = transform;
        p.CustomTransform(state);
        state.State = LightningCustomTransformState.Executing;
      }
      if (p.CustomTransform == null)
      {
        yield return (object) new WaitForSecondsLightning(lifeTime);
      }
      else
      {
        while ((double) lifeTime > 0.0)
        {
          p.CustomTransform(state);
          lifeTime -= LightningBoltScript.DeltaTime;
          yield return (object) null;
        }
      }
      if (p.CustomTransform != null)
      {
        state.State = LightningCustomTransformState.Ended;
        p.CustomTransform(state);
        LightningCustomTransformStateInfo.ReturnStateInfoToCache(state);
      }
      if (dependencies.LightningBoltEnded != null)
        dependencies.LightningBoltEnded(p, start, end);
      LightningBoltParameters.ReturnParametersToCache(p);
    }

    private void ProcessParameters(
      LightningBoltParameters p,
      RangeOfFloats delay,
      LightningBoltDependencies depends)
    {
      this.MinimumDelay = Mathf.Min(delay.Minimum, this.MinimumDelay);
      p.delaySeconds = delay.Random(p.Random);
      if ((double) depends.LevelOfDetailDistance > (double) Mathf.Epsilon)
      {
        float num1;
        float num2;
        if (p.Points.Count > 1)
        {
          num1 = Vector3.Distance(depends.CameraPos, p.Points[0]);
          num2 = Mathf.Min(Vector3.Distance(depends.CameraPos, p.Points[p.Points.Count - 1]));
        }
        else
        {
          num1 = Vector3.Distance(depends.CameraPos, p.Start);
          num2 = Mathf.Min(Vector3.Distance(depends.CameraPos, p.End));
        }
        int num3 = Mathf.Min(8, (int) ((double) num2 / (double) depends.LevelOfDetailDistance));
        p.Generations = Mathf.Max(1, p.Generations - num3);
        p.GenerationWhereForksStopSubtractor = Mathf.Clamp(p.GenerationWhereForksStopSubtractor - num3, 0, 8);
      }
      p.generationWhereForksStop = p.Generations - p.GenerationWhereForksStopSubtractor;
      this.lifeTime = Mathf.Max(p.LifeTime + p.delaySeconds, this.lifeTime);
      this.maxLifeTime = Mathf.Max(this.lifeTime, this.maxLifeTime);
      p.forkednessCalculated = (int) Mathf.Ceil(p.Forkedness * (float) p.Generations);
      if (p.Generations <= 0)
        return;
      p.Generator = p.Generator ?? LightningGenerator.GeneratorInstance;
      Vector3 start;
      Vector3 end;
      p.Generator.GenerateLightningBolt(this, p, out start, out end);
      p.Start = start;
      p.End = end;
    }

    private void ProcessAllLightningParameters()
    {
      int num = LightningBolt.MaximumLightsPerBatch / this.dependencies.Parameters.Count;
      RangeOfFloats delay = new RangeOfFloats();
      List<int> intList = new List<int>(this.dependencies.Parameters.Count + 1);
      int index = 0;
      foreach (LightningBoltParameters parameter in (IEnumerable<LightningBoltParameters>) this.dependencies.Parameters)
      {
        delay.Minimum = parameter.DelayRange.Minimum + parameter.Delay;
        delay.Maximum = parameter.DelayRange.Maximum + parameter.Delay;
        parameter.maxLights = num;
        intList.Add(this.segmentGroups.Count);
        this.ProcessParameters(parameter, delay, this.dependencies);
      }
      intList.Add(this.segmentGroups.Count);
      LightningBoltDependencies dependenciesRef = this.dependencies;
      foreach (LightningBoltParameters parameter in (IEnumerable<LightningBoltParameters>) dependenciesRef.Parameters)
      {
        LightningBoltParameters parameters = parameter;
        Transform transform = this.RenderLightningBolt(parameters.quality, parameters.Generations, intList[index], intList[++index], parameters);
        if (dependenciesRef.ThreadState.multiThreaded)
        {
          dependenciesRef.ThreadState.AddActionForMainThread((Action<bool>) (inDestroy =>
          {
            if (inDestroy)
              return;
            Coroutine coroutine = dependenciesRef.StartCoroutine(LightningBolt.NotifyBolt(dependenciesRef, parameters, transform, parameters.Start, parameters.End));
          }));
        }
        else
        {
          Coroutine coroutine1 = dependenciesRef.StartCoroutine(LightningBolt.NotifyBolt(dependenciesRef, parameters, transform, parameters.Start, parameters.End));
        }
      }
      if (this.dependencies.ThreadState.multiThreaded)
      {
        this.dependencies.ThreadState.AddActionForMainThread(new Action<bool>(this.EnableCurrentLineRendererFromThread));
      }
      else
      {
        this.EnableCurrentLineRenderer();
        this.dependencies.AddActiveBolt(this);
      }
    }

    private void EnableCurrentLineRendererFromThread(bool inDestroy)
    {
      if (inDestroy)
        return;
      this.EnableCurrentLineRenderer();
      this.dependencies.AddActiveBolt(this);
    }

    private void EnableCurrentLineRenderer()
    {
      if (this.activeLineRenderers.Count == 0)
        return;
      if ((double) this.MinimumDelay <= 0.0)
      {
        this.EnableLineRenderer(this.activeLineRenderers[this.activeLineRenderers.Count - 1], this.activeLineRenderers[this.activeLineRenderers.Count - 1].Tag);
      }
      else
      {
        Coroutine coroutine = this.dependencies.StartCoroutine(this.EnableLastRendererCoRoutine());
      }
    }

    private void RenderParticleSystems(
      Vector3 start,
      Vector3 end,
      float trunkWidth,
      float lifeTime,
      float delaySeconds)
    {
      if ((double) trunkWidth <= 0.0)
        return;
      if ((UnityEngine.Object) this.dependencies.OriginParticleSystem != (UnityEngine.Object) null)
      {
        Coroutine coroutine1 = this.dependencies.StartCoroutine(this.GenerateParticleCoRoutine(this.dependencies.OriginParticleSystem, start, delaySeconds));
      }
      if (!((UnityEngine.Object) this.dependencies.DestParticleSystem != (UnityEngine.Object) null))
        return;
      Coroutine coroutine2 = this.dependencies.StartCoroutine(this.GenerateParticleCoRoutine(this.dependencies.DestParticleSystem, end, delaySeconds + lifeTime * 0.8f));
    }

    private Transform RenderLightningBolt(
      LightningBoltQualitySetting quality,
      int generations,
      int startGroupIndex,
      int endGroupIndex,
      LightningBoltParameters parameters)
    {
      if (this.segmentGroups.Count == 0 || startGroupIndex >= this.segmentGroups.Count || endGroupIndex > this.segmentGroups.Count)
        return (Transform) null;
      Transform transform = (Transform) null;
      LightningLightParameters lp = parameters.LightParameters;
      if (lp != null)
      {
        if (this.hasLight |= lp.HasLight)
        {
          lp.LightPercent = Mathf.Clamp(lp.LightPercent, Mathf.Epsilon, 1f);
          lp.LightShadowPercent = Mathf.Clamp(lp.LightShadowPercent, 0.0f, 1f);
        }
        else
          lp = (LightningLightParameters) null;
      }
      LightningBoltSegmentGroup segmentGroup1 = this.segmentGroups[startGroupIndex];
      Vector3 start = segmentGroup1.Segments[segmentGroup1.StartIndex].Start;
      Vector3 end = segmentGroup1.Segments[segmentGroup1.StartIndex + segmentGroup1.SegmentCount - 1].End;
      parameters.FadePercent = Mathf.Clamp(parameters.FadePercent, 0.0f, 0.5f);
      if (parameters.CustomTransform != null)
      {
        LightningBolt.LineRendererMesh currentLineRenderer = this.activeLineRenderers.Count == 0 || !this.activeLineRenderers[this.activeLineRenderers.Count - 1].Empty ? (LightningBolt.LineRendererMesh) null : this.activeLineRenderers[this.activeLineRenderers.Count - 1];
        if (currentLineRenderer == null)
        {
          if (this.dependencies.ThreadState.multiThreaded)
          {
            this.dependencies.ThreadState.AddActionForMainThread((Action<bool>) (inDestroy =>
            {
              if (inDestroy)
                return;
              this.EnableCurrentLineRenderer();
              currentLineRenderer = this.GetOrCreateLineRenderer();
            }), true);
          }
          else
          {
            this.EnableCurrentLineRenderer();
            currentLineRenderer = this.GetOrCreateLineRenderer();
          }
        }
        if (currentLineRenderer == null)
          return (Transform) null;
        currentLineRenderer.CustomTransform = parameters.CustomTransform;
        transform = currentLineRenderer.Transform;
      }
      for (int index = startGroupIndex; index < endGroupIndex; ++index)
      {
        LightningBoltSegmentGroup segmentGroup2 = this.segmentGroups[index];
        segmentGroup2.Delay = parameters.delaySeconds;
        segmentGroup2.LifeTime = parameters.LifeTime;
        segmentGroup2.PeakStart = segmentGroup2.LifeTime * parameters.FadePercent;
        segmentGroup2.PeakEnd = segmentGroup2.LifeTime - segmentGroup2.PeakStart;
        float num1 = segmentGroup2.PeakEnd - segmentGroup2.PeakStart;
        float num2 = segmentGroup2.LifeTime - segmentGroup2.PeakEnd;
        segmentGroup2.PeakStart *= parameters.FadeInMultiplier;
        segmentGroup2.PeakEnd = segmentGroup2.PeakStart + num1 * parameters.FadeFullyLitMultiplier;
        segmentGroup2.LifeTime = segmentGroup2.PeakEnd + num2 * parameters.FadeOutMultiplier;
        segmentGroup2.LightParameters = lp;
        this.RenderGroup(segmentGroup2, parameters);
      }
      if (this.dependencies.ThreadState.multiThreaded)
      {
        this.dependencies.ThreadState.AddActionForMainThread((Action<bool>) (inDestroy =>
        {
          if (inDestroy)
            return;
          this.RenderParticleSystems(start, end, parameters.TrunkWidth, parameters.LifeTime, parameters.delaySeconds);
          if (lp == null)
            return;
          this.CreateLightsForGroup(this.segmentGroups[startGroupIndex], lp, quality, parameters.maxLights);
        }));
      }
      else
      {
        this.RenderParticleSystems(start, end, parameters.TrunkWidth, parameters.LifeTime, parameters.delaySeconds);
        if (lp != null)
          this.CreateLightsForGroup(this.segmentGroups[startGroupIndex], lp, quality, parameters.maxLights);
      }
      return transform;
    }

    private void CreateLightsForGroup(
      LightningBoltSegmentGroup group,
      LightningLightParameters lp,
      LightningBoltQualitySetting quality,
      int maxLights)
    {
      if (LightningBolt.lightCount == LightningBolt.MaximumLightCount || maxLights <= 0)
        return;
      float num1 = (this.lifeTime - group.PeakEnd) * lp.FadeOutMultiplier;
      float num2 = (group.PeakEnd - group.PeakStart) * lp.FadeFullyLitMultiplier;
      float num3 = group.PeakStart * lp.FadeInMultiplier + num2 + num1;
      this.maxLifeTime = Mathf.Max(this.maxLifeTime, group.Delay + num3);
      this.segmentGroupsWithLight.Add(group);
      int segmentCount = group.SegmentCount;
      float num4;
      float num5;
      if (quality == LightningBoltQualitySetting.LimitToQualitySetting)
      {
        int qualityLevel = QualitySettings.GetQualityLevel();
        LightningQualityMaximum lightningQualityMaximum;
        if (LightningBoltParameters.QualityMaximums.TryGetValue(qualityLevel, out lightningQualityMaximum))
        {
          num4 = Mathf.Min(lp.LightPercent, lightningQualityMaximum.MaximumLightPercent);
          num5 = Mathf.Min(lp.LightShadowPercent, lightningQualityMaximum.MaximumShadowPercent);
        }
        else
        {
          Debug.LogError((object) ("Unable to read lightning quality for level " + qualityLevel.ToString()));
          num4 = lp.LightPercent;
          num5 = lp.LightShadowPercent;
        }
      }
      else
      {
        num4 = lp.LightPercent;
        num5 = lp.LightShadowPercent;
      }
      maxLights = Mathf.Max(1, Mathf.Min(maxLights, (int) ((double) segmentCount * (double) num4)));
      int nthLight = Mathf.Max(1, segmentCount / maxLights);
      int nthShadows = maxLights - (int) ((double) maxLights * (double) num5);
      int nthShadowCounter = nthShadows;
      int segmentIndex = group.StartIndex + (int) ((double) nthLight * 0.5);
      while (segmentIndex < group.Segments.Count && !this.AddLightToGroup(group, lp, segmentIndex, nthLight, nthShadows, ref maxLights, ref nthShadowCounter))
        segmentIndex += nthLight;
    }

    private bool AddLightToGroup(
      LightningBoltSegmentGroup group,
      LightningLightParameters lp,
      int segmentIndex,
      int nthLight,
      int nthShadows,
      ref int maxLights,
      ref int nthShadowCounter)
    {
      Light light = this.GetOrCreateLight(lp);
      group.Lights.Add(light);
      Vector3 vector3 = (group.Segments[segmentIndex].Start + group.Segments[segmentIndex].End) * 0.5f;
      if (this.dependencies.CameraIsOrthographic)
      {
        if (this.dependencies.CameraMode == CameraMode.OrthographicXZ)
          vector3.y = this.dependencies.CameraPos.y + lp.OrthographicOffset;
        else
          vector3.z = this.dependencies.CameraPos.z + lp.OrthographicOffset;
      }
      if (this.dependencies.UseWorldSpace)
        light.gameObject.transform.position = vector3;
      else
        light.gameObject.transform.localPosition = vector3;
      if ((double) lp.LightShadowPercent == 0.0 || ++nthShadowCounter < nthShadows)
      {
        light.shadows = LightShadows.None;
      }
      else
      {
        light.shadows = LightShadows.Soft;
        nthShadowCounter = 0;
      }
      return ++LightningBolt.lightCount == LightningBolt.MaximumLightCount || --maxLights == 0;
    }

    private Light GetOrCreateLight(LightningLightParameters lp)
    {
      Light light;
      while (LightningBolt.lightCache.Count != 0)
      {
        light = LightningBolt.lightCache[LightningBolt.lightCache.Count - 1];
        LightningBolt.lightCache.RemoveAt(LightningBolt.lightCache.Count - 1);
        if (!((UnityEngine.Object) light == (UnityEngine.Object) null))
          goto label_3;
      }
      light = new GameObject("LightningBoltLight").AddComponent<Light>();
      light.type = LightType.Point;
label_3:
      light.bounceIntensity = lp.BounceIntensity;
      light.shadowNormalBias = lp.ShadowNormalBias;
      light.color = lp.LightColor;
      light.renderMode = lp.RenderMode;
      light.range = lp.LightRange;
      light.shadowStrength = lp.ShadowStrength;
      light.shadowBias = lp.ShadowBias;
      light.intensity = 0.0f;
      light.gameObject.transform.parent = this.dependencies.Parent.transform;
      light.gameObject.SetActive(true);
      this.dependencies.LightAdded(light);
      return light;
    }

    private void UpdateLight(
      LightningLightParameters lp,
      IEnumerable<Light> lights,
      float delay,
      float peakStart,
      float peakEnd,
      float lifeTime)
    {
      if ((double) this.elapsedTime < (double) delay)
        return;
      float num1 = (lifeTime - peakEnd) * lp.FadeOutMultiplier;
      float num2 = (peakEnd - peakStart) * lp.FadeFullyLitMultiplier;
      peakStart *= lp.FadeInMultiplier;
      peakEnd = peakStart + num2;
      lifeTime = peakEnd + num1;
      float num3 = this.elapsedTime - delay;
      if ((double) num3 >= (double) peakStart)
      {
        if ((double) num3 <= (double) peakEnd)
        {
          foreach (Light light in lights)
            light.intensity = lp.LightIntensity * lp.LightMultiplier;
        }
        else
        {
          float t = (float) (((double) num3 - (double) peakEnd) / ((double) lifeTime - (double) peakEnd));
          foreach (Light light in lights)
            light.intensity = Mathf.Lerp(lp.LightIntensity * lp.LightMultiplier, 0.0f, t);
        }
      }
      else
      {
        float t = num3 / peakStart;
        foreach (Light light in lights)
          light.intensity = Mathf.Lerp(0.0f, lp.LightIntensity * lp.LightMultiplier, t);
      }
    }

    private void UpdateLights()
    {
      foreach (LightningBoltSegmentGroup boltSegmentGroup in this.segmentGroupsWithLight)
        this.UpdateLight(boltSegmentGroup.LightParameters, (IEnumerable<Light>) boltSegmentGroup.Lights, boltSegmentGroup.Delay, boltSegmentGroup.PeakStart, boltSegmentGroup.PeakEnd, boltSegmentGroup.LifeTime);
    }

    private IEnumerator GenerateParticleCoRoutine(ParticleSystem p, Vector3 pos, float delay)
    {
      yield return (object) new WaitForSecondsLightning(delay);
      p.transform.position = pos;
      ParticleSystem.EmissionModule emission = p.emission;
      if (emission.burstCount > 0)
      {
        emission = p.emission;
        ParticleSystem.Burst[] bursts = new ParticleSystem.Burst[emission.burstCount];
        emission = p.emission;
        emission.GetBursts(bursts);
        p.Emit(UnityEngine.Random.Range((int) bursts[0].minCount, (int) bursts[0].maxCount + 1));
      }
      else
      {
        emission = p.emission;
        ParticleSystem.MinMaxCurve rateOverTime = emission.rateOverTime;
        int minInclusive = (int) (((double) rateOverTime.constantMax - (double) rateOverTime.constantMin) * 0.5);
        p.Emit(UnityEngine.Random.Range(minInclusive, minInclusive * 2));
      }
    }

    private void CheckForGlow(IEnumerable<LightningBoltParameters> parameters)
    {
      foreach (LightningBoltParameters parameter in parameters)
      {
        this.HasGlow = (double) parameter.GlowIntensity >= (double) Mathf.Epsilon && (double) parameter.GlowWidthMultiplier >= (double) Mathf.Epsilon;
        if (this.HasGlow)
          break;
      }
    }

    public class LineRendererMesh
    {
      private const int defaultListCapacity = 2048;
      private static readonly Vector2 uv1 = new Vector2(0.0f, 0.0f);
      private static readonly Vector2 uv2 = new Vector2(1f, 0.0f);
      private static readonly Vector2 uv3 = new Vector2(0.0f, 1f);
      private static readonly Vector2 uv4 = new Vector2(1f, 1f);
      private readonly List<int> indices = new List<int>(2048);
      private readonly List<Vector3> vertices = new List<Vector3>(2048);
      private readonly List<Vector4> lineDirs = new List<Vector4>(2048);
      private readonly List<Color32> colors = new List<Color32>(2048);
      private readonly List<Vector3> ends = new List<Vector3>(2048);
      private readonly List<Vector4> texCoordsAndGlowModifiers = new List<Vector4>(2048);
      private readonly List<Vector4> fadeLifetimes = new List<Vector4>(2048);
      private const int boundsPadder = 1000000000;
      private int currentBoundsMinX = 1147483647;
      private int currentBoundsMinY = 1147483647;
      private int currentBoundsMinZ = 1147483647;
      private int currentBoundsMaxX = -1147483648;
      private int currentBoundsMaxY = -1147483648;
      private int currentBoundsMaxZ = -1147483648;
      private Mesh mesh;
      private MeshFilter meshFilterGlow;
      private MeshFilter meshFilterBolt;
      private MeshRenderer meshRendererGlow;
      private MeshRenderer meshRendererBolt;

      public GameObject GameObject { get; private set; }

      public Material MaterialGlow
      {
        get => this.meshRendererGlow.sharedMaterial;
        set => this.meshRendererGlow.sharedMaterial = value;
      }

      public Material MaterialBolt
      {
        get => this.meshRendererBolt.sharedMaterial;
        set => this.meshRendererBolt.sharedMaterial = value;
      }

      public MeshRenderer MeshRendererGlow => this.meshRendererGlow;

      public MeshRenderer MeshRendererBolt => this.meshRendererBolt;

      public int Tag { get; set; }

      public Action<LightningCustomTransformStateInfo> CustomTransform { get; set; }

      public Transform Transform { get; private set; }

      public bool Empty => this.vertices.Count == 0;

      public LineRendererMesh(LightningBoltDependencies dependencies)
      {
        dependencies.ThreadState.AddActionForMainThread((Action<bool>) (b =>
        {
          this.GameObject = new GameObject("LightningBoltMeshRenderer");
          this.GameObject.SetActive(false);
          this.mesh = new Mesh()
          {
            name = "ProceduralLightningMesh"
          };
          this.mesh.MarkDynamic();
          GameObject gameObject1 = new GameObject("LightningBoltMeshRendererGlow");
          gameObject1.transform.parent = this.GameObject.transform;
          GameObject gameObject2 = new GameObject("LightningBoltMeshRendererBolt");
          gameObject2.transform.parent = this.GameObject.transform;
          this.meshFilterGlow = gameObject1.AddComponent<MeshFilter>();
          this.meshFilterBolt = gameObject2.AddComponent<MeshFilter>();
          this.meshFilterGlow.sharedMesh = this.meshFilterBolt.sharedMesh = this.mesh;
          this.meshRendererGlow = gameObject1.AddComponent<MeshRenderer>();
          this.meshRendererBolt = gameObject2.AddComponent<MeshRenderer>();
          this.meshRendererGlow.shadowCastingMode = this.meshRendererBolt.shadowCastingMode = ShadowCastingMode.Off;
          this.meshRendererGlow.reflectionProbeUsage = this.meshRendererBolt.reflectionProbeUsage = ReflectionProbeUsage.Off;
          this.meshRendererGlow.lightProbeUsage = this.meshRendererBolt.lightProbeUsage = LightProbeUsage.Off;
          this.meshRendererGlow.receiveShadows = this.meshRendererBolt.receiveShadows = false;
          this.Transform = this.GameObject.GetComponent<Transform>();
        }), true);
      }

      public void PopulateMesh()
      {
        if (this.vertices.Count == 0)
          this.mesh.Clear();
        else
          this.PopulateMeshInternal();
      }

      public bool PrepareForLines(int lineCount) => this.vertices.Count + lineCount * 4 <= 64999;

      public void BeginLine(
        Vector3 start,
        Vector3 end,
        float radius,
        Color32 color,
        float colorIntensity,
        Vector4 fadeLifeTime,
        float glowWidthModifier,
        float glowIntensity)
      {
        Vector4 vector4 = (Vector4) (end - start) with
        {
          w = radius
        };
        this.AppendLineInternal(ref start, ref end, ref vector4, ref vector4, ref vector4, color, colorIntensity, ref fadeLifeTime, glowWidthModifier, glowIntensity);
      }

      public void AppendLine(
        Vector3 start,
        Vector3 end,
        float radius,
        Color32 color,
        float colorIntensity,
        Vector4 fadeLifeTime,
        float glowWidthModifier,
        float glowIntensity)
      {
        Vector4 dir = (Vector4) (end - start) with
        {
          w = radius
        };
        Vector4 lineDir1 = this.lineDirs[this.lineDirs.Count - 3];
        Vector4 lineDir2 = this.lineDirs[this.lineDirs.Count - 1];
        this.AppendLineInternal(ref start, ref end, ref dir, ref lineDir1, ref lineDir2, color, colorIntensity, ref fadeLifeTime, glowWidthModifier, glowIntensity);
      }

      public void Reset()
      {
        this.CustomTransform = (Action<LightningCustomTransformStateInfo>) null;
        ++this.Tag;
        this.GameObject.SetActive(false);
        this.mesh.Clear();
        this.indices.Clear();
        this.vertices.Clear();
        this.colors.Clear();
        this.lineDirs.Clear();
        this.ends.Clear();
        this.texCoordsAndGlowModifiers.Clear();
        this.fadeLifetimes.Clear();
        this.currentBoundsMaxX = this.currentBoundsMaxY = this.currentBoundsMaxZ = -1147483648;
        this.currentBoundsMinX = this.currentBoundsMinY = this.currentBoundsMinZ = 1147483647;
      }

      private void PopulateMeshInternal()
      {
        this.GameObject.SetActive(true);
        this.mesh.SetVertices(this.vertices);
        this.mesh.SetTangents(this.lineDirs);
        this.mesh.SetColors(this.colors);
        this.mesh.SetUVs(0, this.texCoordsAndGlowModifiers);
        this.mesh.SetUVs(1, this.fadeLifetimes);
        this.mesh.SetNormals(this.ends);
        this.mesh.SetTriangles(this.indices, 0);
        Bounds bounds = new Bounds();
        Vector3 vector3_1 = new Vector3((float) (this.currentBoundsMinX - 2), (float) (this.currentBoundsMinY - 2), (float) (this.currentBoundsMinZ - 2));
        Vector3 vector3_2 = new Vector3((float) (this.currentBoundsMaxX + 2), (float) (this.currentBoundsMaxY + 2), (float) (this.currentBoundsMaxZ + 2));
        bounds.center = (vector3_2 + vector3_1) * 0.5f;
        bounds.size = (vector3_2 - vector3_1) * 1.2f;
        this.mesh.bounds = bounds;
      }

      private void UpdateBounds(ref Vector3 point1, ref Vector3 point2)
      {
        int num1 = (int) point1.x - (int) point2.x;
        int num2 = num1 & num1 >> 31;
        int num3 = (int) point2.x + num2;
        int num4 = (int) point1.x - num2;
        int num5 = this.currentBoundsMinX - num3;
        int num6 = num5 & num5 >> 31;
        this.currentBoundsMinX = num3 + num6;
        int num7 = this.currentBoundsMaxX - num4;
        this.currentBoundsMaxX -= num7 & num7 >> 31;
        int num8 = (int) point1.y - (int) point2.y;
        int num9 = num8 & num8 >> 31;
        int num10 = (int) point2.y + num9;
        int num11 = (int) point1.y - num9;
        int num12 = this.currentBoundsMinY - num10;
        int num13 = num12 & num12 >> 31;
        this.currentBoundsMinY = num10 + num13;
        int num14 = this.currentBoundsMaxY - num11;
        this.currentBoundsMaxY -= num14 & num14 >> 31;
        int num15 = (int) point1.z - (int) point2.z;
        int num16 = num15 & num15 >> 31;
        int num17 = (int) point2.z + num16;
        int num18 = (int) point1.z - num16;
        int num19 = this.currentBoundsMinZ - num17;
        int num20 = num19 & num19 >> 31;
        this.currentBoundsMinZ = num17 + num20;
        int num21 = this.currentBoundsMaxZ - num18;
        this.currentBoundsMaxZ -= num21 & num21 >> 31;
      }

      private void AddIndices()
      {
        int count = this.vertices.Count;
        List<int> indices1 = this.indices;
        int num1 = count;
        int num2 = num1 + 1;
        indices1.Add(num1);
        List<int> indices2 = this.indices;
        int num3 = num2;
        int num4 = num3 + 1;
        indices2.Add(num3);
        this.indices.Add(num4);
        List<int> indices3 = this.indices;
        int num5 = num4;
        int num6 = num5 - 1;
        indices3.Add(num5);
        this.indices.Add(num6);
        int num7;
        this.indices.Add(num7 = num6 + 2);
      }

      private void AppendLineInternal(
        ref Vector3 start,
        ref Vector3 end,
        ref Vector4 dir,
        ref Vector4 dirPrev1,
        ref Vector4 dirPrev2,
        Color32 color,
        float colorIntensity,
        ref Vector4 fadeLifeTime,
        float glowWidthModifier,
        float glowIntensity)
      {
        this.AddIndices();
        color.a = (byte) Mathf.Lerp(0.0f, (float) byte.MaxValue, colorIntensity * 0.1f);
        Vector4 vector4 = new Vector4(LightningBolt.LineRendererMesh.uv1.x, LightningBolt.LineRendererMesh.uv1.y, glowWidthModifier, glowIntensity);
        this.vertices.Add(start);
        this.lineDirs.Add(dirPrev1);
        this.colors.Add(color);
        this.ends.Add((Vector3) dir);
        this.vertices.Add(end);
        this.lineDirs.Add(dir);
        this.colors.Add(color);
        this.ends.Add((Vector3) dir);
        dir.w = -dir.w;
        this.vertices.Add(start);
        this.lineDirs.Add(dirPrev2);
        this.colors.Add(color);
        this.ends.Add((Vector3) dir);
        this.vertices.Add(end);
        this.lineDirs.Add(dir);
        this.colors.Add(color);
        this.ends.Add((Vector3) dir);
        this.texCoordsAndGlowModifiers.Add(vector4);
        vector4.x = LightningBolt.LineRendererMesh.uv2.x;
        vector4.y = LightningBolt.LineRendererMesh.uv2.y;
        this.texCoordsAndGlowModifiers.Add(vector4);
        vector4.x = LightningBolt.LineRendererMesh.uv3.x;
        vector4.y = LightningBolt.LineRendererMesh.uv3.y;
        this.texCoordsAndGlowModifiers.Add(vector4);
        vector4.x = LightningBolt.LineRendererMesh.uv4.x;
        vector4.y = LightningBolt.LineRendererMesh.uv4.y;
        this.texCoordsAndGlowModifiers.Add(vector4);
        this.fadeLifetimes.Add(fadeLifeTime);
        this.fadeLifetimes.Add(fadeLifeTime);
        this.fadeLifetimes.Add(fadeLifeTime);
        this.fadeLifetimes.Add(fadeLifeTime);
        this.UpdateBounds(ref start, ref end);
      }
    }
  }
}
