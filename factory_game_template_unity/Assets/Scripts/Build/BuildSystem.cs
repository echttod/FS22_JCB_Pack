using UnityEngine;

public class BuildSystem : MonoBehaviour
{
    public BuildCatalog catalog;
    public Camera playerCamera;
    public LayerMask placementMask = ~0;
    public float maxDistance = 6f;
    public float gridSize = 1f;
    public float rotationStep = 90f;

    public KeyCode toggleKey = KeyCode.B;
    public KeyCode nextKey = KeyCode.E;
    public KeyCode prevKey = KeyCode.Q;
    public KeyCode rotateLeftKey = KeyCode.Z;
    public KeyCode rotateRightKey = KeyCode.C;

    private bool _buildMode;
    private int _selectedIndex;
    private string[] _ids = new string[0];
    private GameObject _ghost;
    private BuildGhost _ghostScript;
    private float _rotationY;
    private bool _hasPlacementHit;
    private float _ghostYOffset;
    private GameObject _currentPrefab;

    public bool IsBuildMode => _buildMode;
    public string SelectedId => _ids.Length > 0 ? _ids[_selectedIndex] : string.Empty;

    private void Start()
    {
        RefreshCatalog();
    }

    public void RefreshCatalog()
    {
        if (catalog == null)
        {
            return;
        }

        _ids = catalog.GetIds();
        _selectedIndex = _ids.Length > 0 ? Mathf.Clamp(_selectedIndex, 0, _ids.Length - 1) : 0;
        RebuildGhost();
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleBuildMode();
        }

        if (!_buildMode)
        {
            if (_ghost != null)
            {
                _ghost.SetActive(false);
            }
            return;
        }

        HandleSelectionInput();
        UpdateGhost();

        if (Input.GetMouseButtonDown(0))
        {
            TryPlace();
        }
    }

    private void ToggleBuildMode()
    {
        _buildMode = !_buildMode;
        if (_ghost != null)
        {
            _ghost.SetActive(_buildMode);
        }
    }

    private void HandleSelectionInput()
    {
        if (_ids.Length == 0)
        {
            return;
        }

        if (Input.GetKeyDown(nextKey))
        {
            _selectedIndex = (_selectedIndex + 1) % _ids.Length;
            RebuildGhost();
        }
        else if (Input.GetKeyDown(prevKey))
        {
            _selectedIndex = (_selectedIndex - 1 + _ids.Length) % _ids.Length;
            RebuildGhost();
        }

        if (Input.GetKeyDown(rotateLeftKey))
        {
            _rotationY -= rotationStep;
        }
        else if (Input.GetKeyDown(rotateRightKey))
        {
            _rotationY += rotationStep;
        }
    }

    private void UpdateGhost()
    {
        if (_ghost == null)
        {
            return;
        }

        if (playerCamera == null)
        {
            _hasPlacementHit = false;
            if (_ghostScript != null)
            {
                _ghostScript.SetValid(false);
            }
            return;
        }

        _ghost.SetActive(true);
        _ghost.transform.rotation = Quaternion.Euler(0f, _rotationY, 0f);

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, placementMask))
        {
            Vector3 snapped = SnapToGrid(hit.point);
            snapped.y += _ghostYOffset;
            _ghost.transform.position = snapped;
            _hasPlacementHit = true;
            if (_ghostScript != null)
            {
                _ghostScript.SetValid(true);
            }
        }
        else
        {
            _hasPlacementHit = false;
            if (_ghostScript != null)
            {
                _ghostScript.SetValid(false);
            }
        }
    }

    private void TryPlace()
    {
        if (!_hasPlacementHit || _currentPrefab == null)
        {
            return;
        }

        Vector3 pos = _ghost.transform.position;
        Quaternion rot = _ghost.transform.rotation;
        GameObject placed = Instantiate(_currentPrefab, pos, rot);
        placed.name = _currentPrefab.name;
        placed.SetActive(true);
    }

    private void RebuildGhost()
    {
        if (_ghost != null)
        {
            Destroy(_ghost);
        }

        _currentPrefab = null;
        if (_ids.Length == 0 || catalog == null)
        {
            return;
        }

        string id = SelectedId;
        GameObject prefab = catalog.GetPrefab(id);
        if (prefab == null)
        {
            return;
        }

        _currentPrefab = prefab;
        _ghost = Instantiate(prefab);
        _ghost.name = prefab.name + "_Ghost";
        _ghostScript = _ghost.GetComponent<BuildGhost>();
        if (_ghostScript == null)
        {
            _ghostScript = _ghost.AddComponent<BuildGhost>();
        }

        foreach (Collider col in _ghost.GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }

        _ghostYOffset = ComputeYOffset(_ghost);
        _ghost.SetActive(_buildMode);
    }

    private float ComputeYOffset(GameObject target)
    {
        Buildable buildable = target.GetComponent<Buildable>();
        if (buildable != null)
        {
            return buildable.GetWorldBounds().extents.y;
        }

        Renderer renderer = target.GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            return renderer.bounds.extents.y;
        }

        return 0.5f;
    }

    private Vector3 SnapToGrid(Vector3 position)
    {
        if (gridSize <= 0.01f)
        {
            return position;
        }

        float x = Mathf.Round(position.x / gridSize) * gridSize;
        float z = Mathf.Round(position.z / gridSize) * gridSize;
        return new Vector3(x, position.y, z);
    }
}
