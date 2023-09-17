using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SpawnState
{
    Ready,
    Wait
}

public class GameManager : MonoBehaviour
{
    Color LIGHT_RED = new Color(1, 89f / 255, 74f / 255);
    Color LIGHT_ORANGE = new Color(1, 174f / 255, 74f / 255);

    private Util util;
    private UIManager uiManager;

    public GameObject blockPrefab;
    public GameObject blockFragmentPrefab;
    public ParticleSystem rippleEffectPrefab;
    public GameObject[] blocks;
    public GameObject[] blockFragments;
    public ParticleSystem[] rippleEffects;
    public ParticleSystem sparkEffect;
    private Camera mainCamera;
    public GameObject background;

    private MeshRenderer[] blockRenderers;
    private Color[] blockColors;
    private MeshRenderer[] blockFragmentRenderers;
    private Rigidbody[] blockFragmentRigibodies;

    float xMax = 2.5f;
    float zMin = -2.5f;
    public Vector3 currentScale;
    private int score = 0;
    private int streak = 0;
    private bool isGameOver = false;

    public SpawnState spawnState;

    private void Awake()
    {
        util = GameObject.Find("Util").GetComponent<Util>();
        uiManager = GameObject.Find("UI Manager").GetComponent<UIManager>();
    }

    // Start is called before the first frame update
    void Start()
    {
        blockRenderers = new MeshRenderer[blocks.Length];
        blockColors = new Color[blocks.Length];
        blockFragmentRenderers = new MeshRenderer[blockFragments.Length];
        blockFragmentRigibodies = new Rigidbody[blockFragments.Length];

        for (int i = 0; i < blocks.Length; i++)
        {
            blocks[i] = Instantiate(blockPrefab);
            blockRenderers[i] = blocks[i].GetComponent<MeshRenderer>();
            blocks[i].GetComponent<Block>().blockIndex = i;
            blocks[i].SetActive(false);

            blockFragments[i] = Instantiate(blockFragmentPrefab);
            blockFragmentRenderers[i] = blockFragments[i].GetComponent<MeshRenderer>();
            blockFragmentRigibodies[i] = blockFragments[i].GetComponent<Rigidbody>();
            blockFragments[i].SetActive(false);
        }

        for (int i = 0; i < rippleEffects.Length; i++)
        {
            rippleEffects[i] = Instantiate(rippleEffectPrefab);
            rippleEffects[i].gameObject.SetActive(false);
        }
        Destroy(rippleEffectPrefab.gameObject);

        mainCamera = Camera.main;

        currentScale = blockPrefab.transform.localScale;
        spawnState = SpawnState.Ready;
    }

    public void StartGame()
    {
        StartCoroutine(SpawnBlock());
    }

    IEnumerator SpawnBlock()
    {
        Random.InitState(System.DateTime.Now.Millisecond);
        Color bottomColor = new Color(
            Random.Range(0f, 1f),
            Random.Range(0f, 1f),
            Random.Range(0f, 1f),
            Random.Range(0f, 1f)
        );
        Color topColor = new Color(
            Random.Range(0f, 1f),
            Random.Range(0f, 1f),
            Random.Range(0f, 1f),
            Random.Range(0f, 1f)
        );

        background.GetComponent<MeshRenderer>().material.SetColor("_Color1", bottomColor - 0.2f * Color.white);
        background.GetComponent<MeshRenderer>().material.SetColor("_Color1", topColor - 0.2f * Color.white);

        while (!isGameOver)
        {
            for (int i = 0; i < blocks.Length; i++)
            {
                if (spawnState == SpawnState.Wait)
                {
                    break;
                }

                if (!blocks[i].activeInHierarchy)
                {
                    Vector3 spawnPosition;

                    spawnPosition.x = i % 2 == 0 ? -10 : blocks[i - 1].transform.position.x;
                    spawnPosition.y = 1.05f * blockRenderers[0].bounds.size.y * i;
                    if (i == 0)
                    {
                        spawnPosition.z = 0;
                    }
                    else
                    {
                        spawnPosition.z = i % 2 == 0 ? blocks[i - 1].transform.position.z : 10;
                    }

                    blocks[i].transform.localScale = currentScale;
                    blocks[i].transform.position = spawnPosition;
                    blockColors[i] = Color.Lerp(bottomColor, topColor, (float)i / blocks.Length);
                    util.SetMaterialColor(blockRenderers[i], blockColors[i]);
                    blocks[i].SetActive(true);

                    Vector3 end;

                    end.x = i == 0 ? 0 : blocks[i - 1].transform.position.x;
                    end.y = blocks[i].transform.position.y;
                    end.z = i == 0 ? 0 : blocks[i - 1].transform.position.z;

                    if (i % 2 == 0)
                    {
                        StartCoroutine(blocks[i].GetComponent<Block>().Slide(SlideDirection.X, end));
                    }
                    else
                    {
                        StartCoroutine(blocks[i].GetComponent<Block>().Slide(SlideDirection.Z, end));
                    }

                    /*StartCoroutine(MoveCamera());*/

                    spawnState = SpawnState.Wait;

                    break;
                }
            }

            yield return new WaitForSeconds(1f);
        }
    }

