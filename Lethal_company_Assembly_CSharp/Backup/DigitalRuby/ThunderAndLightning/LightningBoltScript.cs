// Decompiled with JetBrains decompiler
// Type: DigitalRuby.ThunderAndLightning.LightningBoltScript
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

#nullable disable
namespace DigitalRuby.ThunderAndLightning
{
  public class LightningBoltScript : MonoBehaviour
  {
    [Header("Lightning General Properties")]
    [Tooltip("The camera the lightning should be shown in. Defaults to the current camera, or the main camera if current camera is null. If you are using a different camera, you may want to put the lightning in it's own layer and cull that layer out of any other cameras.")]
    public Camera Camera;
    [Tooltip("Type of camera mode. Auto detects the camera and creates appropriate lightning. Can be overriden to do something more specific regardless of camera.")]
    public CameraMode CameraMode;
    internal CameraMode calculatedCameraMode = CameraMode.Unknown;
    [Tooltip("True if you are using world space coordinates for the lightning bolt, false if you are using coordinates relative to the parent game object.")]
    public bool UseWorldSpace = true;
    [Tooltip("Whether to compensate for the parent transform. Default is false. If true, rotation, scale and position are altered by the parent transform. Use this to fix scaling, rotation and other offset problems with the lightning.")]
    public bool CompensateForParentTransform;
    [Tooltip("Lightning quality setting. This allows setting limits on generations, lights and shadow casting lights based on the global quality setting.")]
    public LightningBoltQualitySetting QualitySetting;
    [Tooltip("Whether to use multi-threaded generation of lightning. Lightning will be delayed by about 1 frame if this is turned on, but this can significantly improve performance.")]
    public bool MultiThreaded;
    [Range(0.0f, 1000f)]
    [Tooltip("If non-zero, the Camera property is used to get distance of lightning from camera. Lightning generations is reduced for each distance from camera. For example, if LevelOfDetailDistance was 100 and the lightning was 200 away from camera, generations would be reduced by 2, to a minimum of 1.")]
    public float LevelOfDetailDistance;
    [Tooltip("True to use game time, false to use real time")]
    public bool UseGameTime;
    [Header("Lightning 2D Settings")]
    [Tooltip("Sort layer name")]
    public string SortLayerName;
    [Tooltip("Order in sort layer")]
    public int SortOrderInLayer;
    [Header("Lightning Rendering Properties")]
    [Tooltip("Soft particles factor. 0.01 to 3.0 are typical, 100.0 to disable.")]
    [Range(0.01f, 100f)]
    public float SoftParticlesFactor = 3f;
    [Tooltip("The render queue for the lightning. -1 for default.")]
    public int RenderQueue = -1;
    [Tooltip("Lightning material for mesh renderer - glow")]
    public Material LightningMaterialMesh;
    [Tooltip("Lightning material for mesh renderer - bolt")]
    public Material LightningMaterialMeshNoGlow;
    [Tooltip("The texture to use for the lightning bolts, or null for the material default texture.")]
    public Texture2D LightningTexture;
    [Tooltip("The texture to use for the lightning glow, or null for the material default texture.")]
    public Texture2D LightningGlowTexture;
    [Tooltip("Particle system to play at the point of emission (start). 'Emission rate' particles will be emitted all at once.")]
    public ParticleSystem LightningOriginParticleSystem;
    [Tooltip("Particle system to play at the point of impact (end). 'Emission rate' particles will be emitted all at once.")]
    public ParticleSystem LightningDestinationParticleSystem;
    [Tooltip("Tint color for the lightning")]
    public Color LightningTintColor = Color.white;
    [Tooltip("Tint color for the lightning glow")]
    public Color GlowTintColor = new Color(0.1f, 0.2f, 1f, 1f);
    [Tooltip("Allow tintint the main trunk differently than forks.")]
    public Color MainTrunkTintColor = new Color(1f, 1f, 1f, 1f);
    [Tooltip("Source blend mode. Default is SrcAlpha.")]
    public BlendMode SourceBlendMode = BlendMode.SrcAlpha;
    [Tooltip("Destination blend mode. Default is One. For additive blend use One. For alpha blend use OneMinusSrcAlpha.")]
    public BlendMode DestinationBlendMode = BlendMode.One;
    [Tooltip("Source blend mode. Default is SrcAlpha.")]
    public BlendMode SourceBlendModeGlow = BlendMode.SrcAlpha;
    [Tooltip("Destination blend mode. Default is One. For additive blend use One. For alpha blend use OneMinusSrcAlpha.")]
    public BlendMode DestinationBlendModeGlow = BlendMode.One;
    [Header("Lightning Movement Properties")]
    [Tooltip("Jitter multiplier to randomize lightning size. Jitter depends on trunk width and will make the lightning move rapidly and jaggedly, giving a more lively and sometimes cartoony feel. Jitter may be shared with other bolts depending on materials. If you need different jitters for the same material, create a second script object.")]
    public float JitterMultiplier;
    [Tooltip("Built in turbulance based on the direction of each segment. Small values usually work better, like 0.2.")]
    public float Turbulence;
    [Tooltip("Global turbulence velocity for this script")]
    public Vector3 TurbulenceVelocity = Vector3.zero;
    [Tooltip("Flicker intensity, causes lightning to pop in and out rapidly. X = intensity, Y = speed.")]
    public Vector2 IntensityFlicker = new Vector2(0.0f, 64f);
    public static float TimeScale = 1f;
    private static bool needsTimeUpdate = true;
    private Texture2D lastLightningTexture;
    private Texture2D lastLightningGlowTexture;
    private readonly List<LightningBolt> activeBolts = new List<LightningBolt>();
    private readonly LightningBoltParameters[] oneParameterArray = new LightningBoltParameters[1];
    private readonly List<LightningBolt> lightningBoltCache = new List<LightningBolt>();
    private readonly List<LightningBoltDependencies> dependenciesCache = new List<LightningBoltDependencies>();
    private LightningThreadState threadState;
    private static int shaderId_MainTex = int.MinValue;
    private static int shaderId_TintColor;
    private static int shaderId_JitterMultiplier;
    private static int shaderId_Turbulence;
    private static int shaderId_TurbulenceVelocity;
    private static int shaderId_SrcBlendMode;
    private static int shaderId_DstBlendMode;
    private static int shaderId_InvFade;
    private static int shaderId_LightningTime;
    private static int shaderId_IntensityFlicker;
    private static int shaderId_RenderMode;

