﻿using Assets.Logic.World;
using UnityEngine;

namespace Assets.Logic
{
    public class WorldGenerator : MonoBehaviour
    {
        public static WorldGenerator Instance;

        public GameObject BlockPrefab;
        public GameObject SecondaryBlockPrefab;
        public GameObject BrainBlock;
        public GameObject MovableBlock;
        public GameObject BounceBlock;
        public GameObject GoalBlock;
        public Character Character;

        private readonly Vector3 _r = Vector3.right;
        private readonly Vector3 _l = Vector3.left;
        private readonly Vector3 _f = Vector3.forward;
        private readonly Vector3 _b = Vector3.back;
        private readonly Vector3 _u = Vector3.up;
        private readonly Vector3 _d = Vector3.down;

        void Start()
        {
            Instance = this;
            Go();

        }

        public void Go () {

            Map.StartingVoxel = Map.GetVoxel(new Vector3(75, 10, 5));

            //starting platform
            var currentVox = PlacePlatform(3, 3, Map.StartingVoxel.Position + _l + _b + _d);

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
            currentVox = PlacePath(3, currentVox.Position + _f + _d, _l);
            currentVox = PlacePath(3, currentVox.Position + _f, _f);

            var intersection = currentVox;
            var intersectionLevel = Map.TotalRegisteredBlocks;

            //branch right
            currentVox = PlacePath(2, intersection.Position + _r, _r, intersectionLevel);
            currentVox = PlaceGate(currentVox.Position + _r, _r, intersectionLevel);
            currentVox = PlacePath(3, currentVox.Position + _r, _r, intersectionLevel);
            currentVox = PlacePath(2, currentVox.Position + _f, _f, intersectionLevel);

            //puzzle 1
            PlaceBlock(Map.GetVoxel(currentVox.Position + _f * 2 + _r * 3 + _u), BlockPrefab, Map.TotalRegisteredBlocks + 2);
            PlaceBlock(Map.GetVoxel(currentVox.Position + _f * 4 + _r * 3 + _u), BlockPrefab, Map.TotalRegisteredBlocks + 2);
            currentVox = PlacePlatform(6, 5, currentVox.Position + _f + _l);
            PlaceBlock(Map.GetVoxel(currentVox.Position + _b * 2 + _l), BounceBlock);
            PlaceBlock(Map.GetVoxel(currentVox.Position + _b * 2 + _l * 3 + _u), MovableBlock);
            PlaceBlock(Map.GetVoxel(currentVox.Position + _b * 2), GoalBlock, Map.TotalRegisteredBlocks);

            //branch left
            currentVox = PlacePath(2, intersection.Position + _l, _l, intersectionLevel);
            currentVox = PlaceGate(currentVox.Position + _l, Vector3.right, intersectionLevel);
            PlacePath(2, currentVox.Position + _l, _l);
            currentVox = PlacePath(5, currentVox.Position + _l, _b);
            var gateLevel = Map.TotalRegisteredBlocks + 1;
            PlaceBlock(Map.GetVoxel(currentVox.Position + _f * 4 + _u + _r), MovableBlock);
            currentVox = PlacePath(2, currentVox.Position + _f + _l, _l);
            
            // intersection 2
            intersection = currentVox;
            intersectionLevel = Map.TotalRegisteredBlocks;

            //Puzzle 2

            currentVox = PlacePath(2, intersection.Position , _b, intersectionLevel);
            currentVox = PlaceGate(currentVox.Position + _b, _f, intersectionLevel);
            PlaceBlock(Map.GetVoxel(currentVox.Position + _b * 3 + _r * 3 + _u + _l), MovableBlock);
            var puzzleLevel = Map.TotalRegisteredBlocks + 9;
            PlacePath(1, currentVox.Position + _b * 5 + _r * 2 + _u, _u, puzzleLevel);
            PlacePath(2, currentVox.Position + _b * 5 + _r + _u, _u, puzzleLevel);
            PlacePath(3, currentVox.Position + _b * 5 + _u, _u, puzzleLevel);
            var goal = PlacePath(3, currentVox.Position + _b * 5 + _l * 2 + _u, _u, puzzleLevel);
            PlaceBlock(goal, GoalBlock, Map.TotalRegisteredBlocks + 2);
            currentVox = PlacePlatform(7, 6, currentVox.Position + _b * 6 + _l * 3);
            PlaceBlock(Map.GetVoxel(currentVox.Position + _b * 4 + _l * 4), BounceBlock);


            //Puzzle 3 path
            currentVox = PlaceGate(intersection.Position + _l, _l, intersectionLevel);
            currentVox = PlacePath(3, currentVox.Position + _b + _l, _f, intersectionLevel);
            PlaceBlock(Map.GetVoxel(currentVox.Position + _b * 2 + _u), MovableBlock);
            currentVox = PlacePath(2, currentVox.Position + _l, _l, intersectionLevel);

            //Puzzle 3
            intersection = currentVox;
            intersectionLevel = Map.TotalRegisteredBlocks + 105;
            PlacePath(2, intersection.Position + _u, _u, intersectionLevel);
            PlacePath(3, intersection.Position + _u + _f, _u, intersectionLevel);
            PlacePath(1, intersection.Position + _u + _f + _l, _u, intersectionLevel);
            PlaceBlock(Map.GetVoxel(intersection.Position + _u + _f + _l * 4), MovableBlock);
            //row 3
            PlacePath(3, intersection.Position + _u + _f * 3, _u, intersectionLevel);
            PlacePath(3, intersection.Position + _u + _f * 3 + _l, _u, intersectionLevel);
            PlacePath(3, intersection.Position + _u + _f * 3 + _l * 2, _u, intersectionLevel);
            PlacePath(2, intersection.Position + _u + _f * 3 + _l * 3, _u, intersectionLevel);
            PlacePath(2, intersection.Position + _u + _f * 3 + _l * 4, _u, intersectionLevel);
            PlacePath(1, intersection.Position + _u + _f * 3 + _l * 5, _u, intersectionLevel);
            PlacePath(1, intersection.Position + _u + _f * 3 + _l * 6, _u, intersectionLevel);
            //row 4
            PlacePath(5, intersection.Position + _u + _f * 4, _u, intersectionLevel);
            PlacePath(4, intersection.Position + _u + _f * 4 + _l * 2, _u, intersectionLevel);
            PlacePath(5, intersection.Position + _u + _f * 4 + _l * 3, _u, intersectionLevel);
            PlacePath(3, intersection.Position + _u + _f * 4 + _l * 4, _u, intersectionLevel);
            PlacePath(2, intersection.Position + _u + _f * 4 + _l * 5, _u, intersectionLevel);
            PlacePath(2, intersection.Position + _u + _f * 4 + _l * 6, _u, intersectionLevel);
            //row 5
            PlacePath(5, intersection.Position + _u + _f * 5, _u, intersectionLevel);
            PlacePath(2, intersection.Position + _u + _f * 5 + _l, _u, intersectionLevel);
            PlacePath(0, intersection.Position + _u + _f * 5 + _l * 2, _u, intersectionLevel);
            PlacePath(4, intersection.Position + _u + _f * 5 + _l * 3, _u, intersectionLevel);
            PlacePath(3, intersection.Position + _u + _f * 5 + _l * 4, _u, intersectionLevel);
            PlacePath(2, intersection.Position + _u + _f * 5 + _l * 5, _u, intersectionLevel);
            PlacePath(1, intersection.Position + _u + _f * 5 + _l * 6, _u, intersectionLevel);
            //row 6
            PlacePath(6, intersection.Position + _u + _f * 6, _u, intersectionLevel);
            PlacePath(7, intersection.Position + _u + _f * 6 + _l, _u, intersectionLevel);
            PlacePath(8, intersection.Position + _u + _f * 6 + _l * 2, _u, intersectionLevel);
            PlacePath(9, intersection.Position + _u + _f * 6 + _l * 3, _u, intersectionLevel);
            PlacePath(6, intersection.Position + _u + _f * 6 + _l * 4, _u, intersectionLevel);
            PlacePath(5, intersection.Position + _u + _f * 6 + _l * 5, _u, intersectionLevel);

            PlacePlatform(7, 7, intersection.Position + _l * 6 + _f);
            PlaceBlock(Map.GetVoxel(intersection.Position + _f * 2 + _u ), BounceBlock);
            PlaceBlock(Map.GetVoxel(intersection.Position + _f * 4 + _l + _u * 2), BounceBlock); 
            PlaceBlock(Map.GetVoxel(intersection.Position + _f * 5 + _l * 2 + _u * 3), BounceBlock);
            PlaceBlock(Map.GetVoxel(intersection.Position + _f * 3 + _u * 4 + _r), MovableBlock);

            //row 2
            RemoveBlock(Map.GetVoxel(intersection.Position + _f * 2 + _l));
            RemoveBlock(Map.GetVoxel(intersection.Position + _f * 2 + _l * 2));
            RemoveBlock(Map.GetVoxel(intersection.Position + _f * 2 + _l * 3));
            RemoveBlock(Map.GetVoxel(intersection.Position + _f * 2 + _l * 4));
            RemoveBlock(Map.GetVoxel(intersection.Position + _f * 2 + _l * 5));

            //Path to other side


            currentVox = PlaceGate(intersection.Position + _u * 6 + _f * 6 + _r, _r);
            currentVox = PlacePath(1, currentVox.Position + _r, _r);
            //currentVox = PlaceGate(currentVox.Position + _f, _f);
            currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _d), BlockPrefab);
            currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _f), BlockPrefab);
            currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _r), BlockPrefab);
            currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _d), BlockPrefab);
            currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _f), BlockPrefab);
            currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _r), BlockPrefab);
            currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _d), BlockPrefab);
            currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _f), BlockPrefab);
            currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _r), BlockPrefab);
            currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _d), BlockPrefab);
            currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _r), BlockPrefab);
            currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _d), BlockPrefab);
            currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _r), BlockPrefab);
            currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _d), BlockPrefab);
            currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _r), BlockPrefab);
            currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _r), BlockPrefab);
            currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _r), BlockPrefab);

            currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _b * 3), BlockPrefab);
            currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _b), BlockPrefab);
            currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _f + _u), BlockPrefab);
            currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _u), BlockPrefab);

            currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _f * 4 + _d * 2), BlockPrefab);
            currentVox = PlaceBlock2(Map.GetVoxel(Map.StartingVoxel.Position + Vector3.forward - Vector3.up),BounceBlock);


            /*
           //Switch sides

           //TurnRight brain yeeeeah
           //****hint block here****
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _u * 10), BlockPrefab);

           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _l + _d), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _f), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _f), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _r + _u), BlockPrefab);

           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _d * 10), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _u + _l), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _u + _l), BlockPrefab);

           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _u + _l), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _u + _l), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _f), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _u + _l), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _f + _l), BlockPrefab);

           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _r * 10), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _b), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _u + _l), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _b), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _b), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _b), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _d + _r), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _b), BlockPrefab);

           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _l * 9), BlockPrefab);
           //****hint block here****
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _f * 2), BlockPrefab);


           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _f * 4), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _d + _r), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _f), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _d + _r), BlockPrefab);

           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _d + _r), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _d + _r), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _f), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _d + _r), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _f + _u), MovableBlock);

           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _u * 10), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _f), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _d + _r), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _d + _r), BlockPrefab);

           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _d + _r), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _d + _r), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _d + _r), BlockPrefab);

           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _l * 10), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _f), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _d + _r), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _d + _r), BlockPrefab);

           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _d + _r), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _f), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _d + _r), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _d + _r), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _f), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _f), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _f), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _f), BlockPrefab);

           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _u * 10), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _d + _r), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _b), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _d + _r), BlockPrefab);

           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _d + _r), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _d + _r), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _b), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _d + _r), BlockPrefab);

           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _l * 10), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _f), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _f), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _f), BlockPrefab);

           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _r * 10), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _f), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _f), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _u + _l), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _u + _l), BlockPrefab);

           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _u + _l), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _f), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _u + _l), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _f), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _u + _l), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _f), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _f), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _d), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _d), BlockPrefab);

           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _u * 2 + _b * 37), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _u * 3 + _f), BounceBlock);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _d * 3 + _f), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _d * 3), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _f * 3), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _u * 3), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _f), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _f), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _d), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _d), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _f * 3), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _u + _f), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _f), BlockPrefab);

           // SWITCH DEBUGGING PLEASE DELETE BEFORE EXPORT********************************************************************************************************************************************
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _b * 26), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _f), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _f * 2), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _f + _u * 2), BounceBlock);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _f + _d * 2), BlockPrefab);
           currentVox = PlaceBlock2(Map.GetVoxel(currentVox.Position + _f * 3), BlockPrefab);
           // SRSLY***********************************************************************************************************************************************************************************

           */

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

        Voxel PlacePath(int length, Vector3 startPosition, Vector3 direction, int? infectionLevel = null)
        {
            Voxel vox = null;

            for (var i = 0; i < length; i++)
            {
                vox = Map.GetVoxel(startPosition + direction * i);

                var iLevel = infectionLevel ?? Map.TotalRegisteredBlocks;
                PlaceBlock(vox, BlockPrefab, iLevel);
            }
            return vox;
        }

        Voxel PlaceGate(Vector3 startPosition, Vector3 direction, int? infectionLevel = null)
        {
            var iLevel = infectionLevel ?? Map.TotalRegisteredBlocks;
            var floor = PlaceBlock(Map.GetVoxel(startPosition),BlockPrefab, iLevel).transform;
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

        Block PlaceBlock(Voxel vox, GameObject block, int infectionLevel = -1)
        {
            Map.PlaceNewBlock(vox.Position, block, infectionLevel);

            return vox.GetBlock();
        }

        Voxel PlaceBlock2(Voxel vox, GameObject block, int infectionLevel = -1)
        {
            Map.PlaceNewBlock(vox.Position, block, infectionLevel);

            return vox;
        }

        void RemoveBlock(Voxel vox)
        {
            vox.Destroy();
        }
    }
}
