using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public TMP_Text scoreTMP;
    public RectTransform startMenuRT;
    public TMP_Text titleTMP;
    public TMP_Text tapToStartTMP;

    public GameManager gameManager;

    private Vector2 screenSize;

    // Start is called before the first frame update
    void Start()
    {
        screenSize.x = Screen.currentResolution.width;
        screenSize.y = Screen.currentResolution.height;

        StartCoroutine(CheckStartGame());
    }

    public void SetScoreTMP(int score)
    {
        scoreTMP.text = score.ToString();
    }

    public void SetGameOverTMP()
    {
        scoreTMP.text = "Game Over";
    }

    IEnumerator CheckStartGame()
    {
        while(true)
        {
            if(Input.GetMouseButton(0))
            {
                startMenuRT.gameObject.SetActive(false);
                scoreTMP.gameObject.SetActive(true);

                yield return new WaitForSeconds(0.5f);

                gameManager.StartGame();

                break;
            }

            yield return new WaitForSeconds(0.05f);
        }
    }
}