    public Action<LightningBoltParameters, Vector3, Vector3> LightningStartedCallback { get; set; }

    public Action<LightningBoltParameters, Vector3, Vector3> LightningEndedCallback { get; set; }

    public Action<Light> LightAddedCallback { get; set; }

    public Action<Light> LightRemovedCallback { get; set; }

    public bool HasActiveBolts => this.activeBolts.Count != 0;

    public static Vector4 TimeVectorSinceStart { get; private set; }

    public static float TimeSinceStart { get; private set; }

    public static float DeltaTime { get; private set; }

    public virtual void CreateLightningBolt(LightningBoltParameters p)
    {
      if (p == null || !((UnityEngine.Object) this.Camera != (UnityEngine.Object) null))
        return;
      this.UpdateTexture();
      this.oneParameterArray[0] = p;
      this.GetOrCreateLightningBolt().SetupLightningBolt(this.CreateLightningBoltDependencies((ICollection<LightningBoltParameters>) this.oneParameterArray));
    }

    public void CreateLightningBolts(ICollection<LightningBoltParameters> parameters)
    {
      if (parameters == null || parameters.Count == 0 || !((UnityEngine.Object) this.Camera != (UnityEngine.Object) null))
        return;
      this.UpdateTexture();
      this.GetOrCreateLightningBolt().SetupLightningBolt(this.CreateLightningBoltDependencies(parameters));
    }

    protected virtual void Awake() => this.UpdateShaderIds();

