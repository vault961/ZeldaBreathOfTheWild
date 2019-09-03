using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class GetMagnet : InteractComponent
{
    //public GameObject Player;
    public Collider col_player;

    public GameObject LoadingPanel;
    public GameObject MainPanel;
    public Image LoadingImage;
    public ParticleSystem EmitParticle;

    //TerminalComponent
    public GameObject terminal;
    public Material terminalMat;
    public ParticleSystem dropParticle;

    public float animTime = 2.0f;
    private float start = 0.0f;
    private float end = 1.0f;
    private float time = 0f;

    private bool isPlaying = false;

    new void Start()
    {
        player = InventorySystem.instance.player;
        player.GetComponent<MagnetController>().getMagnet = false;
        col_player = player.GetComponent<Collider>();
        EmitParticle = GetComponentInChildren<ParticleSystem>();
        EmitParticle.Stop();

        terminalMat = terminal.GetComponentInChildren<Renderer>().material;
        dropParticle = terminal.GetComponentInChildren<ParticleSystem>();
        dropParticle.Stop();

        // UI
        LoadingPanel.SetActive(false);
        MainPanel.SetActive(false);

        LoadingImage = LoadingPanel.GetComponent<Image>();
    }

    public override void Interact()
    {      
        if (player.GetComponent<MagnetController>().getMagnet == false)
        {
            player.GetComponent<MagnetController>().getMagnet = true;
            StartCoroutine("TerminalFadeIn");

        }
    }

    public void StartFadeAnim()
    {
        // 중복 재생 방지
        if (isPlaying == true)
            return;

        if (player.GetComponent<MagnetController>().getMagnet == true)
            StartCoroutine("PlayFadeOut");
    }

    IEnumerator PlayFadeOut()
    {
        isPlaying = true;

        Color color = LoadingImage.color;
        time = 0;
        color.a = Mathf.Lerp(start, end, time);

        while (color.a < 1f)
        {
            time += Time.deltaTime / animTime;
            color.a = Mathf.Lerp(start, end, time);
            LoadingImage.color = color;

            yield return null;
        }

        MainPanel.SetActive(true);
        LoadingPanel.SetActive(false);

        color = MainPanel.GetComponent<Image>().color;
        color.a = Mathf.Lerp(start, end, time);

        while (time > 0f)
        {
            time -= Time.deltaTime / animTime;
            //color.a = Mathf.Lerp(start, end, time);
            //MainPanel.GetComponent<Image>().color = color;

            yield return null;
        }

        terminalMat.SetFloat("_Burn", 1.0f);

        MainPanel.SetActive(false);

        isPlaying = false;
    }

    IEnumerator TerminalFadeIn()
    {
        // Terminal Mat FadeIN
        float amount = 1.0f;
        while (amount > -1.0f)
        {
            terminalMat.SetFloat("_Burn", amount);
            amount -= 0.1f;

            if (amount == 0.65)
            {
                yield return new WaitForSeconds(3.0f);
            }
            yield return new WaitForSeconds(0.01f);
        }

        dropParticle.Play();
        yield return new WaitForSeconds(3.0f);
        EmitParticle.Play();
        yield return new WaitForSeconds(2.0f);


        // Panel 루틴 실행
        dropParticle.Stop();
        LoadingPanel.SetActive(true);
        StartFadeAnim();
    }
}
