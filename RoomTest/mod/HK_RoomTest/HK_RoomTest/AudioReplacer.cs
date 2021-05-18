using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using GlobalEnums;
using UnityEngine;
using Modding;
using ModCommon.Util;
using HutongGames.PlayMaker.Actions;
using HK_UnityHelper;


using UnityEngine.SceneManagement;

namespace HK_RoomTest {
    public class AudioReplacer {
        

        public Dictionary<string, AudioClip> audioClips = new Dictionary<string, AudioClip>();

        public void Initialize() {
            //ModHooks.Instance.ObjectPoolSpawnHook += ObjectPoolSpawnHook;
            LoadScene.preloadedObjects = ModifyPreloads(LoadScene.preloadedObjects);
            AddHooks();
            

            ApplyMusicClip(audioClips["title"]);

            AudioSource[] al = GameObject.Find("Menu_Styles").GetComponentsInChildren<AudioSource>();
            foreach (AudioSource a in al) a.volume = 0f;


        }

        public void AddHooks() {
            ModHooks.Instance.ObjectPoolSpawnHook += ObjectPoolSpawnHook;

            //On.HeroAudioController.PlaySound += HeroAudioController_PlaySound;
            On.NailSlash.StartSlash += NailSlash_StartSlash;

            On.MenuAudioController.PlayStartGame += MenuAudioController_PlayStartGame;
            On.MenuAudioController.PlaySelect += MenuAudioController_PlaySelect;
            On.MenuAudioController.PlaySubmit += MenuAudioController_PlaySubmit;
            On.MenuAudioController.PlayCancel += MenuAudioController_PlayCancel;

            On.SoftLandEffect.OnEnable += SoftLandEffect_OnEnable;
            

            On.HeroController.Awake += HeroController_Awake;
            On.HeroController.Start += HeroController_Start;
            On.HeroController.checkEnvironment += HeroController_checkEnvironment;
            On.HeroBox.Start += HeroBox_Start;
            On.GameManager.SetupSceneRefs += GameManager_SetupSceneRefs;
            On.CheckpointSprite.Show += CheckpointSprite_Show;
            On.AtmosCue.IsChannelEnabled += AtmosCue_IsChannelEnabled;
            On.RestBench.Start += RestBench_Start;
            
            On.GeoControl.Start += GeoControl_Start;
            

            //On.InfectedEnemyEffects.RecieveHitEffect += InfectedEnemyEffects_RecieveHitEffect;
            

            //On.HutongGames.PlayMaker.Actions.AudioPlay.OnEnter += AudioPlay_OnEnter;
            //On.HutongGames.PlayMaker.Actions.AudioPlayerOneShotSingle.OnEnter += AudioPlayerOneShotSingle_OnEnter;
            On.HutongGames.PlayMaker.Actions.AudioPlayerOneShot.OnEnter += AudioPlayerOneShot_OnEnter;
            On.HutongGames.PlayMaker.Actions.AudioPlayRandom.OnEnter += AudioPlayRandom_OnEnter;



        }

        private void AudioPlayRandom_OnEnter(On.HutongGames.PlayMaker.Actions.AudioPlayRandom.orig_OnEnter orig, AudioPlayRandom self) {
            if(self.Owner.name == "Geo Rock 3(Clone)(Clone)") {
                Modding.Logger.Log("AABBB");
                self.audioClips = new AudioClip[] { audioClips["Georock_Hit"] };
                self.weights = new HutongGames.PlayMaker.FsmFloat[] { 1f };
            }
            Modding.Logger.Log(self.Owner.name);
            orig(self);
        }

        private void AudioPlayerOneShot_OnEnter(On.HutongGames.PlayMaker.Actions.AudioPlayerOneShot.orig_OnEnter orig, AudioPlayerOneShot self) {
            Modding.Logger.Log("FSM Audio Oneshot: " + self.audioClips[0].name + "\nOwner name: " + self.Owner.name);
            if(self.Owner.name == "Knight Spike Death(Clone)") {
                self.audioClips = new AudioClip[] { audioClips["player_take_damage"] };
                self.weights = new HutongGames.PlayMaker.FsmFloat[] { 1f };
                self.volume.Value *= 2.0f;
            }
            else if(self.Owner.name == "Buzzer(Clone)(Clone)") {
                self.audioClips = new AudioClip[] { audioClips["spitter_startle3"] };
                self.weights = new HutongGames.PlayMaker.FsmFloat[] { 1f };
                self.volume.Value = self.volume.Value * 0.5f;
            }
            orig(self);
            return;
        }

