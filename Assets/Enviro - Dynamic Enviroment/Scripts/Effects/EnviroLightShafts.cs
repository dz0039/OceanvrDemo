using System;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Enviro/Effects/LightShafts")]
public class EnviroLightShafts : EnviroEffects
{
    public enum SunShaftsResolution
    {
        Low = 0,
        Normal = 1,
        High = 2,
    }

    public enum ShaftsScreenBlendMode
    {
        Screen = 0,
        Add = 1,
    }


    [HideInInspector]
    public SunShaftsResolution resolution = SunShaftsResolution.Normal;
    [HideInInspector]
    public ShaftsScreenBlendMode screenBlendMode = ShaftsScreenBlendMode.Screen;

    [HideInInspector]
    public Transform sunTransform;

    [HideInInspector]
    public int radialBlurIterations = 2;
    [HideInInspector]
    public Color sunColor = Color.white;
    [HideInInspector]
    public Color sunThreshold = new Color(0.87f, 0.74f, 0.65f);
    [HideInInspector]
    public float sunShaftBlurRadius = 2.5f;
    [HideInInspector]
    public float sunShaftIntensity = 1.15f;

    [HideInInspector]
    public float maxRadius = 0.75f;

    [HideInInspector]
    public bool useDepthTexture = true;

    [HideInInspector]
    public Shader sunShaftsShader;
    [HideInInspector]
    public Material sunShaftsMaterial;

    [HideInInspector]
    public Shader simpleClearShader;
    [HideInInspector]
    public Material simpleClearMaterial;

    private Camera cam;

