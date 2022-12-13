using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using GorillaLocomotion;

namespace GorillaTierList.Scripts
{
    public class Rating : MonoBehaviour
    {
        public bool IsInLeftHand;
        public bool InHand;
        public GameObject refObject;
        public Transform parent;

        internal void ResetTransform()
        {
            gameObject.transform.localPosition = Vector3.zero;
            gameObject.transform.localRotation = Quaternion.identity;
            gameObject.transform.localScale = Vector3.one;
        }

        internal void Update()
        {
            if (refObject != null)
            {
                Transform target = Plugin.Instance.DropperCanvases[Plugin.Instance.DropperCanvases.Count - 1].transform;
                if (Plugin.Instance.GetNearest(IsInLeftHand) != null && Plugin.Instance.GetNearest(IsInLeftHand).childCount != 9 && Vector3.Distance(Player.Instance.bodyCollider.transform.position, Plugin.Instance.tierObject.transform.position) <= 4) target = Plugin.Instance.GetNearest(IsInLeftHand);
                refObject.transform.SetParent(target, false);
                refObject.transform.localScale = Vector3.one;
            }

            // Left Hand
            float LeftDist = Vector3.Distance(Player.Instance.leftHandFollower.transform.position, gameObject.transform.position);
            InputDevices.GetDeviceAtXRNode(XRNode.LeftHand).TryGetFeatureValue(CommonUsages.triggerButton, out bool leftHandDown);

            // Right Hand
            float RightDist = Vector3.Distance(Player.Instance.rightHandFollower.transform.position, gameObject.transform.position);
            InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(CommonUsages.triggerButton, out bool rightHandDown);

            bool shouldBeInLeftHand = leftHandDown && LeftDist < 0.2f;
            bool shouldBeInRightHand = rightHandDown && RightDist < 0.2f;

            // Pick up (Left)
            if (!InHand && !Plugin.Instance.InLeftHand && shouldBeInLeftHand)
            {
                InHand = true;
                IsInLeftHand = true;
                Plugin.Instance.InLeftHand = true;
                GorillaTagger.Instance.StartVibration(true, 1, 0.05f);
                GorillaTagger.Instance.offlineVRRig.tagSound.PlayOneShot(Plugin.Instance.Pick, 1.25f);
                gameObject.transform.SetParent(Plugin.Instance.HandCanvases[0], false);
                ResetTransform();
                gameObject.transform.localScale = Plugin.Instance.DropperSizeLocal;

                if (refObject == null)
                {
                    refObject = Instantiate(Plugin.Instance.refObject);
                    refObject.GetComponentInChildren<Text>().text = gameObject.transform.GetComponentInChildren<Text>().text;
                }
            }
            // Pick up (Right)
            else if (!InHand && !Plugin.Instance.InRightHand && shouldBeInRightHand)
            {
                InHand = true;
                IsInLeftHand = false;
                Plugin.Instance.InRightHand = true;
                GorillaTagger.Instance.StartVibration(false, 1, 0.05f);
                GorillaTagger.Instance.offlineVRRig.tagSound.PlayOneShot(Plugin.Instance.Pick, 1.25f);
                gameObject.transform.SetParent(Plugin.Instance.HandCanvases[1], false);
                ResetTransform();
                gameObject.transform.localScale = Plugin.Instance.DropperSizeLocal;

                if (refObject == null)
                {
                    refObject = Instantiate(Plugin.Instance.refObject);
                    refObject.GetComponentInChildren<Text>().text = gameObject.transform.GetComponentInChildren<Text>().text;
                }
            }
            // Statements for dropping
            else if (InHand)
            {
                // Drop (Left)
                if (IsInLeftHand && !leftHandDown)
                {
                    if (refObject == null) return;
                    InHand = false;
                    Plugin.Instance.InLeftHand = false;
                    GorillaTagger.Instance.StartVibration(true, 0.5f, 0.05f);
                    parent = Plugin.Instance.GetNearest(true) ?? Plugin.Instance.DropperCanvases[Plugin.Instance.DropperCanvases.Count - 2].transform;
                    gameObject.transform.SetParent(refObject.transform.parent, false);
                    gameObject.transform.localScale = Vector3.one;
                    GorillaTagger.Instance.offlineVRRig.tagSound.PlayOneShot(Plugin.Instance.Drop, 1.25f);
                    Destroy(refObject);
                }
                // Drop (Right)
                if (!IsInLeftHand && !rightHandDown)
                {
                    if (refObject == null) return;  
                    InHand = false;
                    Plugin.Instance.InRightHand = false;
                    GorillaTagger.Instance.StartVibration(false, 0.5f, 0.05f);
                    parent = Plugin.Instance.GetNearest(false) ?? Plugin.Instance.DropperCanvases[Plugin.Instance.DropperCanvases.Count - 2].transform;
                    gameObject.transform.SetParent(refObject.transform.parent, false);
                    gameObject.transform.localScale = Vector3.one;
                    GorillaTagger.Instance.offlineVRRig.tagSound.PlayOneShot(Plugin.Instance.Drop, 1.25f);
                    Destroy(refObject);
                }
                // Drop (When you get too far away from the tier list object)
                if (Vector3.Distance(Player.Instance.bodyCollider.transform.position, Plugin.Instance.tierObject.transform.position) >= 5)
                {
                    if (refObject == null) return;  
                    InHand = false;
                    if (IsInLeftHand) Plugin.Instance.InLeftHand = false;
                    else Plugin.Instance.InRightHand = false;
                    GorillaTagger.Instance.StartVibration(IsInLeftHand, 0.5f, 0.05f);
                    parent = Plugin.Instance.DropperCanvases[Plugin.Instance.DropperCanvases.Count - 1].transform;
                    gameObject.transform.SetParent(refObject.transform.parent, false);
                    gameObject.transform.localScale = Vector3.one;
                    GorillaTagger.Instance.offlineVRRig.tagSound.PlayOneShot(Plugin.Instance.DropError, 0.8f);
                    Destroy(refObject);
                }
            }
        }
    }
}
