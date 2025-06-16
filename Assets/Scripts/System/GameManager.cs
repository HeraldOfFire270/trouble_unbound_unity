using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum HitDirection {
    None,
    Top,
    Bottom,
    Forward,
    Back,
    Left,
    Right
}

public enum UIDisplay
{
    Blank,
    DebugVelocity,
    DebugOnGround,
    DebugReserved,
    Timer,
    Score,
}

public enum BlockColor
{
    White = 1,
    Black = 2,
    Green = 3,
    Blue = 4,
    Red = 5,
    Yellow = 6,
    Orange = 7,
    Purple = 8,
    Pink = 9,

    // Everything from Ice onwards is not counted as a lockable block
    Ice = 15,
    Fire = 16,
    Steel = 17,
    Crate = 18,
}

public enum PhysicsLayer
{
    Default = 0x00,
    TransparentFX = 0x01,
    IgnoreRaycast = 0x02,
    BuiltInLayer3 = 0x04,
    Water = 0x08,
    UI = 0x10,
    BuiltInLayer6 = 0x20,
    BuiltInLayer7 = 0x40,

    Ground = 0x80,
    Pushables = 0x100,
    Detectors = 0x200,
    Player = 0x400,
    Hazard = 0x800,
    Bonus = 0x1000,
    TrapDoor = 0x2000,

    AllVisible = 0x13c0,
}

[Flags]
public enum LevelFlags
{
    None = 0x00,
    All = ~0,
    Flag1 = 0x01,
    Flag2 = 0x02,
    Flag3 = 0x04,
    Flag4 = 0x08,
    Flag5 = 0x10,
    Flag6 = 0x20,
    Flag7 = 0x40,
    Flag8 = 0x80,
    Flag9 = 0x100,
    Flag10 = 0x200,
    Flag11 = 0x400,
    Flag12 = 0x800,
    Flag14 = 0x1000,
    Flag15 = 0x2000,
    Flag16 = 0x4000,
    Flag17 = 0x8000,
    Flag18 = 0x10000,
    Flag19 = 0x20000,
    Flag20 = 0x40000,
    Flag21 = 0x80000,
    Flag22 = 0x100000,
    Flag23 = 0x200000,
    Flag24 = 0x400000,
    Flag25 = 0x800000,
    Flag26 = 0x1000000,
    Flag27 = 0x2000000,
    Flag28 = 0x4000000,
    Flag29 = 0x8000000,
    Flag30 = 0x10000000,
    Flag31 = 0x20000000,
    Flag32 = 0x40000000,
}

[Flags]
public enum GameFlags
{
    None = 0x00,
    All = ~0,
    Flag1 = 0x01,
    Flag2 = 0x02,
    Flag3 = 0x04,
    Flag4 = 0x08,
    Flag5 = 0x10,
    Flag6 = 0x20,
    Flag7 = 0x40,
    Flag8 = 0x80,
    Flag9 = 0x100,
    Flag10 = 0x200,
    Flag11 = 0x400,
    Flag12 = 0x800,
    Flag14 = 0x1000,
    Flag15 = 0x2000,
    Flag16 = 0x4000,
    Flag17 = 0x8000,
    Flag18 = 0x10000,
    Flag19 = 0x20000,
    Flag20 = 0x40000,
    Flag21 = 0x80000,
    Flag22 = 0x100000,
    Flag23 = 0x200000,
    Flag24 = 0x400000,
    Flag25 = 0x800000,
    Flag26 = 0x1000000,
    Flag27 = 0x2000000,
    Flag28 = 0x4000000,
    Flag29 = 0x8000000,
    Flag30 = 0x10000000,
    Flag31 = 0x20000000,
    Flag32 = 0x40000000,
}

public enum TrapDoorState
{
    // Static denotes not in use
    Static = 0,

    SourceOpen = 1,
    BlockEntry = 2,
    SourceClosed = 3,
    DestinationOpen = 4,
    BlockExit = 5,
    DestinationClosed = 6,
}

public enum MenuState
{
    NoActiveMenu,
    MainMenu,
    PauseMenu,
    OptionsMenu,
    ControlsMenu,
    LevelSelectMenu,
}

public class GameManager : MonoBehaviour {

    public static GameManager GM;

    public const float Gravity = 7.5f;
    public const float WorldBottom = -50f;
    public const float BlockFallLimit = -100f;
    public const float VolumeDefault = 0.5f;

    // System related variables

    public static bool DebugMode { get; set; }

    public static float MusicVolume { get; set; }
    public static float SoundVolume { get; set; }

