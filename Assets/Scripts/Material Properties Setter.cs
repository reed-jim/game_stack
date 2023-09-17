using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialPropertiesSetter : MonoBehaviour
{
    public Color color;
    public int materialIndex = -1;

    private MaterialPropertyBlock propertyBlock;

    private void Start()
    {
        if (propertyBlock == null)
            propertyBlock = new MaterialPropertyBlock();

        Renderer renderer = GetComponent<Renderer>();

        propertyBlock.SetColor("_BaseColor", color);

        if(materialIndex == -1)
        {
            renderer.SetPropertyBlock(propertyBlock);
        }
        else
        {
            renderer.SetPropertyBlock(propertyBlock, materialIndex);
        }
    }

    void OnValidate()
    {
        if (propertyBlock == null)
            propertyBlock = new MaterialPropertyBlock();
       
        Renderer renderer = GetComponent<Renderer>();
   
        propertyBlock.SetColor("_BaseColor", color);

        if (materialIndex == -1)
        {
            renderer.SetPropertyBlock(propertyBlock);
        }
        else
        {
            renderer.SetPropertyBlock(propertyBlock, materialIndex);
        }
    }
}
