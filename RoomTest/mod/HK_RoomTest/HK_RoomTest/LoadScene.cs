using System;
using System.Collections;
using GlobalEnums;
using UnityEngine;
using SFCore.MonoBehaviours;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using HK_UnityHelper;
using ModCommon.Util;

namespace HK_RoomTest {
    public class LoadScene : MonoBehaviour {

        public static AudioClip shadowDash;

        public static Dictionary<string, GameObject> preloadedObjects;

        private static Dictionary<string, GameObject> loadedEnemies = new Dictionary<string, GameObject>();

        private void Start() {
            On.GameManager.EnterHero += GameManagerOnEnterHero;
        }

        private void GameManagerOnEnterHero(On.GameManager.orig_EnterHero orig, GameManager self, bool additiveGateSearch) {


            if (self.GetAttr<GameManager, bool>("hazardRespawningHero")) {
                orig(self, additiveGateSearch);
                return;
            }

            Modding.Logger.Log("Entering Scene: " + GameManager.instance.sceneName);
            UnityEngine.Debug.Log("Entering scene");
            
            /*foreach (GameObject enemy in enemies.Values) {
                Destroy(enemy);
            }
            enemies.Clear();*/


            if (self.sceneName == "GG_Atrium") {
                CreateGateway("right test",
                    new Vector2(115f, 60.6f), new Vector2(1f, 4f),
                    "NewRoomTest", "left test",
                    true, false,
                    GameManager.SceneLoadVisualizations.Default);
            }
            else if (self.sceneName == "Tutorial_01") {
                Modding.Logger.Log("Redirecting Player");
                CreateGateway("test",
                    new Vector2(34.5f, 91f), new Vector2(10f, 4f),
                    "NewRoomTest", "left test",
                    true, false,
                    GameManager.SceneLoadVisualizations.Default);
            }
            else if (self.sceneName == "NewRoomTest") {
                UnityEngine.Debug.Log("Entering custom scene");

                CreateGateway("left test",
                    new Vector2(2f, 23.5f), new Vector2(1f, 4f),
                    "GG_Atrium", "right test",
                    false, true,
                    GameManager.SceneLoadVisualizations.Default);

                GameObject bench;

                GameObject benchPlaceholder = GameObject.Find("RestBench");
                bench = Instantiate(preloadedObjects["bench"]);
                bench.SetActive(true);
                bench.transform.position = benchPlaceholder.transform.position;

                GameObject georock;
                Modding.Logger.Log("Haven't crashed yet! 0");

                GameObject geoPlaceholder = GameObject.Find("GeoRock");
                georock = Instantiate(preloadedObjects["georock"]);
                georock.SetActive(true);
                georock.transform.position = geoPlaceholder.transform.position;

                geoPlaceholder = GameObject.Find("GeoRock (1)");
                georock = Instantiate(preloadedObjects["georock"]);
                georock.SetActive(true);
                georock.transform.position = geoPlaceholder.transform.position;

                geoPlaceholder = GameObject.Find("GeoRock (2)");
                georock = Instantiate(preloadedObjects["georock"]);
                georock.SetActive(true);
                georock.transform.position = geoPlaceholder.transform.position;

                geoPlaceholder = GameObject.Find("GeoRock (3)");
                georock = Instantiate(preloadedObjects["georock"]);
                georock.SetActive(true);
                georock.transform.position = geoPlaceholder.transform.position;

                geoPlaceholder = GameObject.Find("GeoRock (4)");
                georock = Instantiate(preloadedObjects["georock"]);
                georock.SetActive(true);
                georock.transform.position = geoPlaceholder.transform.position;

                geoPlaceholder = GameObject.Find("GeoRock (5)");
                georock = Instantiate(preloadedObjects["georock"]);
                georock.SetActive(true);
                georock.transform.position = geoPlaceholder.transform.position;

                Modding.Logger.Log("Haven't crashed yet! 1");

                GameObject g = GameObject.Find("_Enemies");
                var enemyList = g.GetComponentsInChildren<Enemy>(); 
                Modding.Logger.Log("number of enemies " + enemyList.Length);
                foreach(Enemy e in enemyList) AddEnemy(e);

                GameObject dreamOverlay = Instantiate(preloadedObjects["dream overlay"]);
                dreamOverlay.transform.localScale *= 2f;
                dreamOverlay.SetActive(true);

                Modding.Logger.Log("Haven't crashed yet! 2");


                Material mat = new Material(Shader.Find("Sprites/Default"));
                GameObject Terrain = GameObject.Find("Grid/Tilemap");
                TilemapRenderer tr = Terrain.GetComponent<TilemapRenderer>();
                tr.material = mat;

                Modding.Logger.Log("Haven't crashed yet! 3");

                //AudioSource flower = GameObject.Find("Flower").GetComponent<AudioSource>();
                //flower.volume = GameManager.instance.GetImplicitCinematicVolume();

                //if (shadowDash != null) HeroController.instance.shadowDashClip = shadowDash;
            }
            orig(self, additiveGateSearch);

            //TODO: fix things so i can delete this
            //if(self.sceneName=="NewRoomTest") HeroController.instance.transform.position = new Vector3(3f,23.4f,0f);
        }

