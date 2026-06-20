using UnityEngine;

[CreateAssetMenu(fileName = "LevelConfig", menuName = "Scriptable Objects/LevelConfig")]
public class LevelConfig : ScriptableObject
{
    [Min(2)]
    public int Cols;
    [Min(2)]
    public int Rows;
    [Min(1)]
    public int MinesCount;
}
