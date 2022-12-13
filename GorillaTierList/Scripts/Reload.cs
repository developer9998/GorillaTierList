using UnityEngine;

namespace GorillaTierList.Scripts
{
    public class Reload : MonoBehaviour
    {
        public float LastTime = 0;
        public Material offMat = Resources.Load<Material>("objects/treeroom/materials/plastic");
        public Material onMat = Resources.Load<Material>("objects/treeroom/materials/pressed");

        internal void LateUpdate()
        {
            if (LastTime >= Time.time) gameObject.GetComponent<Renderer>().material = onMat;
            else gameObject.GetComponent<Renderer>().material = offMat;
        }

        internal void OnTriggerEnter(Collider collider)
        {
            if (LastTime >= Time.time) return;
            if (collider.GetComponent<GorillaTriggerColliderHandIndicator>() == null) return;

            ButtonActivation();

            var indicator = collider.GetComponent<GorillaTriggerColliderHandIndicator>();
            GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(67, indicator.isLeftHand, 0.05f);
            GorillaTagger.Instance.StartVibration(indicator.isLeftHand, GorillaTagger.Instance.tapHapticStrength / 2f, GorillaTagger.Instance.tapHapticDuration);
        }

        public void ButtonActivation()
        {
            LastTime = Time.time + 0.25f;
            Plugin.Instance.Reload();
        }
    }
}