    public static bool SoundOn { get; set; }
    public static bool MusicOn { get; set; }

    private static int _level;
    private const int _levelCount = 1;

    private static int cameraFacing = 0;
    private static int blockCount = 0;
    private static int lockedBlocks = 0;
    private static int attemptCount = 0;
    private static LevelFlags levelFlag = LevelFlags.None;
    private static GameFlags gameFlag = GameFlags.None;

    private static Vector3 playerPos;            // Player starting location on current map
    private static Quaternion playerRot;         // Player starting rotation on current map

    private static bool _failed;

    private static Camera mainCamera;

    void Awake()
    {
        if (!GM)
        {
            DontDestroyOnLoad(gameObject);
            GM = this;
            DebugMode = false;
            LoadConfig();
        }
        else
        {
            if (GM != this)
            {
                Destroy(gameObject);
            }
        }
    }

    public static void LoadConfig()
    {
        string saveFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + @"\Trouble\";
        string data;
        char _f = 'a';

        MusicOn = true;
        SoundOn = true;
        MusicVolume = VolumeDefault;
        SoundVolume = VolumeDefault;

        System.IO.StreamReader file = null;
        Debug.Log("Loading configuration file: " + saveFolder + "Trouble.cfg");

        try
        {
            file = new System.IO.StreamReader(saveFolder + "Trouble.cfg");
            if (file == null) Debug.Log("Could not load flie!");
            while (!file.EndOfStream)
            {
                data = file.ReadLine().Trim();
                _f = 'a';
                switch (data)
                {
                    case "[Music]":
                        while (_f != '[')
                        {
                            if (file.Peek() > -1) _f = (char)file.Peek(); else _f = '[';
                            if (_f != '[') 
                            {
                                data = file.ReadLine().Trim();
                                if (data.StartsWith("MUSIC="))
                                {
                                    data = data.Remove(0, 6);
                                    MusicOn = Convert.ToBoolean(data);
                                }
                                else if (data.StartsWith("VOLUME="))
                                {
                                    data = data.Remove(0, 7);
                                    Debug.Log("Music Volume Data: " + data);
                                    float vol = (float)Convert.ToDouble(data);
                                    Debug.Log("Music Volume converted: " + vol);
                                    MusicVolume = (vol / 100.0f);
                                    Debug.Log("Music Volume Final: " + MusicVolume);
                                }
                                else
                                {
                                    if (data != "") Debug.Log("Unknown config setting: " + data);
                                }
                            }
                        }
                        break;
                    case "[Sound]":
                        while (_f != '[')
                        {
                            if (file.Peek() > -1) _f = (char)file.Peek(); else _f = '[';
                            if (_f != '[')
                            {
                                data = file.ReadLine().Trim();
                                if (data.StartsWith("SOUND="))
                                {
                                    data = data.Remove(0, 6);
                                    SoundOn = Convert.ToBoolean(data);
                                }
                                else if (data.StartsWith("VOLUME="))
                                {
                                    data = data.Remove(0, 7);
                                    Debug.Log("Sound Volume Data: " + data);
                                    float vol = (float)Convert.ToDouble(data);
                                    Debug.Log("Sound Volume converted: " + vol);
                                    SoundVolume = (vol / 100.0f);
                                    Debug.Log("Sound Volume Final: " + SoundVolume);
                                }
                                else
                                {
                                    if (data != "") Debug.Log("Unknown config setting: " + data);
                                }
                            }
                        }
                        break;
                    default:
                        Debug.Log("Unknown config setting, or setting without header:" + data);
                        break;
                }
            }
        }
        catch( System.IO.FileNotFoundException e )
        {
            Debug.LogError("LoadConfig(): " + e);
            SaveConfig();
        }
        catch(Exception e)
        {
            Debug.LogError("LoadConfig(): " + e);
        }
        finally
        {
            try
            {
                if (file != null) file.Close();
            }
            catch(System.IO.IOException e )
            {
                Debug.Log("Error closing file: " + e);
            }
        }
    }   

