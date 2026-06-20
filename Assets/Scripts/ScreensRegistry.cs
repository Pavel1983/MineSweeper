using UnityEngine;

[CreateAssetMenu(fileName = "ScreensRegistry", menuName = "Scriptable Objects/ScreensRegistry")]
public class ScreensRegistry : ScriptableObject
{
    [SerializeField] private BaseScreen[] _screens;

    public bool TryGetPrefab(int screenId, out BaseScreen prefab)
    {
        for (var i = 0; i < _screens.Length; i++)
        {
            if (_screens[i].ScreenId == screenId)
            {
                prefab = _screens[i];
                return true;
            }
        }

        prefab = null;
        return false;
    }
}