    public void SpawnBlockFragment(int blockIndex, SlideDirection slideDirection)
    {
        Vector3 initialBlockSize = blockRenderers[blockIndex].bounds.size;

        Vector3 stopPosition;

        if (slideDirection == SlideDirection.X)
        {
            stopPosition = new Vector3(
                blocks[blockIndex].transform.position.x + 0.5f * initialBlockSize.x,
                blocks[blockIndex].transform.position.y,
                blocks[blockIndex].transform.position.z
            );
        }
        else
        {
            stopPosition = new Vector3(
                blocks[blockIndex].transform.position.x,
                blocks[blockIndex].transform.position.y,
                blocks[blockIndex].transform.position.z - 0.5f * initialBlockSize.z
            );
        }

        // check game over
        if (slideDirection == SlideDirection.X && Mathf.Abs(xMax - stopPosition.x) > initialBlockSize.x)
        {
            uiManager.SetGameOverTMP();
            isGameOver = true;
        }
        if (slideDirection == SlideDirection.Z && Mathf.Abs(stopPosition.z - zMin) > initialBlockSize.z)
        {
            uiManager.SetGameOverTMP();
            isGameOver = true;
        }
        if (isGameOver)
        {
            Rigidbody blockRigidbody = blocks[blockIndex].GetComponent<Rigidbody>();
            blockRigidbody.mass = 1;
            blockRigidbody.useGravity = true;
            blockRigidbody.isKinematic = false;
            return;
        }
        //

        AddScore();

        // Perfect
        if (Mathf.Abs(stopPosition.x - xMax) < 0.3f || Mathf.Abs(stopPosition.z - zMin) < 0.3f)
        {
            blocks[blockIndex].transform.position = new Vector3(
                xMax - 0.5f * initialBlockSize.x,
                blocks[blockIndex].transform.position.y,
                zMin + 0.5f * initialBlockSize.z
            );

            if (streak < 5)
            {
                streak++;
            }

            if (streak < 5)
            {
                StartCoroutine(util.ColorShiftEffect(blockRenderers[blockIndex], blockColors[blockIndex]));

               /* if (currentScale.x > 0.3f && currentScale.z > 0.3f)
                {
                    StartCoroutine(StartRippleEffect(blocks[blockIndex].transform.position));
                }*/
            }
            else
            {
                if (currentScale.x > 0.3f && currentScale.z > 0.3f)
                {
                    StartSparkEffect(blocks[blockIndex].transform.position);
                }
            }

            return;
        }
        else
        {
            streak = 0;
        }

        for (int i = 0; i < blockFragments.Length; i++)
        {
            if (!blockFragments[i].activeInHierarchy)
            {
                Vector3 initialBlockFragmentSize = blockFragmentRenderers[i].bounds.size;

                Vector3 newBlockLocalScale;
                Vector3 newBlockPosition;
                Vector3 newBlockFragmentLocalScale;
                Vector3 newBlockFragmentPosition;

                if (slideDirection == SlideDirection.X)
                {
                    newBlockLocalScale.x = blocks[blockIndex].transform.localScale.x
                        * (initialBlockSize.x - Mathf.Abs(xMax - stopPosition.x)) / initialBlockSize.x;
                    newBlockLocalScale.z = currentScale.z;
                }
                else
                {
                    newBlockLocalScale.x = currentScale.x;
                    newBlockLocalScale.z = blocks[blockIndex].transform.localScale.z
                        * (initialBlockSize.z - Mathf.Abs(stopPosition.z - zMin)) / initialBlockSize.z;
                }
                newBlockLocalScale.y = blocks[blockIndex].transform.localScale.y;

                blocks[blockIndex].transform.localScale = newBlockLocalScale;



                if (slideDirection == SlideDirection.X)
                {
                    if (stopPosition.x < xMax)
                    {
                        newBlockPosition.x = stopPosition.x - 0.5f * blockRenderers[blockIndex].bounds.size.x;
                    }
                    else
                    {
                        newBlockPosition.x = xMax - 0.5f * blockRenderers[blockIndex].bounds.size.x;
                    }
                    newBlockPosition.z = stopPosition.z;
                }
                else
                {
                    newBlockPosition.x = stopPosition.x;
                    if (stopPosition.z > zMin)
                    {
                        newBlockPosition.z = stopPosition.z + 0.5f * blockRenderers[blockIndex].bounds.size.z;
                    }
                    else
                    {
                        newBlockPosition.z = zMin + 0.5f * blockRenderers[blockIndex].bounds.size.z;
                    }
                }
                newBlockPosition.y = blocks[blockIndex].transform.position.y;

                blocks[blockIndex].transform.position = newBlockPosition;



                if (slideDirection == SlideDirection.X)
                {
                    newBlockFragmentLocalScale.x = blockFragments[blockIndex].transform.localScale.x
                        * (initialBlockSize.x - blockRenderers[blockIndex].bounds.size.x) / initialBlockFragmentSize.x;
                    newBlockFragmentLocalScale.z = currentScale.z;
                }
                else
                {
                    newBlockFragmentLocalScale.x = currentScale.x;
                    newBlockFragmentLocalScale.z = blockFragments[blockIndex].transform.localScale.z
                        * (initialBlockSize.z - blockRenderers[blockIndex].bounds.size.z) / initialBlockFragmentSize.z;
                }
                newBlockFragmentLocalScale.y = blockFragments[blockIndex].transform.localScale.y;

                blockFragments[i].transform.localScale = newBlockFragmentLocalScale;



                if (slideDirection == SlideDirection.X)
                {
                    if (stopPosition.x < xMax)
                    {
                        newBlockFragmentPosition.x = stopPosition.x -
                        blockRenderers[blockIndex].bounds.size.x - blockFragmentRenderers[i].bounds.size.x;
                    }
                    else
                    {
                        newBlockFragmentPosition.x = xMax + blockFragmentRenderers[i].bounds.size.x;
                    }
                    newBlockFragmentPosition.z = stopPosition.z;
                }
                else
                {
                    newBlockFragmentPosition.x = stopPosition.x;
                    if (stopPosition.z > zMin)
                    {
                        newBlockFragmentPosition.z = stopPosition.z +
                        blockRenderers[blockIndex].bounds.size.z + blockFragmentRenderers[i].bounds.size.z;
                    }
                    else
                    {
                        newBlockFragmentPosition.z = zMin - blockFragmentRenderers[i].bounds.size.z;
                    }
                }
                newBlockFragmentPosition.y = stopPosition.y;

                util.SetMaterialColor(
                    blockFragmentRenderers[i], blockColors[blockIndex] - new Color(0.15f, 0.15f, 0.15f));
                blockFragments[i].transform.position = newBlockFragmentPosition;
                blockFragments[i].SetActive(true);
                blockFragmentRigibodies[i].AddForce(new Vector3(1, -5, 1), ForceMode.Impulse);



                if (slideDirection == SlideDirection.X)
                {
                    if (stopPosition.x < xMax)
                    {
                        xMax = stopPosition.x;
                    }
                }
                else
                {
                    if (stopPosition.z > zMin)
                    {
                        zMin = stopPosition.z;
                    }
                }

                currentScale = blocks[blockIndex].transform.localScale;

                break;
            }
        }
    }