    protected virtual void Start()
    {
      this.UpdateCamera();
      this.UpdateMaterialsForLastTexture();
      this.UpdateShaderParameters();
      this.CheckCompensateForParentTransform();
      SceneManager.sceneLoaded += new UnityAction<Scene, LoadSceneMode>(this.OnSceneLoaded);
      this.threadState = new LightningThreadState(this.MultiThreaded);
    }

    protected virtual void Update()
    {
      if ((double) Time.timeScale <= 0.0)
        return;
      if (LightningBoltScript.needsTimeUpdate)
      {
        LightningBoltScript.needsTimeUpdate = false;
        LightningBoltScript.DeltaTime = (this.UseGameTime ? Time.deltaTime : Time.unscaledDeltaTime) * LightningBoltScript.TimeScale;
        LightningBoltScript.TimeSinceStart += LightningBoltScript.DeltaTime;
      }
      if (this.HasActiveBolts)
      {
        this.UpdateCamera();
        this.UpdateShaderParameters();
        this.CheckCompensateForParentTransform();
        this.UpdateActiveBolts();
        Shader.SetGlobalVector(LightningBoltScript.shaderId_LightningTime, LightningBoltScript.TimeVectorSinceStart = new Vector4(LightningBoltScript.TimeSinceStart * 0.05f, LightningBoltScript.TimeSinceStart, LightningBoltScript.TimeSinceStart * 2f, LightningBoltScript.TimeSinceStart * 3f));
      }
      this.threadState.UpdateMainThreadActions();
    }

    protected virtual void LateUpdate() => LightningBoltScript.needsTimeUpdate = true;

    protected virtual LightningBoltParameters OnCreateParameters()
    {
      return LightningBoltParameters.GetOrCreateParameters();
    }

    protected LightningBoltParameters CreateParameters()
    {
      LightningBoltParameters parameters = this.OnCreateParameters();
      parameters.quality = this.QualitySetting;
      this.PopulateParameters(parameters);
      return parameters;
    }

    protected virtual void PopulateParameters(LightningBoltParameters parameters)
    {
      parameters.MainTrunkTintColor = (Color32) this.MainTrunkTintColor;
    }

    internal Material lightningMaterialMeshInternal { get; private set; }

    internal Material lightningMaterialMeshNoGlowInternal { get; private set; }

    private Coroutine StartCoroutineWrapper(IEnumerator routine)
    {
      return this.isActiveAndEnabled ? this.StartCoroutine(routine) : (Coroutine) null;
    }

    private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1) => LightningBolt.ClearCache();

    private LightningBoltDependencies CreateLightningBoltDependencies(
      ICollection<LightningBoltParameters> parameters)
    {
      LightningBoltDependencies boltDependencies;
      if (this.dependenciesCache.Count == 0)
      {
        boltDependencies = new LightningBoltDependencies();
        boltDependencies.AddActiveBolt = new Action<LightningBolt>(this.AddActiveBolt);
        boltDependencies.LightAdded = new Action<Light>(this.OnLightAdded);
        boltDependencies.LightRemoved = new Action<Light>(this.OnLightRemoved);
        boltDependencies.ReturnToCache = new Action<LightningBoltDependencies>(this.ReturnLightningDependenciesToCache);
        boltDependencies.StartCoroutine = new Func<IEnumerator, Coroutine>(this.StartCoroutineWrapper);
        boltDependencies.Parent = this.gameObject;
      }
      else
      {
        int index = this.dependenciesCache.Count - 1;
        boltDependencies = this.dependenciesCache[index];
        this.dependenciesCache.RemoveAt(index);
      }
      boltDependencies.CameraPos = this.Camera.transform.position;
      boltDependencies.CameraIsOrthographic = this.Camera.orthographic;
      boltDependencies.CameraMode = this.calculatedCameraMode;
      boltDependencies.LevelOfDetailDistance = this.LevelOfDetailDistance;
      boltDependencies.DestParticleSystem = this.LightningDestinationParticleSystem;
      boltDependencies.LightningMaterialMesh = this.lightningMaterialMeshInternal;
      boltDependencies.LightningMaterialMeshNoGlow = this.lightningMaterialMeshNoGlowInternal;
      boltDependencies.OriginParticleSystem = this.LightningOriginParticleSystem;
      boltDependencies.SortLayerName = this.SortLayerName;
      boltDependencies.SortOrderInLayer = this.SortOrderInLayer;
      boltDependencies.UseWorldSpace = this.UseWorldSpace;
      boltDependencies.ThreadState = this.threadState;
      boltDependencies.Parameters = !this.threadState.multiThreaded ? parameters : (ICollection<LightningBoltParameters>) new List<LightningBoltParameters>((IEnumerable<LightningBoltParameters>) parameters);
      boltDependencies.LightningBoltStarted = this.LightningStartedCallback;
      boltDependencies.LightningBoltEnded = this.LightningEndedCallback;
      return boltDependencies;
    }

