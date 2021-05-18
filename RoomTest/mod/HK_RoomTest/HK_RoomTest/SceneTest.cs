using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Modding;
using ModCommon.Util;
using UnityEngine;
using GlobalEnums;
using On;

using UnityEngine.SceneManagement;


using System.Reflection;
using UnityEngine.SceneManagement;
using USceneManager = UnityEngine.SceneManagement.SceneManager;
using UObject = UnityEngine.Object;
using System.IO;
using HK_UnityHelper;

namespace HK_RoomTest
{
    public class SceneTest: Mod
    {
        public override string GetVersion() => "0.0.0.0";
        public AssetBundle SceneBundle;
        public AssetBundle AudioBundle;
        public AudioReplacer ar;



        public override List<(string, string)> GetPreloadNames() {

            var preloadDict = new List<(string, string)> {
                ("Crossroads_47","RestBench"),
                ("Tutorial_01","_Props/Geo Rock 3"),
                ("Dream_01_False_Knight","dream_scene pieces/dream_overlay")
            };


            foreach (EnemyType e in Enum.GetValues(typeof(EnemyType))) {
                if (e == EnemyType.none) continue;
                preloadDict.Add((Enemy.getLoadSceneName(e),Enemy.getLoadName(e)));
            }

            return preloadDict;
        }

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects) {

            Log("Initializing scene mod...");
            SceneBundle = new AssetBundle();
            AudioBundle = new AssetBundle();
            ar = new AudioReplacer();

            Unload();

            ModHooks.Instance.AfterSavegameLoadHook += AfterSaveGameLoad;
            ModHooks.Instance.NewGameHook += AddComponent;

            Assembly asm = Assembly.GetExecutingAssembly();

            foreach (string res in asm.GetManifestResourceNames()) {
                using (Stream s = asm.GetManifestResourceStream(res)) {

                    string bundleName = Path.GetExtension(res).Substring(1);

                    if (bundleName == "scene_test") SceneBundle = AssetBundle.LoadFromStream(s);
                    else if (bundleName == "audio_test") AudioBundle = AssetBundle.LoadFromStream(s);


                }
            }

            LoadAudio();
            PreloadObjects(preloadedObjects);

            On.ChainSequence.Begin += ChainSequence_Begin;

            ar.Initialize();

            Log("Finished Adding Hooks...");
            
            SceneManager sm = GameObject.Find("_SceneManager").GetComponent<SceneManager>();

            


        }

        private void ChainSequence_Begin(On.ChainSequence.orig_Begin orig, ChainSequence self) {
            if(GameManager.instance.sceneName == "Opening_Sequence") self.SetAttr("sequences", new SkippableSequence[0]);
            orig(self);
        }



        private void LoadAudio() {
            Log("Loading Audio Clips...");

            var audioClips = AudioBundle.LoadAllAssets<AudioClip>();
            foreach (AudioClip a in audioClips) {
                ar.audioClips.Add(a.name, a);
                Log("Loaded Clip: " + a.name);
            }

            Log("Finished Loading Audio Clips");
        }

        private void PreloadObjects(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects) {

            LoadScene.preloadedObjects = new Dictionary<string, GameObject>();


            LoadScene.preloadedObjects.Add("bench", preloadedObjects["Crossroads_47"]["RestBench"]);
            LoadScene.preloadedObjects.Add("georock", preloadedObjects["Tutorial_01"]["_Props/Geo Rock 3"]);
            LoadScene.preloadedObjects.Add("dream overlay", preloadedObjects["Dream_01_False_Knight"]["dream_scene pieces/dream_overlay"]);


            foreach (EnemyType e in Enum.GetValues(typeof(EnemyType))) {
                if (e == EnemyType.none) continue;
                LoadScene.preloadedObjects.Add(Enemy.getName(e), preloadedObjects[Enemy.getLoadSceneName(e)][Enemy.getLoadName(e)]);
            }
        }

        private void AfterSaveGameLoad(SaveGameData data) => AddComponent();

        private void AddComponent() {
            GameManager.instance.gameObject.AddComponent<LoadScene>();
        }



        public void Unload() {
            ModHooks.Instance.AfterSavegameLoadHook -= AfterSaveGameLoad;
            ModHooks.Instance.NewGameHook -= AddComponent;

            //ReSharper disable once Unity.NoNullPropogation
            var x = GameManager.instance?.gameObject.GetComponent<LoadScene>();
            if (x == null) return;
            UObject.Destroy(x);
        }
    }
}
