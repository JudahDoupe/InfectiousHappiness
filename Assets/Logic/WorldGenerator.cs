using Assets.Logic.World;
using UnityEngine;

namespace Assets.Logic
{
    public class WorldGenerator : MonoBehaviour
    {
        public GameObject BlockPrefab;
        public GameObject SecondaryBlockPrefab;
        public GameObject BrainBlock;
        public Character Character;

        private readonly Vector3 _r = Vector3.right;
        private readonly Vector3 _l = Vector3.left;
        private readonly Vector3 _f = Vector3.forward;
        private readonly Vector3 _b = Vector3.back;
        private readonly Vector3 _u = Vector3.up;
        private readonly Vector3 _d = Vector3.down;

        void Start () {

            Map.StartingVoxel = Map.GetVoxel(new Vector3(75, 10, 5));

            //starting platform
            var currentVox = PlacePath(1, Map.StartingVoxel.Position + _d, _f);
            currentVox = PlacePlatform(3, 3, currentVox.Position + _l + _f);
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

            var intersection = currentVox;

            BlockPrefab = SecondaryBlockPrefab;
            //branch right
            currentVox = PlacePath(2, intersection.Position + _r, _r);
            currentVox = PlaceGate(currentVox.Position + _r, Vector3.right);
            currentVox = PlacePath(3, currentVox.Position + _r, _r);
            currentVox = PlacePath(2, currentVox.Position + _f, _f);
            currentVox = PlacePlatform(6, 5, currentVox.Position + _f + _l);

            //branch left
            currentVox = PlacePath(2, intersection.Position + _l, _l);
            currentVox = PlaceGate(currentVox.Position + _l, Vector3.right);

            //Switch sides



            Character.StartCoroutine("MoveToVoxel", Map.StartingVoxel);
        }

        Voxel PlacePlatform(int width, int length, Vector3 startPosition)
        {
            Voxel vox = null;
            var infectionLevel = Map.TotalRegisteredBlocks;
            for (var i = 0; i < length; i++)
            {
                for (var j = 0; j < width; j++)
                {
                    vox = Map.GetVoxel(startPosition + Vector3.forward * i + Vector3.right * j);

                    PlaceBlock(vox, BlockPrefab, infectionLevel);
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

                PlaceBlock(vox, BlockPrefab, Map.TotalRegisteredBlocks);
            }
            return vox;
        }

        Voxel PlaceGate(Vector3 startPosition, Vector3 direction)
        {
            var floor = PlaceBlock(Map.GetVoxel(startPosition),BlockPrefab, Map.TotalRegisteredBlocks).transform;
            floor.forward = direction;

            PlaceBlock(Map.GetVoxel(startPosition - floor.up), BrainBlock);
            PlaceBlock(Map.GetVoxel(startPosition - floor.up + floor.right), BrainBlock);
            PlaceBlock(Map.GetVoxel(startPosition - floor.up - floor.right), BrainBlock);
            PlaceBlock(Map.GetVoxel(startPosition + floor.right), BrainBlock);
            PlaceBlock(Map.GetVoxel(startPosition - floor.right), BrainBlock);
            PlaceBlock(Map.GetVoxel(startPosition + floor.up + floor.right), BrainBlock);
            PlaceBlock(Map.GetVoxel(startPosition + floor.up - floor.right), BrainBlock);
            PlaceBlock(Map.GetVoxel(startPosition + floor.up * 2 + floor.right), BrainBlock);
            PlaceBlock(Map.GetVoxel(startPosition + floor.up * 2 - floor.right), BrainBlock);
            PlaceBlock(Map.GetVoxel(startPosition + floor.up * 2), BrainBlock);

            return Map.GetVoxel(floor.transform.position);
        }

        Block PlaceBlock(Voxel vox, GameObject block, int? infectionLevel = null)
        {
            var obj = Instantiate(block, vox.Position, Quaternion.identity);
            obj.transform.parent = gameObject.transform;
            vox.Block = obj.GetComponent<Block>();

            if (infectionLevel == null) return vox.Block;

            Map.RegisterBlock(vox.Block);
            vox.Block.InfectionLevel = infectionLevel.Value;

            return vox.Block;
        }

    }
}