    public static void SaveConfig()
    {
        string saveFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + @"\Trouble\";

        System.IO.StreamWriter file = null;

        try
        {

            if (!System.IO.Directory.Exists(saveFolder))
            {
                System.IO.Directory.CreateDirectory(saveFolder);
            }

            file = new System.IO.StreamWriter(saveFolder + "Trouble.cfg");
            file.WriteLine("[Sound]");
            file.WriteLine("SOUND=" + SoundOn.ToString());
            file.WriteLine("VOLUME=" + Convert.ToString(SoundVolume * 100));
            file.WriteLine("[Music]");
            file.WriteLine("MUSIC=" + MusicOn.ToString());
            file.WriteLine("VOLUME=" + Convert.ToString(MusicVolume * 100));
            Debug.Log("Config file saved successfully!");
        }
        catch(Exception e)
        {
            Debug.LogError("SaveConfig(): " + e);
        }
        finally
        {
            if (file != null) file.Close();
        }
    }

    // used for rotating the camera view

    public static int CameraDir
    {
        get { return cameraFacing; }
        set
        {
            cameraFacing = value;

            if (value < 0)
            {
                cameraFacing = 3;
            }

            if (value > 3)
            {
                cameraFacing = 0;
            }

        }
    }

    public static int BlockCount
    {
        get { return blockCount; }
        set { blockCount = value; }
    }

    public static int LockedBlocks
    {
        get { return lockedBlocks; }
        set { lockedBlocks = value; }
    }

    public static int Attempts
    {
        get { return attemptCount; }
        set { attemptCount = value; }
    }

    public static Vector3 StartPos
    {
        get { return playerPos; }
        set { playerPos = value; }
    }

    public static Quaternion StartRot
    {
        get { return playerRot; }
        set { playerRot = value; }
    }

    public static Camera MainCamera
    {
        get { return mainCamera; }
        set { mainCamera = value; }
    }

    public static bool CheckWin()
    {
        if (lockedBlocks == blockCount)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static Vector3 VectorToGrid(Vector3 v)
    {
        float x; float y; float z;
        x = Mathf.Round(v.x);
        y = Mathf.Round(v.y);
        z = Mathf.Round(v.z);
        return new Vector3(x, y, z);
    }

    public static void AddFlag(LevelFlags flag)
    {
        levelFlag = (levelFlag | flag);
        Debug.Log("Added new LEVEL flag: " + flag);
    }

    public static void AddFlag(GameFlags flag)
    {
        gameFlag = (gameFlag | flag);
        Debug.Log("Added new GAME flag: " + flag);
    }

    public static bool CheckFlag(LevelFlags flag)
    {
        return ((levelFlag & flag) == flag);
    }

    public static bool CheckFlag(GameFlags flag)
    {
        return ((gameFlag & flag) == flag);
    }

    public static void ToggleFlag(LevelFlags flag)
    {
        levelFlag ^= flag;
    }

    public static void ToggleFlag(GameFlags flag)
    {
        gameFlag ^= flag;
    }

    public static void ClearFlag(LevelFlags flag)
    {
        levelFlag &= ~flag;
        Debug.Log("Cleared LEVEL flag: " + flag);
    }

    public static void ClearFlag(GameFlags flag)
    {
        gameFlag &= ~flag;
        Debug.Log("Cleared GAME flag: " + flag);
    }

    public static void ShowFlags()
    {
        Debug.Log("Game Flags: " + gameFlag);
        Debug.Log("Level Flags: " + levelFlag);
    }

    public static void ResetLevel()
    {
        levelFlag = LevelFlags.None;
    }

    public static void ResetGame()
    {
        gameFlag = GameFlags.None;
    }

    public static bool Failed
    {
        get { return _failed; }
        set { _failed = value; }
    }

    public static void LoadNext()
    {
        if (++_level > _levelCount)
        {
            // Default to first scene
            SceneManager.LoadScene(0);
        }
        else
        {
            SceneManager.LoadScene(_level - 1);
        }
    }

    public static void LoadLevel(int levelOffset)
    {
        SceneManager.LoadScene(levelOffset);
    }

    public static void LoadLevel(string LevelName)
    {
        SceneManager.LoadScene(LevelName);
    }

    public static bool VectorIntersection(out Vector3 intersection, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
    {

        Vector3 lineVec3 = linePoint2 - linePoint1;
        Vector3 crossVec1and2 = Vector3.Cross(lineVec1, lineVec2);
        Vector3 crossVec3and2 = Vector3.Cross(lineVec3, lineVec2);

        float planarFactor = Vector3.Dot(lineVec3, crossVec1and2);

        //is coplanar, and not parrallel
        if (Mathf.Abs(planarFactor) < 0.0001f && crossVec1and2.sqrMagnitude > 0.0001f)
        {
            float s = Vector3.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude;
            intersection = linePoint1 + (lineVec1 * s);
            return true;
        }
        else
        {
            intersection = Vector3.zero;
            return false;
        }
    }


}
