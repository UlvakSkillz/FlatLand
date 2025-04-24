using Il2CppRUMBLE.Interactions.InteractionBase;
using Il2CppRUMBLE.Managers;
using Il2CppRUMBLE.MoveSystem;
using Il2CppRUMBLE.Players.Subsystems;
using Il2CppTMPro;
using MelonLoader;
using RumbleModdingAPI;
using RumbleModUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FlatLand
{
    public class main : MelonMod
    {
        private string currentScene = "Loader";
        private string lastScene = "";
        private bool gymInit = false;
        private bool flatLandActive = false;
        private bool respawning = false;
        private GameObject buttonToSwaptoFlatLand, flatLandTextPanel, flatLandParent;
        private List<string> dontDisableGameObject = new List<string>();
        private List<GameObject> DisabledDDOLGameObjects = new List<GameObject>();
        private GameObject landParent, plane, measureParent, flatLandParent2, matchmaker, regionBoard;
        UI UI = UI.instance;
        public static Mod FlatLand = new Mod();
        private int size;
        private bool loadMatchmaker, showMeasurements, darkMode, autoLoad;

        public override void OnLateInitializeMelon()
        {
            FlatLand.ModName = "FlatLand";
            FlatLand.ModVersion = "1.6.3";
            FlatLand.SetFolder("FlatLand");
            FlatLand.AddToList("Map Size", 125, "Determins the size of the FlatLand", new Tags { });
            FlatLand.AddToList("Have Matchmaker", false, 0, "Loads a Matchmaker into FlatLand", new Tags { });
            FlatLand.AddToList("Show Measurements", true, 0, "Shows Numbers in FlatLand for Easy Measurements", new Tags { });
            FlatLand.AddToList("DarkMode", false, 0, "Toggles Walls for Dark Mode", new Tags { });
            FlatLand.AddToList("Auto Load FlatLand", false, 0, "Automatically Loads into FlatLand instead of the Gym", new Tags { });
            FlatLand.GetFromFile();
            FlatLand.ModSaved += Save;
            size = (int)FlatLand.Settings[0].SavedValue;
            if (size < 1)
            {
                size = 1;
                FlatLand.Settings[0].SavedValue = 1;
                FlatLand.Settings[0].Value = 1;
            }
            loadMatchmaker = (bool)FlatLand.Settings[1].SavedValue;
            showMeasurements = (bool)FlatLand.Settings[2].SavedValue;
            darkMode = (bool)FlatLand.Settings[3].SavedValue;
            autoLoad = (bool)FlatLand.Settings[4].SavedValue;
            UI.instance.UI_Initialized += UIInit;
            Calls.onMapInitialized += SceneInit;
            dontDisableGameObject.Add("LanguageManager");
            dontDisableGameObject.Add("PhotonMono");
            dontDisableGameObject.Add("Game Instance");
            dontDisableGameObject.Add("Timer Updater");
            dontDisableGameObject.Add("PlayFabHttp");
            dontDisableGameObject.Add("LIV");
            dontDisableGameObject.Add("UniverseLibCanvas");
            dontDisableGameObject.Add("UE_Freecam");
            dontDisableGameObject.Add("--------------SCENE--------------");
            dontDisableGameObject.Add("--------------LOGIC--------------");
            dontDisableGameObject.Add("!ftraceLightmaps");
            dontDisableGameObject.Add("VoiceLogger");
            dontDisableGameObject.Add("Player Controller(Clone)");
            dontDisableGameObject.Add("Health");
        }

        private void Save()
        {
            size = (int)FlatLand.Settings[0].SavedValue;
            if (size < 1)
            {
                size = 1;
                FlatLand.Settings[0].SavedValue = 1;
                FlatLand.Settings[0].Value = 1;
            }
            bool oldLoadMatchmaker = loadMatchmaker;
            loadMatchmaker = (bool)FlatLand.Settings[1].SavedValue;
            showMeasurements = (bool)FlatLand.Settings[2].SavedValue;
            darkMode = (bool)FlatLand.Settings[3].SavedValue;
            autoLoad = (bool)FlatLand.Settings[4].SavedValue;
            if (flatLandActive)
            {
                if (oldLoadMatchmaker != loadMatchmaker)
                {
                    matchmaker.SetActive(loadMatchmaker);
                    regionBoard.SetActive(loadMatchmaker);
                }
                MelonCoroutines.Start(ReLoadFlatLand());
            }
        }

        private void UIInit()
        {
            UI.AddMod(FlatLand);
        }

        public override void OnFixedUpdate()
        {
            if (flatLandActive)
            {
                float playerY = PlayerManager.instance.localPlayer.Controller.gameObject.transform.GetChild(5).GetChild(5).GetChild(0).position.y;
                if (!respawning && (playerY <= -10))
                {
                    respawning = true;
                    PlayerManager.instance.localPlayer.Controller.gameObject.GetComponent<PlayerResetSystem>().ResetPlayerController();
                }
                else if (respawning && (playerY >= -10))
                {
                    respawning = false;
                }
            }
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            lastScene = currentScene;
            currentScene = sceneName;
            if (flatLandActive)
            {
                ReactivateDDOLObjects();
                flatLandActive = false;
            }
            gymInit = false;
        }

        private void SceneInit()
        {
            if ((currentScene == "Gym") && !gymInit)
            {
                try
                {
                    GameObject matchmakerBackPanel = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    matchmakerBackPanel.AddComponent<MeshCollider>();
                    matchmakerBackPanel.name = "BackPanel";
                    matchmakerBackPanel.GetComponent<Renderer>().material.shader = Shader.Find("Universal Render Pipeline/Lit");
                    matchmakerBackPanel.GetComponent<Renderer>().material.color = new Color(0, 1, 0.814f);
                    matchmakerBackPanel.transform.parent = Calls.GameObjects.Gym.Logic.HeinhouserProducts.MatchConsole.GetGameObject().transform;
                    matchmakerBackPanel.transform.localRotation = Quaternion.Euler(0, 0, 0);
                    matchmakerBackPanel.transform.localPosition = new Vector3(-0.4915f, 1.2083f, -0.0151f);
                    matchmakerBackPanel.transform.localScale = new Vector3(3f, 2.41f, 0.02f);
                    flatLandParent = new GameObject();
                    flatLandParent.name = "FlatLand";
                    flatLandTextPanel = GameObject.Instantiate(Calls.GameObjects.Gym.Logic.HeinhouserProducts.MatchConsole.RankRelaxControls.GetGameObject().transform.GetChild(17).gameObject);
                    flatLandTextPanel.name = "FlatLand Plate";
                    flatLandTextPanel.transform.parent = flatLandParent.transform;
                    flatLandTextPanel.transform.position = new Vector3(7.45f, 1.9f, 10.12f);
                    flatLandTextPanel.transform.rotation = Quaternion.Euler(90f, 122.8f, 0f);
                    flatLandTextPanel.transform.localScale = new Vector3(0.29f, 0.3036f, 0.362f);
                    GameObject textPanelTextGO = GameObject.Instantiate(Calls.GameObjects.Gym.Logic.HeinhouserProducts.MatchConsole.RankRelaxControls.GetGameObject().transform.GetChild(15).GetChild(6).gameObject);
                    textPanelTextGO.transform.parent = flatLandTextPanel.transform;
                    textPanelTextGO.name = "Text";
                    textPanelTextGO.transform.localPosition = new Vector3(0.04f, 0.74f, 0f);
                    textPanelTextGO.transform.localRotation = Quaternion.Euler(0f, 270f, 90f);
                    textPanelTextGO.transform.localScale = new Vector3(6.0414f, 3.7636f, 6.462f);
                    TextMeshPro flatLandTextPanelTMP = textPanelTextGO.GetComponent<TextMeshPro>();
                    flatLandTextPanelTMP.text = "FlatLand";
                    buttonToSwaptoFlatLand = Calls.Create.NewButton();
                    buttonToSwaptoFlatLand.name = "FlatLandButton";
                    buttonToSwaptoFlatLand.transform.parent = flatLandParent.transform;
                    buttonToSwaptoFlatLand.transform.position = new Vector3(7.67f, 1.7f, 10f);
                    buttonToSwaptoFlatLand.transform.localRotation = Quaternion.Euler(0f, 302.5f, 90f);
                    buttonToSwaptoFlatLand.transform.GetChild(0).gameObject.GetComponent<InteractionButton>().onPressed.AddListener(new System.Action(() =>
                    {
                        MelonCoroutines.Start(ToFlatLandPressed());
                    }));
                    gymInit = true;
                    if (autoLoad && lastScene != "Gym")
                    {
                        MelonCoroutines.Start(AutoLoad(currentScene));
                    }
                }
                catch {  }
            }
        }

        private IEnumerator AutoLoad(string scene)
        {
            yield return new WaitForSeconds(5f);
            if (scene == currentScene)
            {
                buttonToSwaptoFlatLand.transform.GetChild(0).gameObject.GetComponent<InteractionButton>().RPC_OnPressed();
            }
            yield break;
        }

        private IEnumerator ToFlatLandPressed()
        {
            try
            {
                matchmaker = Calls.GameObjects.Gym.Logic.HeinhouserProducts.MatchConsole.GetGameObject();
                regionBoard = Calls.GameObjects.Gym.Logic.HeinhouserProducts.RegionSelector.GetGameObject();
                PlayerManager.instance.localPlayer.Controller.gameObject.GetComponent<PlayerResetSystem>().ResetPlayerController();
            }
            catch { }
            yield return new WaitForSeconds(1);
            flatLandActive = true;
            DeloadGym();
            LoadFlatLand();
            yield break;
        }

        private IEnumerator ReLoadFlatLand()
        {
            PlayerManager.instance.localPlayer.Controller.gameObject.GetComponent<PlayerResetSystem>().ResetPlayerController();
            yield return new WaitForSeconds(1);
            GameObject.DestroyImmediate(landParent);
            GameObject.DestroyImmediate(measureParent);
            GameObject.DestroyImmediate(flatLandParent2);
            LoadFlatLand();
            yield break;
        }

        private IEnumerator TurnOffAllExtraRootObjects()
        {
            GameObject temp1 = new GameObject();
            GameObject.DontDestroyOnLoad(temp1);
            Scene ddolScene = temp1.scene;
            GameObject.DestroyImmediate(temp1);
            GameObject[] ddolGameObjects = ddolScene.GetRootGameObjects();
            GameObject[] allGameObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (GameObject temp in ddolGameObjects)
            {
                if (temp.active && !dontDisableGameObject.Contains(temp.name))
                {
                    temp.SetActive(false);
                    DisabledDDOLGameObjects.Add(temp);
                }
            }
            foreach (GameObject temp in allGameObjects)
            {
                if (temp.active && !dontDisableGameObject.Contains(temp.name))
                {
                    temp.SetActive(false);
                }
                else if (temp.name == "--------------LOGIC--------------")
                {
                    for (int i = 0; i < temp.transform.GetChildCount(); i++)
                    {
                        if (temp.transform.GetChild(i).gameObject.name == "Heinhouser products")
                        {
                            for (int j = 0; j < temp.transform.GetChild(i).GetChildCount(); j++)
                            {
                                if ((temp.transform.GetChild(i).GetChild(j).gameObject.name != "MatchConsole") && (temp.transform.GetChild(i).GetChild(j).gameObject.name != "RegionSelector"))
                                {
                                    temp.transform.GetChild(i).GetChild(j).gameObject.SetActive(false);
                                }
                                else if (!loadMatchmaker)
                                {
                                    temp.transform.GetChild(i).GetChild(j).gameObject.SetActive(false);
                                }
                            }
                        }
                        else if (temp.transform.GetChild(i).gameObject.name != "Handelers")
                        {
                            temp.transform.GetChild(i).gameObject.SetActive(false);
                        }
                    }
                }
                else if (temp.name == "--------------SCENE--------------")
                {
                    temp.transform.GetChild(4).GetChild(1).position = new Vector3(6.84f, 4.3841f, 1.0968f);
                    temp.transform.GetChild(4).GetChild(1).localRotation = Quaternion.Euler(58.2924f, 189.6045f, 292.3752f);
                    for (int i = 0;i < temp.transform.GetChildCount(); i++)
                    {
                        if (i == 4)
                        {
                            for (int j = 0; j < temp.transform.GetChild(4).childCount; j++)
                            {
                                if ((j == 1) || (j == 3)) { continue; }
                                temp.transform.GetChild(i).GetChild(j).gameObject.SetActive(false);
                            }
                            continue;
                        }
                        temp.transform.GetChild(i).gameObject.SetActive(false);
                    }
                }
            }
            yield break;
        }

        private void DeloadGym()
        {
                ResetStructures();
            try
            {
                GameObject.Destroy(Calls.GameObjects.Gym.Logic.Bounds.SceneBoundaryStructures.GetGameObject());
            }
            catch { }
            try
            {
                GameObject.Destroy(Calls.GameObjects.Gym.Logic.Bounds.SceneBoundaryPlayer.GetGameObject());
            }
            catch { }
            MelonCoroutines.Start(TurnOffAllExtraRootObjects());
        }

        private void ResetStructures()
        {
            PoolManager.instance.GetPool("Disc").Reset(true);
            PoolManager.instance.GetPool("Ball").Reset(true);
            PoolManager.instance.GetPool("Pillar").Reset(true);
            PoolManager.instance.GetPool("RockCube").Reset(true);
            PoolManager.instance.GetPool("Wall").Reset(true);
            PoolManager.instance.GetPool("BoulderBall").Reset(true);
            PoolManager.instance.GetPool("SmallRock").Reset(true);
            PoolManager.instance.GetPool("LargeRock").Reset(true);
        }

        private void ReLoadGym()
        {
            if (matchmaker.active)
            {
                InteractionLever flatlandMatchmakingLever = matchmaker.transform.GetChild(7).GetChild(2).GetChild(0).GetComponent<InteractionLever>();
                if (flatlandMatchmakingLever.snappedStep == 1)
                {
                    flatlandMatchmakingLever.SetStep(0, false, false);
                }
            }
            Il2CppRUMBLE.Managers.SceneManager.instance.LoadSceneAsync(1, false, false, 2, LoadSceneMode.Single);
        }

        private void ReactivateDDOLObjects()
        {
            foreach (GameObject temp in DisabledDDOLGameObjects)
            {
                temp.SetActive(true);
            }
            DisabledDDOLGameObjects.Clear();
        }

        private void LoadFlatLand()
        {
            try
            {
                landParent = new GameObject();
                landParent.name = "LandParent";
                plane = GameObject.CreatePrimitive(PrimitiveType.Cube);
                plane.name = "Floor";
                plane.transform.parent = landParent.transform;
                plane.transform.localScale = new Vector3(size, 0.01f, size);
                plane.transform.position = new Vector3(2.8007f, 0, -1.9802f);
                plane.layer = 9;
                Component.Destroy(plane.GetComponent<BoxCollider>());
                plane.GetComponent<Renderer>().material.shader = Shader.Find("Universal Render Pipeline/Lit");
                plane.GetComponent<Renderer>().material.color = new Color(0.5f, 0.5f, 0.5f);
                if (darkMode)
                {
                    plane.GetComponent<Renderer>().material.color = new Color(0.1f, 0.1f, 0.1f);
                    GameObject plane2 = GameObject.Instantiate(plane);
                    plane2.transform.parent = landParent.transform;
                    plane2.transform.position = new Vector3(size / 2 + 2.8007f, size / 2, -1.9802f);
                    plane2.transform.rotation = Quaternion.Euler(0, 0, 90);
                    GameObject plane3 = GameObject.Instantiate(plane2);
                    plane3.transform.parent = landParent.transform;
                    plane3.transform.position = new Vector3(-size / 2 + 2.8007f, size / 2, -1.9802f);
                    plane3.transform.rotation = Quaternion.Euler(0, 0, 90);
                    GameObject plane4 = GameObject.Instantiate(plane2);
                    plane4.transform.parent = landParent.transform;
                    plane4.transform.position = new Vector3(2.8007f, size / 2, size / 2 - 1.9802f);
                    plane4.transform.rotation = Quaternion.Euler(90, 0, 0);
                    GameObject plane5 = GameObject.Instantiate(plane2);
                    plane5.transform.parent = landParent.transform;
                    plane5.transform.position = new Vector3(2.8007f, size / 2, -size / 2 - 1.9802f);
                    plane5.transform.rotation = Quaternion.Euler(90, 0, 0);
                    GameObject plane6 = GameObject.Instantiate(plane2);
                    plane6.transform.parent = landParent.transform;
                    plane6.transform.position = new Vector3(2.8007f, size - 1f, -1.9802f);
                    plane6.transform.rotation = Quaternion.Euler(0, 0, 0);
                }
                MeshCollider meshCollider = plane.AddComponent<MeshCollider>();
                GroundCollider groundCollider = plane.AddComponent<GroundCollider>();
                groundCollider.isMainGroundCollider = true;
                groundCollider.collider = meshCollider;
                measureParent = new GameObject();
                measureParent.name = "Measurements";
                measureParent.transform.position = new Vector3(0, 0, 0);
                for (int x = 0; x <= 1; x++)
                {
                    for (int i = -size / 2; i <= size / 2; i++)
                    {
                        GameObject measure = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        Component.Destroy(measure.GetComponent<BoxCollider>());
                        measure.GetComponent<Renderer>().material.shader = Shader.Find("Universal Render Pipeline/Lit");
                        measure.GetComponent<Renderer>().material.color = new Color(0, 0, 0);
                        measure.transform.parent = measureParent.transform;
                        measure.transform.localScale = new Vector3(0.01f, 0.0101f, size);
                        if (x == 0)
                        {
                            measure.transform.position = new Vector3(i + 2.8007f, 0, -1.9802f);
                            if (showMeasurements && (i % 5 == 0) && (i != 0))
                            {
                                GameObject iText = Calls.Create.NewText();
                                iText.GetComponent<TextMeshPro>().text = Mathf.Abs(i).ToString();
                                iText.GetComponent<TextMeshPro>().fontSize = 10;
                                iText.transform.parent = measureParent.transform;
                                iText.transform.position = new Vector3(i + 2.8007f, 0.5f, -1.9802f);
                                if (i < 0)
                                {
                                    iText.transform.rotation = Quaternion.Euler(0, -90, 0);
                                }
                                else
                                {
                                    iText.transform.rotation = Quaternion.Euler(0, 90, 0);
                                }
                            }
                        }
                        else
                        {
                            measure.transform.position = new Vector3(2.8007f, 0, i - 1.9802f);
                            measure.transform.localRotation = Quaternion.Euler(0, 90, 0);
                            if (showMeasurements && (i % 5 == 0) && (i != 0))
                            {
                                GameObject iText = Calls.Create.NewText();
                                iText.GetComponent<TextMeshPro>().text = Mathf.Abs(i).ToString();
                                iText.GetComponent<TextMeshPro>().fontSize = 10;
                                iText.transform.parent = measureParent.transform;
                                iText.transform.position = new Vector3(2.8007f, 0.5f, i - 1.9802f);
                                if (i < 0)
                                {
                                    iText.transform.rotation = Quaternion.Euler(0, 180, 0);
                                }
                            }
                        }
                    }
                }
                flatLandParent2 = new GameObject();
                flatLandParent2.name = "FlatLandButtonParent";
                flatLandParent2.transform.position = new Vector3(-3.8f, 0f, -11.2473f);
                GameObject returnToGymPlate = GameObject.Instantiate(Calls.GameObjects.Gym.Logic.HeinhouserProducts.MatchConsole.RankRelaxControls.GetGameObject().transform.GetChild(17).gameObject);
                returnToGymPlate.name = "FlatLand Plate";
                returnToGymPlate.transform.parent = flatLandParent2.transform;
                returnToGymPlate.transform.localPosition = new Vector3(9.09f, 1.1882f, 5.6149f);
                returnToGymPlate.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                returnToGymPlate.transform.localScale = new Vector3(0.29f, 0.3036f, 0.362f);
                Component.Destroy(returnToGymPlate.GetComponent<BoxCollider>());
                GameObject textPanelTextGO = GameObject.Instantiate(Calls.GameObjects.Gym.Logic.HeinhouserProducts.MatchConsole.RankRelaxControls.GetGameObject().transform.GetChild(15).GetChild(6).gameObject);
                textPanelTextGO.transform.parent = returnToGymPlate.transform;
                textPanelTextGO.name = "Text";
                textPanelTextGO.transform.localPosition = new Vector3(0.04f, 0.74f, 0f);
                textPanelTextGO.transform.localRotation = Quaternion.Euler(0f, 270f, 90f);
                textPanelTextGO.transform.localScale = new Vector3(6.0414f, 3.7636f, 6.462f);
                TextMeshPro flatLandTextPanelTMP = textPanelTextGO.GetComponent<TextMeshPro>();
                flatLandTextPanelTMP.text = "To Gym";
                GameObject buttonToSwapFromFlatLand = Calls.Create.NewButton();
                buttonToSwapFromFlatLand.name = "FlatLand Button";
                buttonToSwapFromFlatLand.transform.parent = flatLandParent2.transform;
                buttonToSwapFromFlatLand.transform.localPosition = new Vector3(9.0845f, 1f, 5.8345f);
                buttonToSwapFromFlatLand.transform.localRotation = Quaternion.Euler(0f, 180f, 90f);
                buttonToSwapFromFlatLand.transform.GetChild(0).gameObject.GetComponent<InteractionButton>().onPressed.AddListener(new System.Action(() =>
                {
                    ReLoadGym();
                }));
                GameObject killStructuresPlate = GameObject.Instantiate(Calls.GameObjects.Gym.Logic.HeinhouserProducts.MatchConsole.RankRelaxControls.GetGameObject().transform.GetChild(17).gameObject);
                killStructuresPlate.name = "Kill Structures Plate";
                killStructuresPlate.transform.parent = flatLandParent2.transform;
                killStructuresPlate.transform.localPosition = new Vector3(9.09f, 1.7882f, 5.4749f);
                killStructuresPlate.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                killStructuresPlate.transform.localScale = new Vector3(0.29f, 0.5f, 0.362f);
                Component.Destroy(killStructuresPlate.GetComponent<BoxCollider>());
                GameObject killStructuresTextPanelTextGO = GameObject.Instantiate(Calls.GameObjects.Gym.Logic.HeinhouserProducts.MatchConsole.RankRelaxControls.GetGameObject().transform.GetChild(15).GetChild(6).gameObject);
                killStructuresTextPanelTextGO.transform.parent = killStructuresPlate.transform;
                killStructuresTextPanelTextGO.name = "Text";
                killStructuresTextPanelTextGO.transform.localPosition = new Vector3(0.04f, 0.74f, 0f);
                killStructuresTextPanelTextGO.transform.localRotation = Quaternion.Euler(0f, 270f, 90f);
                killStructuresTextPanelTextGO.transform.localScale = new Vector3(3.525f, 3.8836f, 6.462f);
                TextMeshPro killStructuresTextPanelTextTMP = killStructuresTextPanelTextGO.GetComponent<TextMeshPro>();
                killStructuresTextPanelTextTMP.text = "Reset Structures";
                GameObject buttonToKillStructures = Calls.Create.NewButton();
                buttonToKillStructures.name = "Kill Structures Button";
                buttonToKillStructures.transform.parent = flatLandParent2.transform;
                buttonToKillStructures.transform.localPosition = new Vector3(9.0845f, 1.6f, 5.8345f);
                buttonToKillStructures.transform.localRotation = Quaternion.Euler(0f, 180f, 90f);
                buttonToKillStructures.transform.GetChild(0).gameObject.GetComponent<InteractionButton>().onPressed.AddListener(new System.Action(() =>
                {
                    ResetStructures();
                }));
                GameObject shiftstoneSwapper = GameObject.Instantiate(Calls.GameObjects.Gym.Logic.HeinhouserProducts.ShiftstoneQuickswapper.GetGameObject());
                shiftstoneSwapper.SetActive(true);
                Component.Destroy(shiftstoneSwapper.transform.GetChild(0).GetChild(0).GetComponent<MeshCollider>());
                shiftstoneSwapper.transform.parent = flatLandParent2.transform;
                shiftstoneSwapper.transform.localPosition = new Vector3(8.4845f, 1f, 6.6345f);
                shiftstoneSwapper.transform.rotation = Quaternion.Euler(0, 355.9185f, 0);
                if (loadMatchmaker)
                {
                    matchmaker.transform.position = new Vector3(2.5214f, -0.001f, -5.9377f);
                    matchmaker.transform.rotation = Quaternion.Euler(0, 179.2461f, 0);
                    GameObject bell = matchmaker.transform.GetChild(9).gameObject;
                    bell.transform.localPosition = new Vector3(0.75f, 2.15f, -0.5f);
                    regionBoard.transform.position = new Vector3(0.676f, 0.958f, -4.45f);
                    regionBoard.transform.rotation = Quaternion.Euler(0, 90, 305);
                }
            }
            catch { }
        }
    }
}
