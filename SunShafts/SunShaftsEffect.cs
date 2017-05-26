using UnityEngine;

namespace SunShafts2
{
    public class SunShaftsEffect : PostEffectsBase
    {
        public SunShaftsEffect.SunShaftsResolution resolution = SunShaftsEffect.SunShaftsResolution.Normal;
        public SunShaftsEffect.ShaftsScreenBlendMode screenBlendMode = SunShaftsEffect.ShaftsScreenBlendMode.Screen;
        public int radialBlurIterations = 2;
        public float maxRadius = 0.75f;
        public bool useDepthTexture = true;
        public SunShaftsConfig config;
        public Transform sunTransform;
        public Shader sunShaftsShader;
        private Material sunShaftsMaterial;
        public Shader simpleClearShader;
        private Material simpleClearMaterial;

        public void Init()
        {
            this.config = SunShaftsConfig.Deserialize("SunShaftsConfig.xml");
            if (this.config == null)
                this.config = new SunShaftsConfig();
            this.enabled = this.config.m_Enabled;
        }

        public void Unload()
        {
            this.config.m_Enabled = this.enabled;
            SunShaftsConfig.Serialize("SunShaftsConfig.xml", (object) this.config);
            Object shader = (Object) this.sunShaftsMaterial.shader;
            Object sunShaftsMaterial = (Object) this.sunShaftsMaterial;
            Object.DestroyImmediate(shader, true);
            Object.DestroyImmediate(sunShaftsMaterial, true);
            Object.Destroy((Object) this);
        }

        public override bool CheckResources()
        {
            this.CheckSupport(this.useDepthTexture);
            this.sunShaftsMaterial = this.CheckShaderAndCreateMaterial(this.sunShaftsShader, this.sunShaftsMaterial);
            this.simpleClearMaterial =
                this.CheckShaderAndCreateMaterial(this.simpleClearShader, this.simpleClearMaterial);
            if (!this.isSupported)
                this.ReportAutoDisable();
            return this.isSupported;
        }

        public void Enable()
        {
            this.enabled = true;
        }

        public void Disable()
        {
            this.enabled = false;
        }

        public void setColor(Color color)
        {
            this.config.sunColor = new Color(color.r, color.g, color.b);
            this.config.sunShaftIntensity = color.a * 0.6f;
        }

        public void setTime(float time)
        {
            this.config.height = time;
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (!this.CheckResources())
            {
                Graphics.Blit((Texture) source, destination);
            }
            else
            {
                foreach (Light light in Object.FindObjectsOfType<Light>())
                {
                    if (light.name == "Directional Light")
                    {
                        this.sunTransform.position = Camera.main.gameObject.transform.position -
                                                     light.transform.forward * 2000f;
                        this.sunTransform.position = new Vector3(this.sunTransform.position.x,
                            (float) ((double) this.config.height * (double) this.sunTransform.position.y / 2.0),
                            this.sunTransform.position.z);
                    }
                }
                if (this.useDepthTexture)
                    this.GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;
                int num1 = 4;
                if (this.resolution == SunShaftsEffect.SunShaftsResolution.Normal)
                    num1 = 2;
                else if (this.resolution == SunShaftsEffect.SunShaftsResolution.High)
                    num1 = 1;
                Vector3 vector3 = Vector3.one * 0.5f;
                vector3 = !(bool) ((Object) this.sunTransform)
                    ? new Vector3(0.5f, 0.5f, 0.0f)
                    : this.GetComponent<Camera>().WorldToViewportPoint(this.sunTransform.position);
                int width = source.width / num1;
                int height = source.height / num1;
                RenderTexture temporary1 = RenderTexture.GetTemporary(width, height, 0);
                this.sunShaftsMaterial.SetVector("_BlurRadius4",
                    new Vector4(1f, 1f, 0.0f, 0.0f) * this.config.sunShaftBlurRadius);
                this.sunShaftsMaterial.SetVector("_SunPosition",
                    new Vector4(vector3.x, vector3.y, vector3.z, this.maxRadius));
                this.sunShaftsMaterial.SetVector("_SunThreshold", (Vector4) this.config.sunThreshold);
                if (!this.useDepthTexture)
                {
                    RenderTextureFormat format = this.GetComponent<Camera>().hdr
                        ? RenderTextureFormat.DefaultHDR
                        : RenderTextureFormat.Default;
                    RenderTexture temporary2 = RenderTexture.GetTemporary(source.width, source.height, 0, format);
                    RenderTexture.active = temporary2;
                    GL.ClearWithSkybox(false, this.GetComponent<Camera>());
                    this.sunShaftsMaterial.SetTexture("_Skybox", (Texture) temporary2);
                    Graphics.Blit((Texture) source, temporary1, this.sunShaftsMaterial, 3);
                    RenderTexture.ReleaseTemporary(temporary2);
                }
                else
                    Graphics.Blit((Texture) source, temporary1, this.sunShaftsMaterial, 2);
                this.DrawBorder(temporary1, this.simpleClearMaterial);
                this.radialBlurIterations = Mathf.Clamp(this.radialBlurIterations, 1, 4);
                float num2 = this.config.sunShaftBlurRadius * (1f / 768f);
                this.sunShaftsMaterial.SetVector("_BlurRadius4", new Vector4(num2, num2, 0.0f, 0.0f));
                this.sunShaftsMaterial.SetVector("_SunPosition",
                    new Vector4(vector3.x, vector3.y, vector3.z, this.maxRadius));
                for (int index = 0; index < this.radialBlurIterations; ++index)
                {
                    RenderTexture temporary2 = RenderTexture.GetTemporary(width, height, 0);
                    Graphics.Blit((Texture) temporary1, temporary2, this.sunShaftsMaterial, 1);
                    RenderTexture.ReleaseTemporary(temporary1);
                    float num3 = (float) ((double) this.config.sunShaftBlurRadius *
                                          (((double) index * 2.0 + 1.0) * 6.0) / 768.0);
                    this.sunShaftsMaterial.SetVector("_BlurRadius4", new Vector4(num3, num3, 0.0f, 0.0f));
                    temporary1 = RenderTexture.GetTemporary(width, height, 0);
                    Graphics.Blit((Texture) temporary2, temporary1, this.sunShaftsMaterial, 1);
                    RenderTexture.ReleaseTemporary(temporary2);
                    float num4 = (float) ((double) this.config.sunShaftBlurRadius *
                                          (((double) index * 2.0 + 2.0) * 6.0) / 768.0);
                    this.sunShaftsMaterial.SetVector("_BlurRadius4", new Vector4(num4, num4, 0.0f, 0.0f));
                }
                if ((double) vector3.z >= 0.0)
                    this.sunShaftsMaterial.SetVector("_SunColor",
                        new Vector4(this.config.sunColor.r, this.config.sunColor.g, this.config.sunColor.b,
                            this.config.sunColor.a) * this.config.sunShaftIntensity);
                else
                    this.sunShaftsMaterial.SetVector("_SunColor", Vector4.zero);
                this.sunShaftsMaterial.SetTexture("_ColorBuffer", (Texture) temporary1);
                Graphics.Blit((Texture) source, destination, this.sunShaftsMaterial,
                    this.screenBlendMode == SunShaftsEffect.ShaftsScreenBlendMode.Screen ? 0 : 4);
                RenderTexture.ReleaseTemporary(temporary1);
            }
        }

        public enum SunShaftsResolution
        {
            Low,
            Normal,
            High,
        }

        public enum ShaftsScreenBlendMode
        {
            Screen,
            Add,
        }
    }
}