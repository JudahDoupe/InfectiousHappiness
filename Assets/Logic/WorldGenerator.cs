using Assets.Logic.World;
using UnityEngine;

namespace Assets.Logic
{
    public class WorldGenerator : MonoBehaviour
    {
        public GameObject BlockPrefab;
        public Character Character;

        private readonly Vector3 _r = Vector3.right;
        private readonly Vector3 _l = Vector3.left;
        private readonly Vector3 _f = Vector3.forward;
        private readonly Vector3 _b = Vector3.back;
        private readonly Vector3 _u = Vector3.up;
        private readonly Vector3 _d = Vector3.down;

        void Start () {

            Map.StartingVexel = Map.GetVoxel(new Vector3(13, 1, 0));

            //starting platform
            var currentVox = PlacePath(1, Map.StartingVexel.Position + _d, _f);
            currentVox = PlacePlatform(3, currentVox.Position + _l + _f);
            //S-bend
            currentVox = PlacePath(3, currentVox.Position + _l + _f, _f);
            currentVox = PlacePath(2, currentVox.Position + _r, _r);
            currentVox = PlacePath(2, currentVox.Position + _f, _f);
            currentVox = PlacePath(2, currentVox.Position + _l, _l);
            currentVox = PlacePath(2, currentVox.Position + _f, _f);
            currentVox = PlacePath(2, currentVox.Position + _r, _r);
            //hallway
            currentVox = PlacePath(2, currentVox.Position + _f, _r);
            currentVox = PlacePath(2, currentVox.Position + _f + _l, _u);
            currentVox = PlacePath(2, currentVox.Position + _f + _d, _f);
            currentVox = PlacePath(2, currentVox.Position + _l, _l);
            currentVox = PlacePath(3, currentVox.Position + _f, _f);
            //branch

            Character.StartCoroutine("MoveToVoxel", Map.StartingVexel);
        }

        Voxel PlacePlatform(int width, Vector3 startPosition)
        {
            Voxel vox = null;
            var infectionLevel = Map.TotalRegisteredBlocks;
            for (var i = 0; i < width; i++)
            {
                for (var j = 0; j < width; j++)
                {
                    vox = Map.GetVoxel(startPosition + Vector3.forward * i + Vector3.right * j);

                    PlaceBlock(vox, infectionLevel);
                }
            }
            return vox;
        }

        Voxel PlacePath(int length, Vector3 startPosition, Vector3 direction)
        {
            Voxel vox = null;
            for (var i = 0; i < length; i++)
            {
                vox = Map.GetVoxel(startPosition + direction * i);

                PlaceBlock(vox, Map.TotalRegisteredBlocks);
            }
            return vox;
        }

        void PlaceBlock(Voxel vox, int infectionLevel)
        {
            var obj = Instantiate(BlockPrefab, vox.Position, Quaternion.identity);
            obj.transform.parent = gameObject.transform;
            vox.Block = obj.GetComponent<Block>();

            vox.Block.InfectionLevel = infectionLevel;
            Map.RegisterBlock(vox.Block);
        }

    }
}
