using Assets.Logic.World;
using UnityEngine;

namespace Assets.Logic
{
    public class WorldGenerator : MonoBehaviour
    {
        public GameObject BlockPrefab;
        public Character Character;

        void Start () {

            Map.StartingVexel = Map.GetVoxel(new Vector3(13, 1, 0));

            var currentVox = PlacePath(5, Map.StartingVexel.Position + Vector3.down, Vector3.forward, 1);
            currentVox = PlacePath(3, currentVox.Position + Vector3.right, Vector3.forward, 6);
            currentVox = PlacePath(4, currentVox.Position + Vector3.left, Vector3.left, 11);

            Character.StartCoroutine("MoveToVoxel", Map.StartingVexel);
        }

        Voxel PlacePath(int length, Vector3 startPosition, Vector3 direction, int startingInfectionLevel)
        {
            Voxel vox = null;
            for (var i = 0; i < length; i++)
            {
                vox = Map.GetVoxel(startPosition + direction * i);

                PlaceBlock(vox, startingInfectionLevel+i);
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
