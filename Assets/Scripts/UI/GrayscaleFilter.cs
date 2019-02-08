using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GrayscaleFilter : MonoBehaviour
{
    private Material mat;

    public Texture overlay;

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (!mat)
            mat = new Material(Shader.Find("Hidden/GSF"));

        mat.SetTexture("_Mask", overlay);

        Graphics.Blit(src, dest, mat);
    }

    private void OnDisable()
    {
        if (mat)
            DestroyImmediate(mat);
    }
}