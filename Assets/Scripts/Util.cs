using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Util : MonoBehaviour
{
    public void SetMaterialColor(MeshRenderer renderer, Color color)
    {
        MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();

        propertyBlock.SetColor("_BaseColor", color);
        renderer.SetPropertyBlock(propertyBlock);
    }

    public IEnumerator ColorShiftEffect(MeshRenderer renderer, Color initialColor)
    {
        int step = 0;

        Color color = initialColor;
        float deltaR = 12f / 255;

        float minR = initialColor.r - 15f / 255;
        float maxR = initialColor.r + 30f / 255;

        if (minR < 0) minR = 0;
        if (maxR > 1) maxR = 1;

        while(step < 3)
        {
            if(step == 2)
            {
                if (color.r < initialColor.r)
                {
                    color += deltaR * Color.white;
                    SetMaterialColor(renderer, color);
                }
                else
                {
                    step++;
                }
            }
            else if(step % 2 == 0)
            {
                if(color.r < maxR)
                {
                    color += deltaR * Color.white;
                    SetMaterialColor(renderer, color);
                }
                else
                {
                    step++;
                }
            }
            else
            {
                if (color.r > minR)
                {
                    color -= deltaR * Color.white;
                    SetMaterialColor(renderer, color);
                }
                else
                {
                    step++;
                }
            }

            yield return new WaitForSeconds(0.02f);
        }

        SetMaterialColor(renderer, initialColor);
    }
}
