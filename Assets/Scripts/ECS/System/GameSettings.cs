﻿using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Minecraft
{
    public class GameSettings : MonoBehaviour
    {
        public static GameSettings GM;
        public static Texture2D Heightmap;

        public static EntityArchetype BlockArchetype;

        [Header("World = ChunkBase x ChunkBase")]
        public int ChunkBase = 1;

        [Header("Mesh Info")]
        public Mesh blockMesh;
        public Mesh surfaceMesh;
        public Mesh tallGrassMesh;

        [Header("Nature Block Type")]
        public Material stoneMaterial;
        public Material woodMaterial;
        public Material leavesMaterial;
        public Material surfaceMaterial;
        public Material cobbleMaterial;
        public Material dirtMaterial;
        public Material tallGrassMaterial;
        public Material roseMaterial;

        [Header("Other Block Type")]
        public Material glassMaterial;
        public Material brickMaterial;
        public Material plankMaterial;
        public Material tntMaterial;
        [Header("")]
        public Material pinkMaterial;

        [Header("Collision Settings")]
        public float playerCollisionRadius;

        /*
         [Header("For Debug")]
        public Material no1Mat;
        public Material no2Mat;
        public Material no3Mat;
        public Material no4Mat;
        public Material no5Mat;
        public Material no6Mat;
        public Material no0Mat;
        public Material noQMat;
        */

        int ranDice;
        Material maTemp;
        Mesh meshTemp;

        void Awake()
        {
            if (GM != null && GM != this)
                Destroy(gameObject);
            else
                GM = this;
        }

        public EntityManager manager;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {
            // This method creates archetypes for entities we will spawn frequently in this game.
            // Archetypes are optional but can speed up entity spawning substantially.

            EntityManager manager = World.Active.GetOrCreateManager<EntityManager>();

            // Create an archetype for basic blocks.
            BlockArchetype = manager.CreateArchetype(
                typeof(TransformMatrix),
                typeof(Position)
                //typeof(ColliderChecker)
            );
            //typeof(MeshInstanceRenderer));
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        void Start()
        {
            manager = World.Active.GetOrCreateManager<EntityManager>();
            //Generate the world
            ChunkGenerator(ChunkBase);
        }

        void ChunkGenerator(int amount)
        {

            int totalamount = (amount * amount) * 1500;
            //int ordernumber = 0;
            int hightlevel;
            bool airChecker;

            //NativeArray<Entity> entities = new NativeArray<Entity>(totalamount, Allocator.Temp);
            //manager.Instantiate(blockPrefab, entities);



            //for (int i = 0; i < amount; i++)
            //{

            //if (ordernumber == totalamount)
            //return;

            //Block ordering from X*0,0,0 to 15,10,10( * Chunk x2)
            for (int yBlock = 0; yBlock < 15; yBlock++)
            {
                for (int xBlock = 0; xBlock < 10 * amount; xBlock++)
                {
                    for (int zBlock = 0; zBlock < 10 * amount; zBlock++)
                    {
                        hightlevel = (int)(Heightmap.GetPixel(xBlock, zBlock).r * 100) - yBlock;
                        airChecker = false;

                        switch (hightlevel)
                        {
                            case 0:
                                //random surface block
                                ranDice = Random.Range(1, 201);
                                if (ranDice <= 20)
                                {
                                    //grass
                                    PlantGenerator(xBlock, yBlock, zBlock, 1);

                                }
                                if (ranDice == 200)
                                {
                                    //rose
                                    PlantGenerator(xBlock, yBlock, zBlock, 2);
                                }
                                if (ranDice == 199)
                                {
                                    //tree
                                    TreeGenerator(xBlock, yBlock, zBlock);


                                }
                                airChecker = true;
                                break;
                            case 1:
                                meshTemp = surfaceMesh;
                                maTemp = surfaceMaterial;
                                //not a good way to add collider, so it's temporary.
                                Vector3 posTemp = new Vector3(xBlock, yBlock, zBlock);
                                GM.GetComponent<ColliderPool>().AddCollider(posTemp);
                                break;
                            case 2:
                            case 3:
                            case 4:
                                //Dirt
                                meshTemp = blockMesh;
                                maTemp = dirtMaterial;
                                break;
                            case 5:
                            case 6:
                                //stone block
                                meshTemp = blockMesh;
                                maTemp = stoneMaterial;
                                break;
                            case 7:
                            case 8:
                                meshTemp = blockMesh;
                                maTemp = cobbleMaterial;
                                break;
                            default:
                                //airBlock or anything hight level < 0
                                airChecker = true;

                                break;

                        }

                        if (!airChecker)
                        {

                            if (!maTemp)
                                maTemp = pinkMaterial;

                            Entity entities = manager.CreateEntity(BlockArchetype);
                            manager.SetComponentData(entities, new Position { Value = new int3(xBlock, yBlock, zBlock) });
                            //manager.AddComponentData(entities, new BlockTag {});

                            manager.AddSharedComponentData(entities, new MeshInstanceRenderer
                            {
                                mesh = meshTemp,
                                material = maTemp
                            });

                        }
                    }
                }
            }
            //}
            //entities.Dispose();
        }
        void TreeGenerator(int xPos, int yPos, int zPos)
        {
            //xpos,ypos,zpos is the root position of the tree that we are going to plant.
            //woods
            for (int i = yPos; i < yPos + 7; i++)
            {
                //top leaves
                if (i == yPos + 6)
                {
                    maTemp = leavesMaterial;
                }
                else
                {
                    maTemp = woodMaterial;
                }

                if (!maTemp)
                    maTemp = pinkMaterial;

                //this is a temporary line.
                Vector3 posTemp = new Vector3(xPos, i, zPos);
                GM.GetComponent<ColliderPool>().AddCollider(posTemp);

                Entity entities = manager.CreateEntity(BlockArchetype);
                manager.SetComponentData(entities, new Position { Value = new int3(xPos, i, zPos) });
                //manager.AddComponentData(entities, new BlockTag { });
                //manager.AddComponentData(entities, new ColliderChecker{ State = 0 });
                manager.AddSharedComponentData(entities, new MeshInstanceRenderer
                {
                    mesh = blockMesh,
                    material = maTemp
                });

                //leaves
                if(i >= yPos+3 && i <= yPos+6)
                {
                    for (int j = xPos - 1; j <= xPos + 1; j++)
                    {
                        for (int k = zPos - 1; k <= zPos + 1; k++)
                        {
                            if (k != zPos || j != xPos)
                            {
                                //this is a temporary line.
                                posTemp = new Vector3(j, i, k);
                                GM.GetComponent<ColliderPool>().AddCollider(posTemp);

                                entities = manager.CreateEntity(BlockArchetype);
                                manager.SetComponentData(entities, new Position { Value = new int3(j, i, k) });
                                //manager.AddComponentData(entities, new BlockTag { });
                                //manager.AddComponentData(entities, new HasCollider { ColliderState = false });
                                manager.AddSharedComponentData(entities, new MeshInstanceRenderer
                                {
                                    mesh = blockMesh,
                                    material = leavesMaterial
                                });
                            }
                        }
                    }
                }
            }
        }

        void PlantGenerator(int xPos, int yPos, int zPos,int plantType)
        {


            //xpos,ypos,zpos is the root position of the plant that we are going to build.
            //rose
            if (plantType == 1)
            {
                maTemp = tallGrassMaterial;
            }
            else
            {
                maTemp = roseMaterial;
            }

            if (!maTemp)
                maTemp = pinkMaterial;

            Quaternion rotation = Quaternion.Euler(0, 45, 0);
            Entity entities = manager.CreateEntity(BlockArchetype);
            manager.SetComponentData(entities, new Position { Value = new int3(xPos, yPos, zPos) });
            manager.AddComponentData(entities, new Rotation { Value = rotation });
            manager.AddSharedComponentData(entities, new MeshInstanceRenderer
            {
                mesh = tallGrassMesh,
                material = maTemp
            });
        }
    }
}
