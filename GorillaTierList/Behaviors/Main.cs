using GorillaNetworking;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using GorillaLocomotion;

namespace GorillaTierList.Behaviors
{
    public class Main : MonoBehaviour
    {
        public static Main Instance { get; private set; }
        public bool _initalized;

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
        public AudioClip Pick, Drop, DropError;

        // Data
        public bool InLeftHand;
        public bool InRightHand;
        public Vector3 DropperSizeLocal = new Vector3(0.008829016f, 0.002752198f, 0.008829016f);

        public void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(this);
        }

        public async void Start()
        {
            if (_initalized) return;
            _initalized = true;

            await TierParse.LoadData();

            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("GorillaTierList.Resources.tierbundle");
            var request = AssetBundle.LoadFromStreamAsync(stream);

            var bundleCompletionSource = new TaskCompletionSource<AssetBundle>();
            request.completed += operation =>
            {
                var outRequest = operation as AssetBundleCreateRequest;
                bundleCompletionSource.SetResult(outRequest.assetBundle);
            };
            AssetBundle tierBundle = await bundleCompletionSource.Task;

            async Task<T> LoadAsset<T>(string name) where T : UnityEngine.Object
            {
                var request = tierBundle.LoadAssetAsync<T>(name);

                var taskCompletionSource = new TaskCompletionSource<T>();
                request.completed += operation =>
                {
                    var outRequest = operation as AssetBundleRequest;
                    if (outRequest.asset == null)
                    {
                        taskCompletionSource.SetResult(null);
                        return;
                    }

                    taskCompletionSource.SetResult(outRequest.asset as T);
                };
                return await taskCompletionSource.Task;
            }

            Pick = await LoadAsset<AudioClip>("grab");
            Drop = await LoadAsset<AudioClip>("drop");
            DropError = await LoadAsset<AudioClip>("error");

            HandCanvases.Add(Instantiate(await LoadAsset<GameObject>("HandCanvas")).transform);
            HandCanvases[0].transform.SetParent(GorillaTagger.Instance.offlineVRRig.leftHandTransform.parent, false);
            HandCanvases[0].transform.localPosition = new Vector3(-0.01f, 0.05f, 0.11f);
            HandCanvases[0].transform.localRotation = Quaternion.Euler(0, 0, 0);
            HandCanvases[0].transform.localScale = new Vector3(-1, 1, 1);

            HandCanvases.Add(Instantiate(await LoadAsset<GameObject>("HandCanvas")).transform);
            HandCanvases[1].transform.SetParent(GorillaTagger.Instance.offlineVRRig.rightHandTransform.parent, false);
            HandCanvases[1].transform.localPosition = new Vector3(0.01f, 0.05f, 0.11f);
            HandCanvases[1].transform.localRotation = Quaternion.Euler(-5.06f, -176.021f, 1.917f);
            HandCanvases[1].transform.localScale = new Vector3(1, 1, 1);

            tierObject = Instantiate(await LoadAsset<GameObject>("TierArea"));
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
                if (canv.name == "TierNames" || canv.name == "TierHeader" || canv.name == "ReloadText") continue;
                DropperCanvases.Add(canv);

                if (canv.name == "TierStorage")
                {
                    Transform potentialTransform = canv.transform.GetComponentsInChildren<Transform>().FirstOrDefault(a => a.name == "RatingPanel");
                    BaseDropper = potentialTransform ?? BaseDropper; ReturnDropper = canv.transform;
                }

                if (canv.name == "TierHeader") canv.GetComponentInChildren<Text>().text = TierParse.CurrentData.DropperName;
            }
            DropperCanvases.Reverse();

            OccasionalUpdate();
        }

        public async void OccasionalUpdate()
        {
            await TierParse.LoadData();

            InLeftHand = false;
            InRightHand = false;

            foreach (var indivDropper in Droppers)
            {
                if (!indivDropper.TryGetComponent(out Option indivComp)) goto Remove;
                Destroy(indivComp.refObject);

                Remove: Destroy(indivDropper.gameObject);
            }
            Droppers.Clear();

            var potentialCanvas = tierObject.transform.GetComponentsInChildren<Canvas>().FirstOrDefault(a => a.name == "TierHeader");
            if (potentialCanvas != null) potentialCanvas.GetComponentInChildren<Text>().text = TierParse.CurrentData.DropperName;

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
                var shuffledDropperNames = TierParse.CurrentData.DropperNames.OrderBy(a => randomChance.Next()).ToList();

                foreach (var option in shuffledDropperNames)
                {
                    Transform clonedOption = Instantiate(BaseDropper).transform;
                    clonedOption.transform.SetParent(BaseDropper.parent, false);
                    clonedOption.GetComponentInChildren<Text>().text = option;
                    clonedOption.gameObject.AddComponent<Option>();
                    clonedOption.GetComponentInChildren<RawImage>().color = Color.clear;
                    Droppers.Add(clonedOption);

                    string imagePath = Path.Combine(Path.GetDirectoryName(typeof(Plugin).Assembly.Location), $"{option}.png");
                    if (File.Exists(imagePath))
                    {
                        Texture2D signTexture = new Texture2D(128, 128, TextureFormat.RGBA32, false);
                        signTexture.filterMode = FilterMode.Point;

                        signTexture.LoadImage(await File.ReadAllBytesAsync(imagePath));
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
                            static CosmeticsController.CosmeticItem GetItemFromName(string str)
                            {
                                if (CosmeticsController.instance.allCosmeticsItemIDsfromDisplayNamesDict.ContainsKey(str))
                                {
                                    string value = CosmeticsController.instance.allCosmeticsItemIDsfromDisplayNamesDict[str];
                                    if (CosmeticsController.instance.allCosmeticsDict.ContainsKey(value)) return CosmeticsController.instance.allCosmeticsDict[value];
                                }

                                return CosmeticsController.instance.nullItem;
                            }

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

        public Transform GetNearest(bool isLeftHand)
        {
            Transform findObject = isLeftHand 
                ? Player.Instance.leftHandFollower.transform 
                : Player.Instance.rightHandFollower.transform;

            var temporaryCanvas = DropperCanvases.LastOrDefault(a => (a.transform.position.y - 0.2f) < findObject.transform.position.y);
            return temporaryCanvas?.transform;
        }
    }
}
