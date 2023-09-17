using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SlideDirection
{
    X,
    Z
}

public class Block : MonoBehaviour
{
    private GameManager gameManager;

    private MeshRenderer blockRenderer;

    public int blockIndex;

    private void Awake()
    {
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();

        blockRenderer = GetComponent<MeshRenderer>();
    }

    public IEnumerator Slide(SlideDirection slideDirection, Vector3 end)
    {
        int step = 0;
        float range;

        range = slideDirection == SlideDirection.X ?
            end.x + 1.2f * blockRenderer.bounds.size.x : end.z + 1.2f * blockRenderer.bounds.size.z;

        while (true)
        {
            if (Input.GetMouseButton(0))
            {
                gameManager.SpawnBlockFragment(blockIndex, slideDirection);

                StartCoroutine(gameManager.MoveCamera());

                // wait some time before spawning new block
                yield return new WaitForSeconds(0.5f);

                gameManager.spawnState = SpawnState.Ready;

                break;
            }

            if (slideDirection == SlideDirection.X)
            {
                if (step % 2 == 0)
                {
                    if (transform.position.x < range)
                    {
                        transform.position = Vector3.MoveTowards(transform.position
                            , new Vector3(range, end.y, end.z)
                            , 0.2f);
                    }
                    else
                    {
                        step++;
                    }
                }
                else
                {
                    if (transform.position.x > -range)
                    {
                        transform.position = Vector3.MoveTowards(transform.position
                            , new Vector3(-range, end.y, end.z)
                            , 0.2f);
                    }
                    else
                    {
                        step++;
                    }
                }
            }
            else if (slideDirection == SlideDirection.Z)
            {
                if (step % 2 == 0)
                {
                    if (transform.position.z < range)
                    {
                        transform.position = Vector3.MoveTowards(transform.position
                            , new Vector3(end.x, end.y, range)
                            , 0.3f);
                    }
                    else
                    {
                        step++;
                    }
                }
                else
                {
                    if (transform.position.z > -range)
                    {
                        transform.position = Vector3.MoveTowards(transform.position
                            , new Vector3(end.x, end.y, -range)
                            , 0.3f);
                    }
                    else
                    {
                        step++;
                    }
                }
            }

            yield return new WaitForSeconds(0.02f);
        }
    }
}
