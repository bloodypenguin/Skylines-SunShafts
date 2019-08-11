using ICities;
using System;
using System.Reflection;
using ColossalFramework.Plugins;
using UnityEngine;

namespace SunShafts2
{
    public class LoadingExtension : LoadingExtensionBase
    {
        private Material sunShaftShaderMaterial;
        private Material simpleClearShaderMaterial;
        private SunShaftsEffect sunShaftsEffect;

        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);
            this.sunShaftsEffect = Camera.main.gameObject.AddComponent<SunShaftsEffect>();
            LoadShaders();
            this.sunShaftsEffect.simpleClearShader = this.simpleClearShaderMaterial.shader;
            this.sunShaftsEffect.sunShaftsShader = this.sunShaftShaderMaterial.shader;
            this.sunShaftsEffect.sunTransform = new GameObject().transform;
            this.sunShaftsEffect.Init();
            var gameObject = GameObject.Find("ModControl");
            if (gameObject == (UnityEngine.Object) null)
            {
                gameObject = new GameObject("ModControl");
                gameObject.AddComponent<ModControl>();
            }

            var modControl = gameObject.GetComponent<ModControl>();
            modControl.SendMessage("addMod", (object) "SunShafts");
            modControl.SendMessage("setAction", (object) new Action(this.OnModControlGUI));
            modControl.SendMessage("setHeight", (object) 120f);
        }

        private void LoadShaders()
        {
            var assetsUri = "file:///" + modPath.Replace("\\", "/") + "/sunshaftsshaders";
            var www = new WWW(assetsUri);
            var assetBundle = www.assetBundle;

            CheckAssetBundle(assetBundle, assetsUri);
            ThrowPendingCheckErrors();

            const string sunShaftsCompositeAssetName = "Assets/AssetBundle/SunShaftsComposite.shader";
            const string simpleClearAssetName = "Assets/AssetBundle/SimpleClear.shader";
            var sunShaftsCompositeShaderContent = assetBundle.LoadAsset(sunShaftsCompositeAssetName) as Shader;
            var simpleClearShaderContent = assetBundle.LoadAsset(simpleClearAssetName) as Shader;

            CheckShader(sunShaftsCompositeShaderContent, assetBundle, sunShaftsCompositeAssetName);
            CheckShader(simpleClearShaderContent, assetBundle, simpleClearAssetName);
            ThrowPendingCheckErrors();

            sunShaftShaderMaterial = new Material(sunShaftsCompositeShaderContent);
            simpleClearShaderMaterial = new Material(simpleClearShaderContent);

            CheckMaterial(sunShaftShaderMaterial, sunShaftsCompositeAssetName);
            CheckMaterial(simpleClearShaderMaterial, simpleClearAssetName);
            ThrowPendingCheckErrors();

            assetBundle.Unload(false);
        }

        private static string cachedModPath = null;
        private string checkErrorMessage = null;


        static string modPath =>
            cachedModPath ?? (cachedModPath =
                PluginManager.instance.FindPluginInfo(Assembly.GetAssembly(typeof(Mod))).modPath);

        private void HandleCheckError(string message)
        {
#if (DEBUG)
            DebugOutputPanel.AddMessage(PluginManager.MessageType.Error, message);
#endif
            if (checkErrorMessage == null)
            {
                checkErrorMessage = message;
            }
            else
            {
                checkErrorMessage += "; " + message;
            }
        }

        private void ThrowPendingCheckErrors()
        {
            if (checkErrorMessage != null)
            {
                throw new Exception(checkErrorMessage);
            }
        }

        private void CheckAssetBundle(AssetBundle assetBundle, string assetsUri)
        {
            if (assetBundle == null)
            {
                HandleCheckError("AssetBundle with URI '" + assetsUri + "' could not be loaded");
            }
#if (DEBUG)
            else
            {
                DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, "Mod Assets URI: " + assetsUri);
                foreach (string asset in assetBundle.GetAllAssetNames())
                {
                    DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, "Asset: " + asset);
                }
            }
#endif
        }

        private void CheckShader(Shader shader, string source)
        {
            if (shader == null)
            {
                HandleCheckError("Shader " + source + " is missing or invalid");
            }
            else
            {
                if (!shader.isSupported)
                {
                    HandleCheckError("Shader '" + shader.name + "' " + source + " is not supported");
                }
#if (DEBUG)
                else
                {
                    DebugOutputPanel.AddMessage(
                        PluginManager.MessageType.Message,
                        "Shader '" + shader.name + "' " + source + " loaded");
                }
#endif
            }
        }

        private void CheckShader(Shader shader, AssetBundle assetBundle, string shaderAssetName)
        {
            CheckShader(shader, "from asset '" + shaderAssetName + "'");
        }

        private void CheckMaterial(Material material, string materialAssetName)
        {
            if (material == null)
            {
                HandleCheckError("Material for shader '" + materialAssetName + "' could not be created");
            }
#if (DEBUG)
            else
            {
                DebugOutputPanel.AddMessage(
                    PluginManager.MessageType.Message,
                    "Material for shader '" + materialAssetName + "' created");
            }
#endif
        }


        private void OnModControlGUI()
        {
            this.sunShaftsEffect.enabled = GUI.Toggle(new Rect(0.0f, 0.0f, 100f, 20f), this.sunShaftsEffect.enabled,
                new GUIContent("Sun Shafts"));
            this.sunShaftsEffect.config.sunShaftIntensity = GUI.HorizontalSlider(new Rect(0.0f, 20f, 100f, 20f),
                this.sunShaftsEffect.config.sunShaftIntensity, 0.0f, 6f);
            GUI.Label(new Rect(105f, 20f, 130f, 20f), "Intensity");
            this.sunShaftsEffect.config.height = GUI.HorizontalSlider(new Rect(0.0f, 40f, 100f, 20f),
                this.sunShaftsEffect.config.height, 0.0f, 2f);
            GUI.Label(new Rect(105f, 40f, 130f, 20f), "Height");
            this.sunShaftsEffect.config.sunColor.r = GUI.HorizontalSlider(new Rect(0.0f, 60f, 100f, 20f),
                this.sunShaftsEffect.config.sunColor.r, 0.0f, 1f);
            GUI.Label(new Rect(105f, 60f, 130f, 20f), "R");
            this.sunShaftsEffect.config.sunColor.g = GUI.HorizontalSlider(new Rect(0.0f, 80f, 100f, 20f),
                this.sunShaftsEffect.config.sunColor.g, 0.0f, 1f);
            GUI.Label(new Rect(105f, 80f, 130f, 20f), "G");
            this.sunShaftsEffect.config.sunColor.b = GUI.HorizontalSlider(new Rect(0.0f, 100f, 100f, 20f),
                this.sunShaftsEffect.config.sunColor.b, 0.0f, 1f);
            GUI.Label(new Rect(105f, 100f, 130f, 20f), "B");
        }

        public override void OnLevelUnloading()
        {
            base.OnLevelUnloading();
            try
            {
                this.sunShaftsEffect.Unload();
            }
            catch (Exception ex)
            {
            }
        }
    }
}