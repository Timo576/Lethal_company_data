// Decompiled with JetBrains decompiler
// Type: DigitalRuby.ThunderAndLightning.LightningWhipScript
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System.Collections;
using UnityEngine;

#nullable disable
namespace DigitalRuby.ThunderAndLightning
{
  [RequireComponent(typeof (AudioSource))]
  public class LightningWhipScript : MonoBehaviour
  {
    public AudioClip WhipCrack;
    public AudioClip WhipCrackThunder;
    private AudioSource audioSource;
    private GameObject whipStart;
    private GameObject whipEndStrike;
    private GameObject whipHandle;
    private GameObject whipSpring;
    private Vector2 prevDrag;
    private bool dragging;
    private bool canWhip = true;

    private IEnumerator WhipForward()
    {
      if (this.canWhip)
      {
        this.canWhip = false;
        for (int index = 0; index < this.whipStart.transform.childCount; ++index)
        {
          Rigidbody2D component = this.whipStart.transform.GetChild(index).gameObject.GetComponent<Rigidbody2D>();
          if ((Object) component != (Object) null)
            component.drag = 0.0f;
        }
        this.audioSource.PlayOneShot(this.WhipCrack);
        this.whipSpring.GetComponent<SpringJoint2D>().enabled = true;
        this.whipSpring.GetComponent<Rigidbody2D>().position = this.whipHandle.GetComponent<Rigidbody2D>().position + new Vector2(-15f, 5f);
        yield return (object) new WaitForSecondsLightning(0.2f);
        this.whipSpring.GetComponent<Rigidbody2D>().position = this.whipHandle.GetComponent<Rigidbody2D>().position + new Vector2(15f, 2.5f);
        yield return (object) new WaitForSecondsLightning(0.15f);
        this.audioSource.PlayOneShot(this.WhipCrackThunder, 0.5f);
        yield return (object) new WaitForSecondsLightning(0.15f);
        this.whipEndStrike.GetComponent<ParticleSystem>().Play();
        this.whipSpring.GetComponent<SpringJoint2D>().enabled = false;
        yield return (object) new WaitForSecondsLightning(0.65f);
        for (int index = 0; index < this.whipStart.transform.childCount; ++index)
        {
          Rigidbody2D component = this.whipStart.transform.GetChild(index).gameObject.GetComponent<Rigidbody2D>();
          if ((Object) component != (Object) null)
          {
            component.velocity = Vector2.zero;
            component.drag = 0.5f;
          }
        }
        this.canWhip = true;
      }
    }

    private void Start()
    {
      this.whipStart = GameObject.Find("WhipStart");
      this.whipEndStrike = GameObject.Find("WhipEndStrike");
      this.whipHandle = GameObject.Find("WhipHandle");
      this.whipSpring = GameObject.Find("WhipSpring");
      this.audioSource = this.GetComponent<AudioSource>();
    }

    private void Update()
    {
      if (!this.dragging && Input.GetMouseButtonDown(0))
      {
        Vector2 worldPoint = (Vector2) Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D collider2D = Physics2D.OverlapPoint(worldPoint);
        if ((Object) collider2D != (Object) null && (Object) collider2D.gameObject == (Object) this.whipHandle)
        {
          this.dragging = true;
          this.prevDrag = worldPoint;
        }
      }
      else if (this.dragging && Input.GetMouseButton(0))
      {
        Vector2 worldPoint = (Vector2) Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 vector2 = worldPoint - this.prevDrag;
        Rigidbody2D component = this.whipHandle.GetComponent<Rigidbody2D>();
        component.MovePosition(component.position + vector2);
        this.prevDrag = worldPoint;
      }
      else
        this.dragging = false;
      if (!Input.GetKeyDown(KeyCode.Space))
        return;
      this.StartCoroutine(this.WhipForward());
    }
  }
}
