// Decompiled with JetBrains decompiler
// Type: DigitalRuby.ThunderAndLightning.ThunderAndLightningScript
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System.Collections;
using UnityEngine;

#nullable disable
namespace DigitalRuby.ThunderAndLightning
{
  public class ThunderAndLightningScript : MonoBehaviour
  {
    [Tooltip("Lightning bolt script - optional, leave null if you don't want lightning bolts")]
    public LightningBoltPrefabScript LightningBoltScript;
    [Tooltip("Camera where the lightning should be centered over. Defaults to main camera.")]
    public Camera Camera;
    [SingleLine("Random interval between strikes.")]
    public RangeOfFloats LightningIntervalTimeRange = new RangeOfFloats()
    {
      Minimum = 10f,
      Maximum = 25f
    };
    [Tooltip("Probability (0-1) of an intense lightning bolt that hits really close. Intense lightning has increased brightness and louder thunder compared to normal lightning, and the thunder sounds plays a lot sooner.")]
    [Range(0.0f, 1f)]
    public float LightningIntenseProbability = 0.2f;
    [Tooltip("Sounds to play for normal thunder. One will be chosen at random for each lightning strike. Depending on intensity, some normal lightning may not play a thunder sound.")]
    public AudioClip[] ThunderSoundsNormal;
    [Tooltip("Sounds to play for intense thunder. One will be chosen at random for each lightning strike.")]
    public AudioClip[] ThunderSoundsIntense;
    [Tooltip("Whether lightning strikes should always try to be in the camera view")]
    public bool LightningAlwaysVisible = true;
    [Tooltip("The chance lightning will simply be in the clouds with no visible bolt")]
    [Range(0.0f, 1f)]
    public float CloudLightningChance = 0.5f;
    [Tooltip("Whether to modify the skybox exposure when lightning is created")]
    public bool ModifySkyboxExposure;
    [Tooltip("Base point light range for lightning bolts. Increases as intensity increases.")]
    [Range(1f, 10000f)]
    public float BaseLightRange = 2000f;
    [Tooltip("Starting y value for the lightning strikes")]
    [Range(0.0f, 100000f)]
    public float LightningYStart = 500f;
    [Tooltip("Volume multiplier")]
    [Range(0.0f, 1f)]
    public float VolumeMultiplier = 1f;
    private float skyboxExposureOriginal;
    private float skyboxExposureStorm;
    private float nextLightningTime;
    private bool lightningInProgress;
    private AudioSource audioSourceThunder;
    private ThunderAndLightningScript.LightningBoltHandler lightningBoltHandler;
    private Material skyboxMaterial;
    private AudioClip lastThunderSound;

    private void Start()
    {
      this.EnableLightning = true;
      if ((UnityEngine.Object) this.Camera == (UnityEngine.Object) null)
        this.Camera = Camera.main;
      if ((UnityEngine.Object) RenderSettings.skybox != (UnityEngine.Object) null)
        this.skyboxMaterial = RenderSettings.skybox = new Material(RenderSettings.skybox);
      this.skyboxExposureOriginal = this.skyboxExposureStorm = (UnityEngine.Object) this.skyboxMaterial == (UnityEngine.Object) null || !this.skyboxMaterial.HasProperty("_Exposure") ? 1f : this.skyboxMaterial.GetFloat("_Exposure");
      this.audioSourceThunder = this.gameObject.AddComponent<AudioSource>();
      this.lightningBoltHandler = new ThunderAndLightningScript.LightningBoltHandler(this);
      this.lightningBoltHandler.VolumeMultiplier = this.VolumeMultiplier;
    }

    private void Update()
    {
      if (this.lightningBoltHandler == null || !this.EnableLightning)
        return;
      this.lightningBoltHandler.VolumeMultiplier = this.VolumeMultiplier;
      this.lightningBoltHandler.Update();
    }

    public void CallNormalLightning() => this.CallNormalLightning(new Vector3?(), new Vector3?());

