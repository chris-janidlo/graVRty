using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEditor;

namespace GraVRty.Editor
{
    public static class SceneReloader
    {
        [MenuItem("Tools/Reload Current Scene")]
        public static void ReloadCurrentScene ()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