    private void ReturnLightningDependenciesToCache(LightningBoltDependencies d)
    {
      d.Parameters = (ICollection<LightningBoltParameters>) null;
      d.OriginParticleSystem = (ParticleSystem) null;
      d.DestParticleSystem = (ParticleSystem) null;
      d.LightningMaterialMesh = (Material) null;
      d.LightningMaterialMeshNoGlow = (Material) null;
      this.dependenciesCache.Add(d);
    }

    internal void OnLightAdded(Light l)
    {
      if (this.LightAddedCallback == null)
        return;
      this.LightAddedCallback(l);
    }

    internal void OnLightRemoved(Light l)
    {
      if (this.LightRemovedCallback == null)
        return;
      this.LightRemovedCallback(l);
    }

    internal void AddActiveBolt(LightningBolt bolt) => this.activeBolts.Add(bolt);

    private void UpdateShaderIds()
    {
      if (LightningBoltScript.shaderId_MainTex != int.MinValue)
        return;
      LightningBoltScript.shaderId_MainTex = Shader.PropertyToID("_MainTex");
      LightningBoltScript.shaderId_TintColor = Shader.PropertyToID("_TintColor");
      LightningBoltScript.shaderId_JitterMultiplier = Shader.PropertyToID("_JitterMultiplier");
      LightningBoltScript.shaderId_Turbulence = Shader.PropertyToID("_Turbulence");
      LightningBoltScript.shaderId_TurbulenceVelocity = Shader.PropertyToID("_TurbulenceVelocity");
      LightningBoltScript.shaderId_SrcBlendMode = Shader.PropertyToID("_SrcBlendMode");
      LightningBoltScript.shaderId_DstBlendMode = Shader.PropertyToID("_DstBlendMode");
      LightningBoltScript.shaderId_InvFade = Shader.PropertyToID("_InvFade");
      LightningBoltScript.shaderId_LightningTime = Shader.PropertyToID("_LightningTime");
      LightningBoltScript.shaderId_IntensityFlicker = Shader.PropertyToID("_IntensityFlicker");
      LightningBoltScript.shaderId_RenderMode = Shader.PropertyToID("_RenderMode");
    }

    private void UpdateMaterialsForLastTexture()
    {
      if (!Application.isPlaying)
        return;
      this.calculatedCameraMode = CameraMode.Unknown;
      this.lightningMaterialMeshInternal = new Material(this.LightningMaterialMesh);
      this.lightningMaterialMeshNoGlowInternal = new Material(this.LightningMaterialMeshNoGlow);
      if ((UnityEngine.Object) this.LightningTexture != (UnityEngine.Object) null)
        this.lightningMaterialMeshNoGlowInternal.SetTexture(LightningBoltScript.shaderId_MainTex, (Texture) this.LightningTexture);
      if ((UnityEngine.Object) this.LightningGlowTexture != (UnityEngine.Object) null)
        this.lightningMaterialMeshInternal.SetTexture(LightningBoltScript.shaderId_MainTex, (Texture) this.LightningGlowTexture);
      this.SetupMaterialCamera();
    }

