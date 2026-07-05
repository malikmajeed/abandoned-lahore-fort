using ForgottenFort.Core;
using ForgottenFort.Level;
using ForgottenFort.Player;
using UnityEngine;

namespace ForgottenFort.Core
{
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
        public float smoothSpeed = 10f;
        Transform target;
        Camera cam;
        bool snapped;

        void Awake()
        {
            cam = GetComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = GameConstants.ViewportHeightTiles * GameConstants.TileSize * 0.5f;
            cam.backgroundColor = new Color(0.05f, 0.04f, 0.04f);
        }

        void Start()
        {
            FindPlayer();
            SnapToTarget();
        }

        void FindPlayer()
        {
            if (target != null) return;
            var level = FindFirstObjectByType<LevelGenerator>();
            if (level?.Player != null)
                target = level.Player.transform;
            else
            {
                var player = FindFirstObjectByType<PlayerController>();
                if (player != null) target = player.transform;
            }
        }

        void SnapToTarget()
        {
            if (target == null) return;
            var p = target.position;
            transform.position = new Vector3(p.x, p.y, -10f);
            snapped = true;
        }

        void LateUpdate()
        {
            if (target == null) FindPlayer();
            if (target == null) return;

            if (!snapped)
                SnapToTarget();

            var desired = new Vector3(target.position.x, target.position.y, -10f);
            desired = ClampToMap(desired);
            transform.position = Vector3.Lerp(transform.position, desired, smoothSpeed * Time.deltaTime);
        }

        Vector3 ClampToMap(Vector3 camPos)
        {
            float halfH = cam.orthographicSize;
            float halfW = halfH * cam.aspect;
            float mapHalfW = FortLevelData.Width * GameConstants.TileSize * 0.5f;
            float mapHalfH = FortLevelData.Height * GameConstants.TileSize * 0.5f;

            float minX = -mapHalfW + halfW;
            float maxX = mapHalfW - halfW;
            float minY = -mapHalfH + halfH;
            float maxY = mapHalfH - halfH;

            if (minX > maxX) camPos.x = 0;
            else camPos.x = Mathf.Clamp(camPos.x, minX, maxX);

            if (minY > maxY) camPos.y = 0;
            else camPos.y = Mathf.Clamp(camPos.y, minY, maxY);

            return camPos;
        }
    }
}
