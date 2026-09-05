using UnityEngine;

namespace ResourceMonitor.Components
{
    /**
    * Fakes the "spinning coin" look Subnautica's own loading-screen Alterra logo uses (uGUI_Logo) -
    * a flat image rotating around a vertical axis, squashing horizontally as it turns edge-on -
    * without needing an actual perspective-projected mesh. Just scales the RectTransform's X by
    * |cos(angle)| each frame, which reads as a 3D spin for a single flat sprite.
    */
    public class SpinningLogo : MonoBehaviour
    {
        private static readonly float ROTATION_SPEED = 90f; // degrees per second
        private float angle;

        private void Update()
        {
            angle += ROTATION_SPEED * Time.deltaTime;
            if (angle >= 360f)
            {
                angle -= 360f;
            }

            var scale = transform.localScale;
            scale.x = Mathf.Abs(Mathf.Cos(angle * Mathf.Deg2Rad));
            transform.localScale = scale;
        }
    }
}
