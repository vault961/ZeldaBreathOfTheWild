using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;

public class BrokenBarrier : MonoBehaviour {

    public GameObject Pikachu;
    public Transform playerTr;
    public float amount;
    public Material[] materials;

    public Transform cameraTr;
    public GameObject barriers;
    public Camera goalCamera;

    public ParticleSystem brokenParticle;
    public ParticleSystem disapearParticle;
    public GameObject OrbParticle;

    // 대사
    public Color textColor;
    List<string> texs = new List<string>();
    public Text tex;

    private void Start()
    {
        disapearParticle = Pikachu.GetComponentInChildren<ParticleSystem>();
        materials = Pikachu.GetComponentInChildren<SkinnedMeshRenderer>().materials;
        amount = 0.05f;

        goalCamera.enabled = false;
        brokenParticle.Stop();
        disapearParticle.Stop();

        Instantiate(OrbParticle, Pikachu.transform.position, Quaternion.identity);
        OrbParticle.SetActive(false);

        textColor = tex.color;  // 기존 색 저장
        Dialog();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {

            //if (Input.GetButtonDown("AButton"))
            //{
            //Camera.main.enabled = false;
                playerTr = other.transform;
                goalCamera.enabled = true;

                StartCoroutine("PlayGoalAnim");
            //}
        }
    }

    void Dialog()
    {
        texs.Add("시련을 극복하고 이곳에 도달한 자···\n그대야말로 의심할 여지없는 용사");
        texs.Add("나는 머나먼 옛날\n여신 하일리아의 계시를 받은 자이자\n시련의 사당을 창조한 자인 <color=#FFFF00>피카츄</color> ");
        texs.Add("영겁의 시간 속에서\n드디어 진정한 용사가 나타났으니···\n여신 하일리아의 이름으로 극복의 증표를 내리노라···");
        texs.Add("이것으로 나의 사명은 다하였으니···\n여신 하일리아의 가호가 함께하길");
    }

    IEnumerator PlayGoalAnim()
    { 

        yield return new WaitForSeconds(1.0f);

        goalCamera.transform.position = cameraTr.position;
        goalCamera.transform.rotation = Quaternion.Euler(0, -90.0f, 0);  

        yield return new WaitForSeconds(1.5f);
        barriers.SetActive(false);
        brokenParticle.Play();

        yield return new WaitForSeconds(1.5f);
        goalCamera.transform.position = new Vector3(-51.45f, 34.89f, -9.82f);

        //Text Play()
        for (int i = 0; i < texs.Count - 1; i++)
        {
            tex.color = textColor; // 알파가 바뀐 값은 기존의 색으로 변경 
            //Color col = textColor;

            tex.text = texs[i];

            //for (float j = 1f; j > 0; j -= 0.1f)
            //{
            //    col.a = j;
            //    //col = new Vector4(textColor.r, textColor.g, textColor.b, j);
            //    tex.color = col;
            //}

            if (i == 1)
            {
                Vector3 startPos = goalCamera.transform.position;
                Vector3 changePos = new Vector3(-51.783f, 34.756f, -11.65f);
                float elapsedTime = 0.0f;
                Quaternion startRot = goalCamera.transform.rotation;
                Quaternion targetRot = Quaternion.Euler(0, -67.389f, 0);
                while (elapsedTime < 1.0f)
                {
                    //Quaternion changeRot = new Quaternion ()
                    goalCamera.transform.position = Vector3.Lerp(startPos, changePos, elapsedTime/1.0f);
                    goalCamera.transform.rotation = Quaternion.Slerp(startRot, targetRot, elapsedTime/1.0f);
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
            }

            yield return new WaitForSeconds(3.0f);
        }


        goalCamera.transform.position = cameraTr.position;

        StartCoroutine("GiveOrb");

        // 대사 끝
        tex.enabled = false;

        brokenParticle.Stop();

        StartCoroutine("Dissolve");
        yield return null;   
    }

    IEnumerator Dissolve()
    {
        goalCamera.transform.position = new Vector3(-51.45f, 34.89f, -9.82f);
        goalCamera.transform.rotation = Quaternion.Euler(0, -90.0f, 0);

        disapearParticle.Play();

        float value = 0.0f;
        while (value < 1.0f)
        {
            foreach (var mat in materials)
            {
                mat.SetFloat("_SliceAmount", value);
                value += 0.003f;
            }
            yield return new WaitForSeconds(0.4f);
        }

        disapearParticle.Stop();
        goalCamera.enabled = false;
        yield return new WaitForSeconds(4.0f);

        LoadingSceneManager.LoadScene("Credit");
    }

    IEnumerator GiveOrb()
    {
        OrbParticle.SetActive(true);
        OrbParticle.transform.position = Vector3.Lerp(OrbParticle.transform.position, playerTr.position, 1.0f * Time.deltaTime);
        
        if(OrbParticle.transform.position == playerTr.position)
            yield return null;
    }

}
