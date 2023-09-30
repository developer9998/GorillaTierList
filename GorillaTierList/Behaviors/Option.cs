using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using GorillaLocomotion;

namespace GorillaTierList.Behaviors
{
    public class Option : MonoBehaviour
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
                var nearest = Main.Instance.GetNearest(IsInLeftHand);
                Transform target = (nearest != null && Vector3.Distance(Player.Instance.bodyCollider.transform.position, Main.Instance.tierObject.transform.position) <= 8f) 
                    ? ((Vector3.Distance(Player.Instance.bodyCollider.transform.position, Main.Instance.ReturnDropper.transform.position) <= 1.6f) 
                        ? Main.Instance.DropperCanvases[0].transform 
                        : nearest) 
                    : Main.Instance.DropperCanvases[0].transform;
                refObject.transform.SetParent(target, false);
                refObject.transform.localScale = Vector3.one;
            }

            bool leftHandDown = ControllerInputPoller.instance.leftControllerIndexFloat > 0.5f;
            bool shouldBeInLeftHand = leftHandDown && Vector3.Distance(Player.Instance.leftControllerTransform.transform.position, gameObject.transform.position) < 0.2f;
            if (!InHand && !Main.Instance.InLeftHand && shouldBeInLeftHand)
            {
                InHand = true;
                IsInLeftHand = true;
                Main.Instance.InLeftHand = true;
                GorillaTagger.Instance.StartVibration(true, 1, 0.05f);
                GorillaTagger.Instance.offlineVRRig.tagSound.PlayOneShot(Main.Instance.Pick, 1.25f);
                gameObject.transform.SetParent(Main.Instance.HandCanvases[0], false);
                ResetTransform();
                gameObject.transform.localScale = Main.Instance.DropperSizeLocal;

                if (refObject == null)
                {
                    refObject = Instantiate(Main.Instance.refObject);
                    refObject.GetComponentInChildren<Text>().text = gameObject.transform.GetComponentInChildren<Text>().text;
                }
                return;
            }

            bool rightHandDown = ControllerInputPoller.instance.rightControllerIndexFloat > 0.5f;
            bool shouldBeInRightHand = rightHandDown && Vector3.Distance(Player.Instance.rightControllerTransform.transform.position, gameObject.transform.position) < 0.2f;
            if (!InHand && !Main.Instance.InRightHand && shouldBeInRightHand)
            {
                InHand = true;
                IsInLeftHand = false;
                Main.Instance.InRightHand = true;
                GorillaTagger.Instance.StartVibration(false, 1, 0.05f);
                GorillaTagger.Instance.offlineVRRig.tagSound.PlayOneShot(Main.Instance.Pick, 1.25f);
                gameObject.transform.SetParent(Main.Instance.HandCanvases[1], false);
                ResetTransform();
                gameObject.transform.localScale = Main.Instance.DropperSizeLocal;

                if (refObject == null)
                {
                    refObject = Instantiate(Main.Instance.refObject);
                    refObject.GetComponentInChildren<Text>().text = gameObject.transform.GetComponentInChildren<Text>().text;
                }
                return;
            }

            if (InHand)
            {
                // Drop (Left)
                if (IsInLeftHand && !leftHandDown)
                {
                    if (refObject == null) return;

                    if (refObject.transform.parent.childCount >= 13 && refObject.transform.parent != Main.Instance.DropperCanvases[0].transform)
                    {
                        InHand = false;
                        if (IsInLeftHand) Main.Instance.InLeftHand = false;
                        else Main.Instance.InRightHand = false;
                        GorillaTagger.Instance.StartVibration(IsInLeftHand, 0.5f, 0.05f);
                        parent = Main.Instance.DropperCanvases[0].transform;
                        gameObject.transform.SetParent(parent, false);
                        gameObject.transform.localScale = Vector3.one;
                        GorillaTagger.Instance.offlineVRRig.tagSound.PlayOneShot(Main.Instance.DropError, 0.8f);
                        Destroy(refObject);
                        return;
                    }
  
                    InHand = false;
                    Main.Instance.InLeftHand = false;
                    GorillaTagger.Instance.StartVibration(true, 0.5f, 0.05f);
                    parent = Main.Instance.GetNearest(true) ?? Main.Instance.DropperCanvases[^2].transform;
                    gameObject.transform.SetParent(refObject.transform.parent, false);
                    gameObject.transform.localScale = Vector3.one;
                    GorillaTagger.Instance.offlineVRRig.tagSound.PlayOneShot(Main.Instance.Drop, 1.25f);
                    Destroy(refObject);
                }
                // Drop (Right)
                if (!IsInLeftHand && !rightHandDown)
                {
                    if (refObject == null) return;

                    if (refObject.transform.parent.childCount >= 13 && refObject.transform.parent != Main.Instance.DropperCanvases[0].transform)
                    {
                        InHand = false;
                        if (IsInLeftHand) Main.Instance.InLeftHand = false;
                        else Main.Instance.InRightHand = false;
                        GorillaTagger.Instance.StartVibration(IsInLeftHand, 0.5f, 0.05f);
                        parent = Main.Instance.DropperCanvases[0].transform;
                        gameObject.transform.SetParent(parent, false);
                        gameObject.transform.localScale = Vector3.one;
                        GorillaTagger.Instance.offlineVRRig.tagSound.PlayOneShot(Main.Instance.DropError, 0.8f);
                        Destroy(refObject);
                        return;
                    }

                    InHand = false;
                    Main.Instance.InRightHand = false;
                    GorillaTagger.Instance.StartVibration(false, 0.5f, 0.05f);
                    parent = Main.Instance.GetNearest(false) ?? Main.Instance.DropperCanvases[^2].transform;
                    gameObject.transform.SetParent(refObject.transform.parent, false);
                    gameObject.transform.localScale = Vector3.one;
                    GorillaTagger.Instance.offlineVRRig.tagSound.PlayOneShot(Main.Instance.Drop, 1.25f);
                    Destroy(refObject);
                }
                // Drop (When you get too far away from the tier list object)
                if (Vector3.Distance(Player.Instance.bodyCollider.transform.position, Main.Instance.tierObject.transform.position) >= 5)
                {
                    if (refObject == null) return;
  
                    InHand = false;
                    if (IsInLeftHand) Main.Instance.InLeftHand = false;
                    else Main.Instance.InRightHand = false;
                    GorillaTagger.Instance.StartVibration(IsInLeftHand, 0.5f, 0.05f);
                    parent = Main.Instance.DropperCanvases[^1].transform;
                    gameObject.transform.SetParent(refObject.transform.parent, false);
                    gameObject.transform.localScale = Vector3.one;
                    GorillaTagger.Instance.offlineVRRig.tagSound.PlayOneShot(Main.Instance.DropError, 0.8f);
                    Destroy(refObject);
                }
            }
        }
    }
}