        public static void AddEnemy(Enemy enemyPlaceholder) {
            Modding.Logger.Log("AddEnemy called ");
            GameObject preloadedEnemy = null;

            foreach (EnemyType e in Enum.GetValues(typeof(EnemyType))) { //finds placeholder's type and instantiates prefab
                if (enemyPlaceholder.EnemyName == Enemy.getName(e)) {

                    Modding.Logger.Log("Spawning: " + enemyPlaceholder.EnemyName);

                    preloadedEnemy = Instantiate(preloadedObjects[Enemy.getName(e)]);
                    preloadedEnemy.SetActive(true);
                    preloadedEnemy.transform.position = enemyPlaceholder.transform.position;
                    break;
                }
            }

            if (enemyPlaceholder.EnemyName == "primalaspid") {


                PlayMakerFSM fsm = preloadedEnemy.LocateMyFSM("spitter");
                fsm.SetState("Init");
                PlayMakerFSM fsm2 = preloadedEnemy.LocateMyFSM("flyer_receive_direction_msg");
                fsm2.SetState("Idle");

                
            }
            if (loadedEnemies.ContainsKey(enemyPlaceholder.gameObject.name)) {
                Destroy(loadedEnemies[enemyPlaceholder.gameObject.name]);
                loadedEnemies[enemyPlaceholder.gameObject.name] = preloadedEnemy; }
            else loadedEnemies.Add(enemyPlaceholder.gameObject.name, preloadedEnemy);
            Destroy(enemyPlaceholder); //should prevent problems with duplicates
        }

        private void CreateGateway(string gateName, Vector2 pos, Vector2 size, string toScene, string entryGate,
            bool right, bool left, GameManager.SceneLoadVisualizations vis) {

            GameObject gate = new GameObject(gateName);
            gate.transform.SetPosition2D(pos);
            var tp = gate.AddComponent<TransitionPoint>();

            var bc = gate.AddComponent<BoxCollider2D>();
            bc.size = size;
            bc.isTrigger = true;
            tp.targetScene = toScene;
            tp.entryPoint = entryGate;

            tp.alwaysEnterLeft = left;
            tp.alwaysEnterRight = right;
            GameObject rm = new GameObject("Hazard Respawn Marker");
            rm.transform.parent = tp.transform;
            rm.transform.position = new Vector2(rm.transform.position.x + 3f, rm.transform.position.y);
            var tmp = rm.AddComponent<HazardRespawnMarker>();
            tp.respawnMarker = rm.GetComponent<HazardRespawnMarker>();
            tp.sceneLoadVisualization = vis;
        }
    }
}