    public IEnumerator StartRippleEffect(Vector3 spawnPosition)
    {
        for (int i = 0; i < streak; i++)
        {
            rippleEffects[i].transform.localScale = (1.2f / 5) * currentScale + new Vector3(currentScale.x / 13, 0, currentScale.x / 13);
            rippleEffects[i].transform.position = spawnPosition;

            /* var particleSystemMain = rippleEffects[i].main;
             particleSystemMain.startSizeXMultiplier = currentScale.x * 1f;
             particleSystemMain.startSizeYMultiplier = currentScale.z * 1f;
             particleSystemMain.startSizeZMultiplier = 1;*/

            if (streak > 1)
            {
                var sz = rippleEffects[i].sizeOverLifetime;
                sz.enabled = true;
            }
            else
            {
                var sz = rippleEffects[i].sizeOverLifetime;
                sz.enabled = false;
            }

            rippleEffects[i].gameObject.SetActive(true);

            rippleEffects[i].Play();

            yield return new WaitForSeconds(0.35f);
        }
    }

    public void StartSparkEffect(Vector3 spawnPosition)
    {
        rippleEffects[0].transform.localScale = (1.2f / 5) * currentScale + new Vector3(currentScale.x / 13, 0, currentScale.x / 13);
        rippleEffects[0].transform.position = spawnPosition;

        /*var particleSystemMain = rippleEffects[0].main;
        particleSystemMain.startSizeXMultiplier = currentScale.x * 1f;
        particleSystemMain.startSizeYMultiplier = currentScale.z * 1f;
        particleSystemMain.startSizeZMultiplier = 1;*/

        rippleEffects[0].gameObject.SetActive(true);

        rippleEffects[0].Play();

        sparkEffect.transform.position = spawnPosition;
        sparkEffect.Play();
    }

    public IEnumerator MoveCamera()
    {
        Vector3 end = Vector3.zero;

        for (int i = blocks.Length - 1; i >= 0; i--)
        {
            if (blocks[i].activeInHierarchy)
            {
                end = new Vector3(
                    blocks[i].transform.position.x + 6,
                    mainCamera.transform.position.y + 1.05f * blockRenderers[0].bounds.size.y,
                    blocks[i].transform.position.z - 6
                );

                break;
            }
        }

        float percent = 0;
        float deltaPercent = 1f / 13;

        while (mainCamera.transform.position.y < end.y)
        {
            percent += deltaPercent;

            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, end, percent);

            yield return new WaitForSeconds(0.02f);
        }

        mainCamera.transform.position = end;
    }

    void AddScore()
    {
        score++;
        uiManager.SetScoreTMP(score);
    }
}