    public override bool CheckResources()
    {
        CheckSupport(useDepthTexture);

        sunShaftsMaterial = CheckShaderAndCreateMaterial(sunShaftsShader, sunShaftsMaterial);
        simpleClearMaterial = CheckShaderAndCreateMaterial(simpleClearShader, simpleClearMaterial);

        if (!isSupported)
            ReportAutoDisable();
        return isSupported;
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (CheckResources() == false)
        {
            Graphics.Blit(source, destination);
            return;
        }

        if (EnviroSky.instance == null)
        {
            Graphics.Blit(source, destination);
            return;
        }

        if (cam == null)
            cam = GetComponent<Camera>();

        // we actually need to check this every frame
        if (useDepthTexture && cam.actualRenderingPath == RenderingPath.Forward)
        {
            cam.depthTextureMode |= DepthTextureMode.Depth;
        }
        int divider = 4;
        if (resolution == SunShaftsResolution.Normal)
            divider = 2;
        else if (resolution == SunShaftsResolution.High)
            divider = 1;

        Vector3 v = Vector3.one * 0.5f;

        if (sunTransform)
            v = cam.WorldToViewportPoint(sunTransform.position);
        else
            v = new Vector3(0.5f, 0.5f, 0.0f);

        int rtW = source.width / divider;
        int rtH = source.height / divider;

        RenderTexture lrColorB;
        RenderTexture lrDepthBuffer;

#if UNITY_5_6
        lrDepthBuffer = RenderTexture.GetTemporary(rtW, rtH, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Default);
#endif

#if UNITY_2017_1_0
       lrDepthBuffer = RenderTexture.GetTemporary(rtW, rtH, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Default);
#endif

#if UNITY_2017_1_1 || UNITY_2017_1_2 || UNITY_2017_1_3|| UNITY_2017_1_4
       lrDepthBuffer = RenderTexture.GetTemporary(rtW, rtH, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Default,1, RenderTextureMemoryless.None, source.vrUsage);
#endif

#if UNITY_2017_2_OR_NEWER
        lrDepthBuffer = RenderTexture.GetTemporary(rtW, rtH, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Default, 1, RenderTextureMemoryless.None, source.vrUsage);
#endif

        // mask out everything except the skybox
        // we have 2 methods, one of which requires depth buffer support, the other one is just comparing images

        sunShaftsMaterial.SetVector("_BlurRadius4", new Vector4(1.0f, 1.0f, 0.0f, 0.0f) * sunShaftBlurRadius);
        sunShaftsMaterial.SetVector("_SunPosition", new Vector4(v.x, v.y, v.z, maxRadius));
        sunShaftsMaterial.SetVector("_SunThreshold", sunThreshold);

        if (!useDepthTexture)
        {
            var format = EnviroSky.instance.GetCameraHDR(cam) ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default;
            RenderTexture tmpBuffer = RenderTexture.GetTemporary(source.width, source.height, 0, format);
            RenderTexture.active = tmpBuffer;
            GL.ClearWithSkybox(false, cam);

            sunShaftsMaterial.SetTexture("_Skybox", tmpBuffer);
            Graphics.Blit(source, lrDepthBuffer, sunShaftsMaterial, 3);
            RenderTexture.ReleaseTemporary(tmpBuffer);
        }
        else
        {
            Graphics.Blit(source, lrDepthBuffer, sunShaftsMaterial, 2);
        }

        // paint a small black small border to get rid of clamping problems
        if (cam.stereoActiveEye == Camera.MonoOrStereoscopicEye.Mono)
            DrawBorder(lrDepthBuffer, simpleClearMaterial);

        // radial blur:

        radialBlurIterations = Mathf.Clamp(radialBlurIterations, 1, 4);

        float ofs = sunShaftBlurRadius * (1.0f / 768.0f);

        sunShaftsMaterial.SetVector("_BlurRadius4", new Vector4(ofs, ofs, 0.0f, 0.0f));
        sunShaftsMaterial.SetVector("_SunPosition", new Vector4(v.x, v.y, v.z, maxRadius));

        for (int it2 = 0; it2 < radialBlurIterations; it2++)
        {
            // each iteration takes 2 * 6 samples
            // we update _BlurRadius each time to cheaply get a very smooth look

#if UNITY_5_6
            lrColorB = RenderTexture.GetTemporary(rtW, rtH, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Default);
#endif

#if UNITY_2017_1_0
       lrColorB = RenderTexture.GetTemporary(rtW, rtH, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Default);
#endif

#if UNITY_2017_1_1 || UNITY_2017_1_2 || UNITY_2017_1_3 || UNITY_2017_1_4
        lrColorB = RenderTexture.GetTemporary(rtW, rtH, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Default, 1, RenderTextureMemoryless.None, source.vrUsage);
#endif

#if UNITY_2017_2_OR_NEWER
        lrColorB = RenderTexture.GetTemporary(rtW, rtH, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Default, 1, RenderTextureMemoryless.None, source.vrUsage);
#endif


            Graphics.Blit(lrDepthBuffer, lrColorB, sunShaftsMaterial, 1);
            RenderTexture.ReleaseTemporary(lrDepthBuffer);
            ofs = sunShaftBlurRadius * (((it2 * 2.0f + 1.0f) * 6.0f)) / 768.0f;
            sunShaftsMaterial.SetVector("_BlurRadius4", new Vector4(ofs, ofs, 0.0f, 0.0f));

#if UNITY_5_6
            lrDepthBuffer = RenderTexture.GetTemporary(rtW, rtH, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Default);
#endif

#if UNITY_2017_1_0
       lrDepthBuffer = RenderTexture.GetTemporary(rtW, rtH, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Default);
#endif

#if UNITY_2017_1_1 || UNITY_2017_1_2 || UNITY_2017_1_3 || UNITY_2017_1_4
       lrDepthBuffer = RenderTexture.GetTemporary(rtW, rtH, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Default, 1, RenderTextureMemoryless.None, source.vrUsage);
#endif

#if UNITY_2017_2_OR_NEWER
        lrDepthBuffer = RenderTexture.GetTemporary(rtW, rtH, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Default, 1, RenderTextureMemoryless.None, source.vrUsage);
#endif

            Graphics.Blit(lrColorB, lrDepthBuffer, sunShaftsMaterial, 1);
            RenderTexture.ReleaseTemporary(lrColorB);
            ofs = sunShaftBlurRadius * (((it2 * 2.0f + 2.0f) * 6.0f)) / 768.0f;
            sunShaftsMaterial.SetVector("_BlurRadius4", new Vector4(ofs, ofs, 0.0f, 0.0f));
        }

        // put together:

        if (v.z >= 0.0f)
            sunShaftsMaterial.SetVector("_SunColor", new Vector4(sunColor.r, sunColor.g, sunColor.b, sunColor.a) * sunShaftIntensity);
        else
            sunShaftsMaterial.SetVector("_SunColor", Vector4.zero); // no backprojection !

        sunShaftsMaterial.SetTexture("_ColorBuffer", lrDepthBuffer);
        Graphics.Blit(source, destination, sunShaftsMaterial, (screenBlendMode == ShaftsScreenBlendMode.Screen) ? 0 : 4);

        RenderTexture.ReleaseTemporary(lrDepthBuffer);
    }
}
