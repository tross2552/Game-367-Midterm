using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace HK_UnityHelper
{

    public enum EnemyType {
        none,
        primalAspid,
        crawler,
        buzzer
    }

    public class Enemy : MonoBehaviour {

        [SerializeField]
        private EnemyType enemy = EnemyType.none;

        private static readonly string[] enemyNames = { "none", "primalaspid", "crawler", "buzzer" };
        private static readonly string[] loadNames = { "none", "Super Spitter", "_Enemies/Crawler 1", "_Enemies/Buzzer" };
        private static readonly string[] loadSceneNames = { "none", "Deepnest_East_11", "Tutorial_01", "Tutorial_01" };

        public string EnemyName {
            get { return getName(enemy); }
        }


        public static string getName(EnemyType e) { return enemyNames[(int)e]; }
        public static string getLoadName(EnemyType e) { return loadNames[(int)e]; }
        public static string getLoadSceneName(EnemyType e) { return loadSceneNames[(int)e]; }

    }
}