    private void UpdateTexture()
    {
      if ((UnityEngine.Object) this.LightningTexture != (UnityEngine.Object) null && (UnityEngine.Object) this.LightningTexture != (UnityEngine.Object) this.lastLightningTexture)
      {
        this.lastLightningTexture = this.LightningTexture;
        this.UpdateMaterialsForLastTexture();
      }
      if (!((UnityEngine.Object) this.LightningGlowTexture != (UnityEngine.Object) null) || !((UnityEngine.Object) this.LightningGlowTexture != (UnityEngine.Object) this.lastLightningGlowTexture))
        return;
      this.lastLightningGlowTexture = this.LightningGlowTexture;
      this.UpdateMaterialsForLastTexture();
    }

    private void SetMaterialPerspective()
    {
      if (this.calculatedCameraMode == CameraMode.Perspective)
        return;
      this.calculatedCameraMode = CameraMode.Perspective;
      this.lightningMaterialMeshInternal.SetInt(LightningBoltScript.shaderId_RenderMode, 0);
      this.lightningMaterialMeshNoGlowInternal.SetInt(LightningBoltScript.shaderId_RenderMode, 0);
    }

    private void SetMaterialOrthographicXY()
    {
      if (this.calculatedCameraMode == CameraMode.OrthographicXY)
        return;
      this.calculatedCameraMode = CameraMode.OrthographicXY;
      this.lightningMaterialMeshInternal.SetInt(LightningBoltScript.shaderId_RenderMode, 1);
      this.lightningMaterialMeshNoGlowInternal.SetInt(LightningBoltScript.shaderId_RenderMode, 1);
    }

    private void SetMaterialOrthographicXZ()
    {
      if (this.calculatedCameraMode == CameraMode.OrthographicXZ)
        return;
      this.calculatedCameraMode = CameraMode.OrthographicXZ;
      this.lightningMaterialMeshInternal.SetInt(LightningBoltScript.shaderId_RenderMode, 2);
      this.lightningMaterialMeshNoGlowInternal.SetInt(LightningBoltScript.shaderId_RenderMode, 2);
    }

    private void SetupMaterialCamera()
    {
      if ((UnityEngine.Object) this.Camera == (UnityEngine.Object) null && this.CameraMode == CameraMode.Auto)
        this.SetMaterialPerspective();
      else if (this.CameraMode == CameraMode.Auto)
      {
        if (this.Camera.orthographic)
          this.SetMaterialOrthographicXY();
        else
          this.SetMaterialPerspective();
      }
      else if (this.CameraMode == CameraMode.Perspective)
        this.SetMaterialPerspective();
      else if (this.CameraMode == CameraMode.OrthographicXY)
        this.SetMaterialOrthographicXY();
      else
        this.SetMaterialOrthographicXZ();
    }