    public void CallNormalLightning(Vector3? start, Vector3? end)
    {
      this.StartCoroutine(this.lightningBoltHandler.ProcessLightning(start, end, false, true));
    }

    public void CallIntenseLightning() => this.CallIntenseLightning(new Vector3?(), new Vector3?());

    public void CallIntenseLightning(Vector3? start, Vector3? end)
    {
      this.StartCoroutine(this.lightningBoltHandler.ProcessLightning(start, end, true, true));
    }

    public float SkyboxExposureOriginal => this.skyboxExposureOriginal;

    public bool EnableLightning { get; set; }

    private class LightningBoltHandler
    {
      private ThunderAndLightningScript script;
      private readonly System.Random random = new System.Random();

      public float VolumeMultiplier { get; set; }

      public LightningBoltHandler(ThunderAndLightningScript script)
      {
        this.script = script;
        this.CalculateNextLightningTime();
      }

      private void UpdateLighting()
      {
        if (this.script.lightningInProgress)
          return;
        if (this.script.ModifySkyboxExposure)
        {
          this.script.skyboxExposureStorm = 0.35f;
          if ((UnityEngine.Object) this.script.skyboxMaterial != (UnityEngine.Object) null && this.script.skyboxMaterial.HasProperty("_Exposure"))
            this.script.skyboxMaterial.SetFloat("_Exposure", this.script.skyboxExposureStorm);
        }
        this.CheckForLightning();
      }

      private void CalculateNextLightningTime()
      {
        this.script.nextLightningTime = DigitalRuby.ThunderAndLightning.LightningBoltScript.TimeSinceStart + this.script.LightningIntervalTimeRange.Random(this.random);
        this.script.lightningInProgress = false;
        if (!this.script.ModifySkyboxExposure || !this.script.skyboxMaterial.HasProperty("_Exposure"))
          return;
        this.script.skyboxMaterial.SetFloat("_Exposure", this.script.skyboxExposureStorm);
      }

      public IEnumerator ProcessLightning(
        Vector3? _start,
        Vector3? _end,
        bool intense,
        bool visible)
      {
        this.script.lightningInProgress = true;
        AudioClip[] sounds;
        float intensity;
        float time;
        if (intense)
        {
          intensity = Mathf.Lerp(2f, 8f, UnityEngine.Random.Range(0.0f, 1f));
          time = 5f / intensity;
          sounds = this.script.ThunderSoundsIntense;
        }
        else
        {
          intensity = Mathf.Lerp(0.0f, 2f, UnityEngine.Random.Range(0.0f, 1f));
          time = 30f / intensity;
          sounds = this.script.ThunderSoundsNormal;
        }
        if ((UnityEngine.Object) this.script.skyboxMaterial != (UnityEngine.Object) null && this.script.ModifySkyboxExposure)
          this.script.skyboxMaterial.SetFloat("_Exposure", Mathf.Max(intensity * 0.5f, this.script.skyboxExposureStorm));
        this.Strike(_start, _end, intense, intensity, this.script.Camera, visible ? this.script.Camera : (Camera) null);
        this.CalculateNextLightningTime();
        if ((double) intensity >= 1.0 && sounds != null && sounds.Length != 0)
        {
          yield return (object) new WaitForSecondsLightning(time);
          AudioClip clip;
          do
          {
            clip = sounds[UnityEngine.Random.Range(0, sounds.Length - 1)];
          }
          while (sounds.Length > 1 && (UnityEngine.Object) clip == (UnityEngine.Object) this.script.lastThunderSound);
          this.script.lastThunderSound = clip;
          this.script.audioSourceThunder.PlayOneShot(clip, intensity * 0.5f * this.VolumeMultiplier);
        }
      }

