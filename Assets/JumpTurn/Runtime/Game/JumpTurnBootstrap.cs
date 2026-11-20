using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace JumpTurn
{
    public sealed class JumpTurnBootstrap : MonoBehaviour
    {
        [SerializeField] private Camera worldCamera;

        private void Awake()
        {
            if (FindFirstObjectByType<JumpTurnGame>() != null)
            {
                return;
            }

            if (worldCamera == null)
            {
                worldCamera = FindFirstObjectByType<Camera>();
            }

            if (worldCamera == null)
            {
                var cameraObject = new GameObject("Main Camera");
                cameraObject.tag = "MainCamera";
                worldCamera = cameraObject.AddComponent<Camera>();
                cameraObject.AddComponent<AudioListener>();
            }

            ConfigureCamera(worldCamera);
            var gameObject = new GameObject("JumpTurnGame");
            gameObject.transform.SetParent(transform.parent, false);
            var game = gameObject.AddComponent<JumpTurnGame>();
            game.Initialize(worldCamera);
        }

        private static void ConfigureCamera(Camera camera)
        {
            camera.orthographic = true;
            camera.orthographicSize = 11.25f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.02f, 0.035f, 0.055f, 1f);
            camera.allowHDR = false;
            camera.allowMSAA = false;
            camera.allowDynamicResolution = false;
            camera.transform.position = new Vector3(0f, 0f, -10f);
            camera.transform.rotation = Quaternion.identity;

            PixelPerfectCamera pixelPerfect = camera.GetComponent<PixelPerfectCamera>();
            if (pixelPerfect == null)
            {
                pixelPerfect = camera.gameObject.AddComponent<PixelPerfectCamera>();
            }

            pixelPerfect.assetsPPU = 16;
            pixelPerfect.refResolutionX = 640;
            pixelPerfect.refResolutionY = 360;
            pixelPerfect.gridSnapping = PixelPerfectCamera.GridSnapping.PixelSnapping;
            pixelPerfect.cropFrame = PixelPerfectCamera.CropFrame.Windowbox;
        }
    }
}
