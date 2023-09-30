using UnityEngine;

namespace GorillaTierList.Behaviors
{
    public class Reload : MonoBehaviour
    {
        public float LastTime = 0;

        internal void OnTriggerEnter(Collider collider)
        {
            if (LastTime >= Time.time) return;
            if (collider.GetComponent<GorillaTriggerColliderHandIndicator>() == null) return;

            LastTime = Time.time + 0.25f;
            Main.Instance.OccasionalUpdate();

            var indicator = collider.GetComponent<GorillaTriggerColliderHandIndicator>();
            GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(67, indicator.isLeftHand, 0.05f);
            GorillaTagger.Instance.StartVibration(indicator.isLeftHand, GorillaTagger.Instance.tapHapticStrength / 2f, GorillaTagger.Instance.tapHapticDuration);
        }
    }
}