        private void HeroController_Start(On.HeroController.orig_Start orig, HeroController self) {
            PlayMakerFSM spikeFSM = self.spikeDeathPrefab.LocateMyFSM("Knight Death Control");
            var spikeAction = spikeFSM.GetAction<AudioPlayerOneShot>("Stab", 4);
            spikeAction.audioClips = new AudioClip[] { audioClips["running_in_cave_1"] };
            spikeAction.weights = new HutongGames.PlayMaker.FsmFloat[] { 1f };

            //spikeFSM.InsertAction("Stab", spikeAction, 4);

            //spikeAction.volume = new HutongGames.PlayMaker.FsmFloat(0f);
            //self.spikeDeathPrefab.DestroyAll();
            orig(self);
        }

        private void SoftLandEffect_OnEnable(On.SoftLandEffect.orig_OnEnable orig, SoftLandEffect self) {
            self.softLandClip = audioClips["soft_landing"];
            orig(self);
        }

        private void HeroController_checkEnvironment(On.HeroController.orig_checkEnvironment orig, HeroController self) {
            return;
        }

        public Dictionary<string, GameObject> ModifyPreloads(Dictionary<string, GameObject> preloadedObjects) {
           // preloadedObjects["georock"].GetComponent<AudioSource>().clip = audioClips["Georock_Hit"];


            EnemyDeathEffects deathEffect;
            InfectedEnemyEffects infectedEffect;
            GameObject corpsePrefab;
            Corpse corpse;
            AudioEvent damage;
            AudioEvent death;
            AudioEvent corpseStart;
            RandomAudioClipTable corpseSplat;
            PlayMakerFSM enemyFSM;



            GameObject georock = preloadedObjects["georock"];
            PlayMakerFSM georockFSM = georock.LocateMyFSM("Geo Rock");
            var rockHitAction = georockFSM.GetAction<AudioPlayRandom>("Check Direction",0);
            rockHitAction.audioClips = new AudioClip[] { audioClips["georock_hit-002"] };



            GameObject buzzer = preloadedObjects[Enemy.getName(EnemyType.buzzer)];


            buzzer.GetComponent<AudioSource>().clip = audioClips["buzzer_fly"];
            buzzer.GetComponent<AudioSource>().volume *= 0.4f;

            enemyFSM = buzzer.LocateMyFSM("chaser");

            var startleAction = enemyFSM.GetAction<AudioPlayerOneShot>("Startle", 0);
            startleAction.audioClips = new AudioClip[] { audioClips["spitter_startle3"] };
            startleAction.weights = new HutongGames.PlayMaker.FsmFloat[] { new HutongGames.PlayMaker.FsmFloat(1f) };
            enemyFSM.InsertAction("Startle", startleAction, 0);


            deathEffect = buzzer.GetComponent<EnemyDeathEffects>();

            death = deathEffect.GetAttr<EnemyDeathEffects, AudioEvent>("enemyDeathSwordAudio");
            death.Clip = audioClips["enemy_damage"];
            buzzer.GetComponent<EnemyDeathEffects>().SetAttr("enemyDeathSwordAudio",death);

            damage = deathEffect.GetAttr<EnemyDeathEffects, AudioEvent>("enemyDamageAudio");
            damage.Clip = audioClips["enemy_damage"];
            buzzer.GetComponent<EnemyDeathEffects>().SetAttr("enemyDamageAudio", damage);
            

            infectedEffect = buzzer.GetComponent<InfectedEnemyEffects>();

            damage = infectedEffect.GetAttr<InfectedEnemyEffects, AudioEvent>("impactAudio");
            damage.Clip = audioClips["enemy_damage"];
            buzzer.GetComponent<InfectedEnemyEffects>().SetAttr("impactAudio", damage);

            
            corpsePrefab = deathEffect.GetAttr<EnemyDeathEffects, GameObject>("corpsePrefab");
            corpse = corpsePrefab.GetComponent<Corpse>();

            
            corpseStart = corpse.GetAttr<Corpse, AudioEvent>("startAudio");
            corpseStart.Clip = audioClips["light_bonk"];
            corpse.SetAttr("startAudio", corpseStart);
            
            corpseSplat = corpse.GetAttr<Corpse, RandomAudioClipTable>("splatAudioClipTable");

            Type t = typeof(RandomAudioClipTable);
            FieldInfo optionsField = t.GetField("options", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var fieldValue = optionsField.GetValue(corpseSplat);
            var elementType = fieldValue.GetType().GetElementType();

            foreach (FieldInfo fi in elementType.GetFields()) Modding.Logger.Log(fi);


            FieldInfo clipField = elementType.GetField("Clip", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            FieldInfo weightField = elementType.GetField("Weight", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            var newValue = Activator.CreateInstance(elementType);
            clipField.SetValue(newValue, audioClips["corpse_splat"]);
            weightField.SetValue(newValue, 1f);


            var newArray = Array.CreateInstance(elementType, 1);
            newArray.SetValue(newValue, 0);


            optionsField.SetValue(corpseSplat, newArray);
            deathEffect.SetAttr("corpse",corpsePrefab);




            GameObject crawler = preloadedObjects[Enemy.getName(EnemyType.crawler)];
                
            crawler.GetComponent<AudioSource>().clip = audioClips["crawler_crawl (2)"];
            crawler.GetComponent<AudioSource>().volume *= 0.2f;


            deathEffect = preloadedObjects[Enemy.getName(EnemyType.crawler)].GetComponent<EnemyDeathEffects>();

            death = deathEffect.GetAttr<EnemyDeathEffects, AudioEvent>("enemyDeathSwordAudio");
            death.Volume = 0f;
            preloadedObjects[Enemy.getName(EnemyType.crawler)].GetComponent<EnemyDeathEffects>().SetAttr("enemyDeathSwordAudio", death);

            damage = deathEffect.GetAttr<EnemyDeathEffects, AudioEvent>("enemyDamageAudio");
            damage.Clip = audioClips["enemy_damage"];
            preloadedObjects[Enemy.getName(EnemyType.crawler)].GetComponent<EnemyDeathEffects>().SetAttr("enemyDamageAudio", damage);


            infectedEffect = preloadedObjects[Enemy.getName(EnemyType.crawler)].GetComponent<InfectedEnemyEffects>();

            damage = infectedEffect.GetAttr<InfectedEnemyEffects, AudioEvent>("impactAudio");
            damage.Volume = 0f;
            preloadedObjects[Enemy.getName(EnemyType.crawler)].GetComponent<InfectedEnemyEffects>().SetAttr("impactAudio", damage);
            


            return preloadedObjects;
        }

        private void HeroBox_Start(On.HeroBox.orig_Start orig, HeroBox self) {
            PlayMakerFSM geoFSM = self.gameObject.LocateMyFSM("Geo Get");

            var geoGetAction = geoFSM.GetAction<AudioPlayRandom>("Get 1", 2);
            geoGetAction.audioClips = new AudioClip[0];
            orig(self);
        }

        private void GeoControl_Start(On.GeoControl.orig_Start orig, GeoControl self) {
            Modding.Logger.Log("geo spawned");
            orig(self);
            AudioClip[] a = new AudioClip[] { audioClips["geo_collect"] };
            self.SetAttr("pickupSounds", a);

            self.gameObject.GetComponent<ObjectBounce>().clips = new AudioClip[0];
        }

        private void RestBench_Start(On.RestBench.orig_Start orig, RestBench self) {
            PlayMakerFSM benchFSM = self.gameObject.LocateMyFSM("Bench Control");

            var restAction = benchFSM.GetAction<AudioPlayerOneShotSingle>("Start Rest", 3);
            restAction.audioClip = audioClips["fast_woosh"];
        }

        private bool AtmosCue_IsChannelEnabled(On.AtmosCue.orig_IsChannelEnabled orig, AtmosCue self, AtmosChannels channel) {
            return false;
        }

        private void CheckpointSprite_Show(On.CheckpointSprite.orig_Show orig, CheckpointSprite self) {
            AudioSource a = self.gameObject.GetComponent<AudioSource>();
            a.volume = 0f;
            orig(self);
        }

        private void GameManager_SetupSceneRefs(On.GameManager.orig_SetupSceneRefs orig, GameManager self, bool refreshTilemapInfo) {
            orig(self, refreshTilemapInfo);

            PlayMakerFSM fsm = self.soulOrb_fsm;
            if (fsm == null) return;
            var healReadyAction = fsm.GetAction<AudioPlayerOneShotSingle>("Can Heal 2", 4);
            healReadyAction.volume = 0;
        }

        
        private void AudioPlayerOneShotSingle_OnEnter(On.HutongGames.PlayMaker.Actions.AudioPlayerOneShotSingle.orig_OnEnter orig, AudioPlayerOneShotSingle self) {
            self.volume = 0f;
            orig(self);
        }

        private void AudioPlay_OnEnter(On.HutongGames.PlayMaker.Actions.AudioPlay.orig_OnEnter orig, AudioPlay self) {
            self.volume = 0f;
            orig(self);
        }

        public void HeroController_Awake(On.HeroController.orig_Awake orig, HeroController self) {
            orig(self);
            
            GameObject nailTerrainPrefab = self.nailTerrainImpactEffectPrefab;
            AudioSource a = nailTerrainPrefab.GetComponent<AudioSource>();
            a.clip = audioClips["nail_reject3"];
            nailTerrainPrefab.DestroyAll();

            GameObject softLandingPrefab = self.softLandingEffectPrefab;
            a = softLandingPrefab.GetComponent<AudioSource>();
            a.clip = audioClips["soft_landing"];
            self.softLandingEffectPrefab = softLandingPrefab;
            softLandingPrefab.DestroyAll();

            PlayMakerFSM spellFsm = self.gameObject.LocateMyFSM("Spell Control");
            var focusActionD = spellFsm.GetAction<AudioPlay>("Focus Start D", 2);
            focusActionD.volume = 0;
            var focusAction = spellFsm.GetAction<AudioPlay>("Focus Start", 6);
            focusAction.oneShotClip = audioClips["Focus Start"];
            var focusHealAction = spellFsm.GetAction<AudioPlayerOneShotSingle>("Focus Heal", 3);
            focusHealAction.audioClip = audioClips["Focus Heal 2.0"];
            

            HeroAudioController heroAudioController = self.GetAttr<HeroController,HeroAudioController>("audioCtrl");
            heroAudioController.footStepsRun.clip = audioClips["running_in_cave"];
            heroAudioController.footStepsRun.volume *= 0.7f;
            heroAudioController.hardLanding.clip = audioClips["hard_landing"];
            heroAudioController.hardLanding.volume *= 1.1f;
            heroAudioController.jump.clip = audioClips["woosh"];
            heroAudioController.jump.volume *= 0.8f;
            heroAudioController.takeHit.clip = audioClips["player_take_damage"];
            heroAudioController.takeHit.volume *= 1.1f;
            heroAudioController.softLanding.clip = audioClips["soft_landing"];
            heroAudioController.falling.clip = null;



        }

        public void ApplyMusicClip(AudioClip clip) { //prefer using music regions over this method

            MusicCue musicCue = ScriptableObject.CreateInstance<MusicCue>();
            List<MusicCue.MusicChannelInfo> channelInfos = new List<MusicCue.MusicChannelInfo>();
            MusicCue.MusicChannelInfo channelInfo = new MusicCue.MusicChannelInfo();
            channelInfo.SetAttr("clip", clip);
            channelInfos.Add(channelInfo);
            musicCue.SetAttr("channelInfos", channelInfos.ToArray());
            GameManager.instance.AudioManager.ApplyMusicCue(musicCue, 0f, 0f, false);

        }



        private void MenuAudioController_PlayCancel(On.MenuAudioController.orig_PlayCancel orig, MenuAudioController self) {
            self.cancel = audioClips["menu_sound"];
            orig(self);
        }

        private void MenuAudioController_PlaySubmit(On.MenuAudioController.orig_PlaySubmit orig, MenuAudioController self) {
            self.submit = audioClips["menu_sound"];
            orig(self);
        }

        private void MenuAudioController_PlaySelect(On.MenuAudioController.orig_PlaySelect orig, MenuAudioController self) {
            self.select = audioClips["menu_sound"];
            orig(self);
        }

        private void MenuAudioController_PlayStartGame(On.MenuAudioController.orig_PlayStartGame orig, MenuAudioController self) {
            self.startGame = audioClips["menu_sound"];
            orig(self);
        }


        
        private GameObject ObjectPoolSpawnHook(GameObject go) {
            //Log(go.name);
            if (go.name == "Knight Spike Death(Clone)") {
                AudioSource a = go.GetComponent<AudioSource>();
                Debug.Log("TestCallStack: " + Environment.StackTrace);
                //a.clip = audioClips["soft_landing"];

                return go;
            }
            /*else if (go.name == "Nail Terrain Hit Effect(Clone)") {
                AudioSource a = go.GetComponent<AudioSource>();
                a.clip = audioClips["nail_reject3"];
                return go;
            }*/
            else return go;
        }



        private void NailSlash_StartSlash(On.NailSlash.orig_StartSlash orig, NailSlash self) {
            self.GetAttr<NailSlash, AudioSource>("audio").clip = audioClips["nail_slash-002"];
            orig(self);
        }


        private void HeroAudioController_PlaySound(On.HeroAudioController.orig_PlaySound orig, HeroAudioController self, HeroSounds soundEffect) {
            return;
        }


    }
}