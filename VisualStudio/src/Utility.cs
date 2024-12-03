global using CC = System.ConsoleColor;
global using MelonLoader;
global using UnityEngine;
global using System.Reflection;
global using Il2Cpp;
global using System.Collections;
global using HarmonyLib;
global using Il2CppInterop.Runtime.Attributes;
global using Il2CppTLD.Placement;

namespace CalorieWaltz

{
    public enum ExtinguishType
    {
        Instant,
        NoAnimation,
        BlowOut,
        DestroyWick,
        Wind
    }

    public static class TextureExtentions
    {
        public static Texture2D ToTexture2D(this Texture texture)
        {

            Texture2D newTex = new Texture2D(texture.width, texture.height);
            RenderTexture renderTexture = new RenderTexture(texture.width, texture.height, 32);

            Graphics.Blit(texture, renderTexture);

            RenderTexture.active = renderTexture;

            newTex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            newTex.Apply();

            renderTexture.Release();


            return newTex;
        }
    }

    public static class GameobjectExtensions
    {
        public static List<GameObject> GetAllImmediateChildren(this GameObject Go)
        {
            List<GameObject> list = new List<GameObject>();
            for (int i = 0; i < Go.transform.childCount; i++)
            {
                list.Add(Go.transform.GetChild(i).gameObject);
            }
            return list;
        }

        public static void DestroyAllImmediateChildren(this GameObject Go)
        {
            for (int i = 0; i < Go.transform.childCount; i++)
            {
                UnityEngine.Object.Destroy(Go.transform.GetChild(i).gameObject);
            }
        }

        public static bool IsCandle(this GameObject go) => HarmonyValues.allCandleTiers.Contains(go.name.Replace("GEAR_candle", ""));

        public static bool IsCard(this GameObject go) => HarmonyValues.allCandleTiers.Contains(go.name.Replace("GEAR_card", ""));

    }

    public static class Utility
    {


        public static GameObject GetGameObjectUnderCrosshair()
        {
            GameObject go = null;
            PlayerManager pm = GameManager.GetPlayerManagerComponent();

            float maxPickupRange = GameManager.GetGlobalParameters().m_MaxPickupRange;
            float maxRange = pm.ComputeModifiedPickupRange(maxPickupRange);
            if (pm.GetControlMode() == PlayerControlMode.InFPCinematic)
            {
                maxRange = 50f;
            }

            go = GameManager.GetPlayerManagerComponent().GetInteractiveObjectUnderCrosshairs(maxRange);

            //if (go) gi = go.GetComponent<GearItem>();

            return go;
            /*
            
            GameObject go = InterfaceManager.GetPanel<Panel_HUD>().m_PreviousItemUnderCrosshairs;
            if (go) gi = go.GetComponent<GearItem>();
            
            */
        }
        public static IEnumerator InvokeWithSecondsDelay(Action action, float delay)
        {
            yield return new WaitForSecondsRealtime(delay);

            action.Invoke();

            yield return null;
        }

        public static void SetLayerRecursively(this GameObject obj, int layer)
        {
            obj.layer = layer;

            foreach (Transform child in obj.transform)
            {
                child.gameObject.SetLayerRecursively(layer);
            }
        }

        public static List<Transform> GetAllChildrenRecursive(Transform parent)
        {
            List<Transform> list = new List<Transform>();

            foreach (Transform g in parent.GetComponentsInChildren<Transform>())
            {
                list.Add(g);
            }

            return list;
        }

        public static void SetLayerRecursively2(this GameObject parent, int layer)
        {

            parent.layer = layer;

            foreach (Transform g in parent.transform.GetComponentsInChildren<Transform>())
            {
                g.gameObject.layer = 23;
            }

        }

        public static string GetGameObjectPath(GameObject obj)
        {
            string path = "/" + obj.name;
            while (obj.transform.parent != null)
            {
                obj = obj.transform.parent.gameObject;
                path = "/" + obj.name + path;
            }
            return path;
        }

        public static void Log(int num, CC color = CC.Red)
        {
            MelonLogger.Msg(color, "This shouldn't happen #" + num);
        }

        public static void Log(ConsoleColor color, string message)
        {
            if (Settings.options.debugLog)
            {
                MelonLogger.Msg(color, message);
            }
        }

        public static bool IsScenePlayable()
        {
            return !(string.IsNullOrEmpty(GameManager.m_ActiveScene) || GameManager.m_ActiveScene.Contains("MainMenu") || GameManager.m_ActiveScene == "Boot" || GameManager.m_ActiveScene == "Empty");
        }

        public static bool IsScenePlayable(string scene)
        {
            return !(string.IsNullOrEmpty(scene) || scene.Contains("MainMenu") || scene == "Boot" || scene == "Empty");
        }
        public static bool IsMainMenu(string scene)
        {
            return !string.IsNullOrEmpty(scene) && scene.Contains("MainMenu");
        }

    }
}