      private void Strike(
        Vector3? _start,
        Vector3? _end,
        bool intense,
        float intensity,
        Camera camera,
        Camera visibleInCamera)
      {
        float minInclusive1 = intense ? -1000f : -5000f;
        float maxInclusive = intense ? 1000f : 5000f;
        float minInclusive2 = intense ? 500f : 2500f;
        float num1 = UnityEngine.Random.Range(0, 2) == 0 ? UnityEngine.Random.Range(minInclusive1, -minInclusive2) : UnityEngine.Random.Range(minInclusive2, maxInclusive);
        float lightningYstart = this.script.LightningYStart;
        float num2 = UnityEngine.Random.Range(0, 2) == 0 ? UnityEngine.Random.Range(minInclusive1, -minInclusive2) : UnityEngine.Random.Range(minInclusive2, maxInclusive);
        Vector3 origin = this.script.Camera.transform.position;
        origin.x += num1;
        origin.y = lightningYstart;
        origin.z += num2;
        if ((UnityEngine.Object) visibleInCamera != (UnityEngine.Object) null)
        {
          Quaternion rotation = visibleInCamera.transform.rotation;
          visibleInCamera.transform.rotation = Quaternion.Euler(0.0f, rotation.eulerAngles.y, 0.0f);
          float x = UnityEngine.Random.Range((float) visibleInCamera.pixelWidth * 0.1f, (float) visibleInCamera.pixelWidth * 0.9f);
          float z = UnityEngine.Random.Range(visibleInCamera.nearClipPlane + minInclusive2 + minInclusive2, maxInclusive);
          origin = visibleInCamera.ScreenToWorldPoint(new Vector3(x, 0.0f, z)) with
          {
            y = lightningYstart
          };
          visibleInCamera.transform.rotation = rotation;
        }
        Vector3 vector3 = origin;
        float num3 = UnityEngine.Random.Range(-100f, 100f);
        float num4 = UnityEngine.Random.Range(0, 4) == 0 ? UnityEngine.Random.Range(-1f, 600f) : -1f;
        float num5 = num2 + UnityEngine.Random.Range(-100f, 100f);
        vector3.x += num3;
        vector3.y = num4;
        vector3.z += num5;
        vector3.x += minInclusive2 * camera.transform.forward.x;
        vector3.z += minInclusive2 * camera.transform.forward.z;
        while ((double) (origin - vector3).magnitude < 500.0)
        {
          vector3.x += minInclusive2 * camera.transform.forward.x;
          vector3.z += minInclusive2 * camera.transform.forward.z;
        }
        Vector3? nullable = _start;
        origin = nullable ?? origin;
        nullable = _end;
        vector3 = nullable ?? vector3;
        RaycastHit hitInfo;
        if (Physics.Raycast(origin, (origin - vector3).normalized, out hitInfo, float.MaxValue))
          vector3 = hitInfo.point;
        int generations = this.script.LightningBoltScript.Generations;
        RangeOfFloats trunkWidthRange = this.script.LightningBoltScript.TrunkWidthRange;
        if ((double) UnityEngine.Random.value < (double) this.script.CloudLightningChance)
        {
          this.script.LightningBoltScript.TrunkWidthRange = new RangeOfFloats();
          this.script.LightningBoltScript.Generations = 1;
        }
        this.script.LightningBoltScript.LightParameters.LightIntensity = intensity * 0.5f;
        this.script.LightningBoltScript.Trigger(new Vector3?(origin), new Vector3?(vector3));
        this.script.LightningBoltScript.TrunkWidthRange = trunkWidthRange;
        this.script.LightningBoltScript.Generations = generations;
      }

      private void CheckForLightning()
      {
        if ((double) Time.time < (double) this.script.nextLightningTime)
          return;
        this.script.StartCoroutine(this.ProcessLightning(new Vector3?(), new Vector3?(), (double) UnityEngine.Random.value < (double) this.script.LightningIntenseProbability, this.script.LightningAlwaysVisible));
      }

      public void Update() => this.UpdateLighting();
    }
  }
}
