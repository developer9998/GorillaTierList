using BepInEx;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GorillaNetworking;
using GorillaTierList.Patches;
using GorillaTierList.Scripts;

// Credit:
// https://freesound.org/s/576113/
// https://freesound.org/s/327737/

namespace GorillaTierList
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance { get; internal set; }
        public bool Initialized;

        // Tier
        public List<Canvas> DropperCanvases = new List<Canvas>();
        public List<Transform> Droppers = new List<Transform>();
        public List<Transform> HandCanvases = new List<Transform>();
        public Transform ReturnDropper;
        public Transform ReturnCollider;

        // Objects
        public GameObject tierObject;
        public Transform BaseDropper;
        public GameObject refObject;

        // Audio
        public AudioClip 
            Pick, Drop,
            DropError;

        // Data
        public bool InLeftHand;
        public bool InRightHand;
        public Vector3 DropperSizeLocal = new Vector3(0.008829016f, 0.002752198f, 0.008829016f);

        internal void Awake()
        {
            Instance = this;
            HarmonyPatches.ApplyHarmonyPatches();
        }

        public CosmeticsController.CosmeticItem GetItemFromName(string str)
        {
            if (CosmeticsController.instance.allCosmeticsItemIDsfromDisplayNamesDict.ContainsKey(str))
            {
                string value = CosmeticsController.instance.allCosmeticsItemIDsfromDisplayNamesDict[str];
                if (CosmeticsController.instance.allCosmeticsDict.ContainsKey(value)) return CosmeticsController.instance.allCosmeticsDict[value];
            }

            return CosmeticsController.instance.nullItem;
        }

        internal void Reload()
        {
            TierData.LoadData();

            InLeftHand = false;
            InRightHand = false;

            foreach (var indivDropper in Droppers)
            {
                indivDropper.TryGetComponent(out Rating indivComp);
                if (indivComp != null) Destroy(indivComp.refObject);
                Destroy(indivDropper.gameObject);
            }
            Droppers.Clear();

            foreach (var canv in tierObject.transform.GetComponentsInChildren<Canvas>()) if (canv.name == "TierHeader") canv.GetComponentInChildren<Text>().text = TierData.CurrentData.DropperName;

            BaseDropper.gameObject.SetActive(true);

            if (BaseDropper != null)
            {
                refObject = Instantiate(BaseDropper.gameObject);
                refObject.GetComponent<Image>().color = new Color(1, 1, 1, 0.3f);
                refObject.GetComponentInChildren<RawImage>().color = new Color(1, 1, 1, 0f);
                refObject.transform.localPosition = Vector3.zero;
                refObject.transform.localRotation = Quaternion.identity;
                refObject.transform.localScale = Vector3.one;

                System.Random randomChance = new System.Random();
                var shuffledDropperNames = TierData.CurrentData.DropperNames.OrderBy(a => randomChance.Next()).ToList();

                foreach (var option in shuffledDropperNames)
                {
                    Transform clonedOption = Instantiate(BaseDropper).transform;
                    clonedOption.transform.SetParent(BaseDropper.parent, false);
                    clonedOption.GetComponentInChildren<Text>().text = option;
                    clonedOption.gameObject.AddComponent<Rating>();
                    clonedOption.GetComponentInChildren<RawImage>().color = Color.clear;
                    Droppers.Add(clonedOption);

                    string imagePath = Path.Combine(Path.GetDirectoryName(typeof(Plugin).Assembly.Location), $"{option}.png");
                    if (File.Exists(imagePath))
                    {
                        Texture2D signTexture = new Texture2D(128, 128, TextureFormat.RGBA32, false);
                        signTexture.filterMode = FilterMode.Point;

                        signTexture.LoadImage(File.ReadAllBytes(imagePath));
                        signTexture.Apply();

                        clonedOption.GetComponentInChildren<Text>().enabled = false;
                        clonedOption.GetComponentInChildren<RawImage>().texture = signTexture;
                        clonedOption.GetComponentInChildren<RawImage>().color = Color.white;
                    }
                    else
                    {
                        string replacedOption = option.Replace("(", "").Replace(")", "");
                        if (option.Length != replacedOption.Length)
                        {
                            var item = GetItemFromName(replacedOption.ToUpper());
                            if (!item.isNullItem)
                            {
                                var sprite = item.itemPicture;
                                clonedOption.GetComponentInChildren<Text>().enabled = false;
                                clonedOption.GetComponentInChildren<Text>().text = replacedOption;
                                clonedOption.Find("Image").GetComponent<Image>().sprite = sprite;
                                clonedOption.Find("Image").GetComponent<Image>().color = new Color(1, 1, 1, 1);
                            }
                        }
                    }
                }

                BaseDropper.gameObject.SetActive(false);
            }
        }

        internal void OnInitialized()
        {
            if (Initialized) return;
            Initialized = true;

            TierData.LoadData();

            Stream str = Assembly.GetExecutingAssembly().GetManifestResourceStream("GorillaTierList.Resources.tierbundle");
            AssetBundle bundle = AssetBundle.LoadFromStream(str);

            Pick = bundle.LoadAsset<AudioClip>("grab");
            Drop = bundle.LoadAsset<AudioClip>("drop");
            DropError = bundle.LoadAsset<AudioClip>("error");

            HandCanvases.Add(Instantiate(bundle.LoadAsset<GameObject>("HandCanvas")).transform);
            HandCanvases[0].transform.SetParent(GorillaTagger.Instance.offlineVRRig.leftHandTransform.parent, false);
            HandCanvases[0].transform.localPosition = new Vector3(-0.01f, 0.05f, 0.11f);
            HandCanvases[0].transform.localRotation = Quaternion.Euler(0, 0, 0);
            HandCanvases[0].transform.localScale = new Vector3(-1, 1, 1);

            HandCanvases.Add(Instantiate(bundle.LoadAsset<GameObject>("HandCanvas")).transform);
            HandCanvases[1].transform.SetParent(GorillaTagger.Instance.offlineVRRig.rightHandTransform.parent, false);
            HandCanvases[1].transform.localPosition = new Vector3(0.01f, 0.05f, 0.11f);
            HandCanvases[1].transform.localRotation = Quaternion.Euler(-5.06f, -176.021f, 1.917f);
            HandCanvases[1].transform.localScale = new Vector3(1, 1, 1);

            tierObject = Instantiate(bundle.LoadAsset<GameObject>("TierArea"));
            tierObject.transform.position = new Vector3(-54.987f, 18.183f, -113.913f);
            tierObject.transform.rotation = Quaternion.Euler(0, 120.112f, 0.0009918213f);
            tierObject.transform.localScale = new Vector3(0.4781441f, 0.4781441f, 0.478144f);

            GameObject rc = tierObject.transform.Find("ReloadCube").gameObject;
            rc.layer = 18;
            rc.GetComponent<Collider>().isTrigger = true;
            rc.AddComponent<Reload>();

            ReturnCollider = tierObject.transform.Find("BaseTierEntry");

            foreach (var canv in tierObject.transform.GetComponentsInChildren<Canvas>())
            {
                if (canv.name != "TierNames" && canv.name != "TierHeader" && canv.name != "ReloadText")
                {
                    DropperCanvases.Add(canv);
                    if (canv.name == "TierStorage")
                    {
                        foreach(var trans in canv.transform.GetComponentsInChildren<Transform>())
                        {
                            if (trans.name == "RatingPanel")
                            {
                                BaseDropper = trans;
                                break;
                            }
                        }
                    }
                }

                if (canv.name == "TierStorage") ReturnDropper = canv.transform;
                if (canv.name == "TierHeader") canv.GetComponentInChildren<Text>().text = TierData.CurrentData.DropperName;
            }

            // to make it a bit more consistent and to fix some issues 
            Reload();
        }

        public Transform GetNearest(bool l)
        {
            Transform tr = null;
            Transform findObject = l ? GorillaLocomotion.Player.Instance.leftControllerTransform.transform : GorillaLocomotion.Player.Instance.rightControllerTransform.transform;
            List<Canvas> reverse = Enumerable.Reverse(DropperCanvases).ToList();
            foreach (Canvas go in reverse) if ((go.transform.position.y - 0.2f) <= findObject.transform.position.y) tr = go.transform;

            if (tr != null) return tr;
            return null;
        }
    }
}
