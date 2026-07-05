using UnityEngine;

namespace ForgottenFort.Level
{
    [CreateAssetMenu(fileName = "LevelPrefabLibrary", menuName = "Forgotten Fort/Level Prefab Library")]
    public class LevelPrefabLibrary : ScriptableObject
    {
        public GameObject floorPrefab;
        public GameObject wallPrefab;
        public GameObject playerPrefab;
        public GameObject guardPrefab;
        public GameObject treasurePrefab;
        public GameObject keyPrefab;
        public GameObject mosaicPrefab;
        public GameObject doorPrefab;
        public GameObject doorOpenPrefab;
        public GameObject torchPrefab;
        public GameObject barrelPrefab;

        public void ApplyTo(LevelGenerator generator)
        {
            if (generator.floorPrefab == null) generator.floorPrefab = floorPrefab;
            if (generator.wallPrefab == null) generator.wallPrefab = wallPrefab;
            if (generator.playerPrefab == null) generator.playerPrefab = playerPrefab;
            if (generator.guardPrefab == null) generator.guardPrefab = guardPrefab;
            if (generator.treasurePrefab == null) generator.treasurePrefab = treasurePrefab;
            if (generator.keyPrefab == null) generator.keyPrefab = keyPrefab;
            if (generator.mosaicPrefab == null) generator.mosaicPrefab = mosaicPrefab;
            if (generator.doorPrefab == null) generator.doorPrefab = doorPrefab;
            if (generator.doorOpenPrefab == null) generator.doorOpenPrefab = doorOpenPrefab;
            if (generator.torchPrefab == null) generator.torchPrefab = torchPrefab;
            if (generator.barrelPrefab == null) generator.barrelPrefab = barrelPrefab;
        }
    }
}