    private void UpdateShaderParameters()
    {
      this.lightningMaterialMeshInternal.SetColor(LightningBoltScript.shaderId_TintColor, this.GlowTintColor);
      this.lightningMaterialMeshInternal.SetFloat(LightningBoltScript.shaderId_JitterMultiplier, this.JitterMultiplier);
      this.lightningMaterialMeshInternal.SetFloat(LightningBoltScript.shaderId_Turbulence, this.Turbulence * LightningBoltParameters.Scale);
      this.lightningMaterialMeshInternal.SetVector(LightningBoltScript.shaderId_TurbulenceVelocity, (Vector4) (this.TurbulenceVelocity * LightningBoltParameters.Scale));
      this.lightningMaterialMeshInternal.SetInt(LightningBoltScript.shaderId_SrcBlendMode, (int) this.SourceBlendModeGlow);
      this.lightningMaterialMeshInternal.SetInt(LightningBoltScript.shaderId_DstBlendMode, (int) this.DestinationBlendModeGlow);
      this.lightningMaterialMeshInternal.renderQueue = this.RenderQueue;
      this.lightningMaterialMeshInternal.SetFloat(LightningBoltScript.shaderId_InvFade, this.SoftParticlesFactor);
      this.lightningMaterialMeshNoGlowInternal.SetColor(LightningBoltScript.shaderId_TintColor, this.LightningTintColor);
      this.lightningMaterialMeshNoGlowInternal.SetFloat(LightningBoltScript.shaderId_JitterMultiplier, this.JitterMultiplier);
      this.lightningMaterialMeshNoGlowInternal.SetFloat(LightningBoltScript.shaderId_Turbulence, this.Turbulence * LightningBoltParameters.Scale);
      this.lightningMaterialMeshNoGlowInternal.SetVector(LightningBoltScript.shaderId_TurbulenceVelocity, (Vector4) (this.TurbulenceVelocity * LightningBoltParameters.Scale));
      this.lightningMaterialMeshNoGlowInternal.SetInt(LightningBoltScript.shaderId_SrcBlendMode, (int) this.SourceBlendMode);
      this.lightningMaterialMeshNoGlowInternal.SetInt(LightningBoltScript.shaderId_DstBlendMode, (int) this.DestinationBlendMode);
      this.lightningMaterialMeshNoGlowInternal.renderQueue = this.RenderQueue;
      this.lightningMaterialMeshNoGlowInternal.SetFloat(LightningBoltScript.shaderId_InvFade, this.SoftParticlesFactor);
      this.lightningMaterialMeshInternal.SetVector(LightningBoltScript.shaderId_IntensityFlicker, (Vector4) this.IntensityFlicker);
      this.lightningMaterialMeshNoGlowInternal.SetVector(LightningBoltScript.shaderId_IntensityFlicker, (Vector4) this.IntensityFlicker);
      this.SetupMaterialCamera();
    }

    private void CheckCompensateForParentTransform()
    {
      if (!this.CompensateForParentTransform)
        return;
      Transform parent = this.transform.parent;
      if (!((UnityEngine.Object) parent != (UnityEngine.Object) null))
        return;
      this.transform.position = parent.position;
      this.transform.localScale = new Vector3(1f / parent.localScale.x, 1f / parent.localScale.y, 1f / parent.localScale.z);
      this.transform.rotation = parent.rotation;
    }

    private void UpdateCamera()
    {
      this.Camera = (UnityEngine.Object) this.Camera == (UnityEngine.Object) null ? ((UnityEngine.Object) Camera.current == (UnityEngine.Object) null ? Camera.main : Camera.current) : this.Camera;
    }

    private LightningBolt GetOrCreateLightningBolt()
    {
      if (this.lightningBoltCache.Count == 0)
        return new LightningBolt();
      LightningBolt lightningBolt = this.lightningBoltCache[this.lightningBoltCache.Count - 1];
      this.lightningBoltCache.RemoveAt(this.lightningBoltCache.Count - 1);
      return lightningBolt;
    }

    private void UpdateActiveBolts()
    {
      for (int index = this.activeBolts.Count - 1; index >= 0; --index)
      {
        LightningBolt activeBolt = this.activeBolts[index];
        if (!activeBolt.Update())
        {
          this.activeBolts.RemoveAt(index);
          activeBolt.Cleanup();
          this.lightningBoltCache.Add(activeBolt);
        }
      }
    }

    private void OnApplicationQuit()
    {
      if (!this.threadState.multiThreaded)
        return;
      this.threadState.Running = false;
    }

    private void Cleanup()
    {
      foreach (LightningBolt activeBolt in this.activeBolts)
        activeBolt.Cleanup();
      this.activeBolts.Clear();
    }

    private void OnDestroy()
    {
      if (this.threadState.multiThreaded)
        this.threadState.TerminateAndWaitForEnd(true);
      if ((UnityEngine.Object) this.lightningMaterialMeshInternal != (UnityEngine.Object) null)
        UnityEngine.Object.Destroy((UnityEngine.Object) this.lightningMaterialMeshInternal);
      if ((UnityEngine.Object) this.lightningMaterialMeshNoGlowInternal != (UnityEngine.Object) null)
        UnityEngine.Object.Destroy((UnityEngine.Object) this.lightningMaterialMeshNoGlowInternal);
      this.Cleanup();
    }

    private void OnDisable() => this.Cleanup();
  }
}
