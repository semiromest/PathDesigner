using UnityEngine;

#if UNITY_EDITOR
[ExecuteInEditMode]
#endif
public class PathDesigner : MonoBehaviour
{
    public Transform[] waypoints;
    public int resolution = 10;
    public bool loop = false;
    public GameObject[] objectPrefabs; 
    public float placementRadius = 1f; 

    private Vector3[] pathPoints;
    private GameObject[] placedObjects;

    private int previousResolution;
    private bool previousLoop;
    private float previousPlacementRadius;

    private GameObject objectsParent;

#if UNITY_EDITOR
    private void Update()
    {
        if (!Application.isPlaying)
        {
            if (HasSettingsChanged())
            {
                GeneratePathPoints();
                ClearPlacedObjects();
                PlaceObjectsOnPath();
                UpdatePreviousSettings();
            }
        }
    }
#endif

    private void Start()
    {
#if !UNITY_EDITOR
        GeneratePathPoints();
        PlaceObjectsOnPath();
#endif
    }

    private void OnDrawGizmos()
    {
        GeneratePathPoints();

        if (pathPoints.Length > 1)
        {
            for (int i = 0; i < pathPoints.Length - 1; i++)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(pathPoints[i], pathPoints[i + 1]);
            }

            if (loop)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(pathPoints[pathPoints.Length - 1], pathPoints[0]);
            }
        }
    }

    private void GeneratePathPoints()
    {
        int numPoints = waypoints.Length * resolution;
        pathPoints = new Vector3[numPoints];
        int index = 0;

        for (int i = 0; i < waypoints.Length; i++)
        {
            int nextIndex = (i + 1) % waypoints.Length;
            float tStep = 1f / resolution;

            for (int j = 0; j < resolution; j++)
            {
                float t = j * tStep;
                pathPoints[index] = CalculateBezierPoint(waypoints[i].position, waypoints[i].position + waypoints[i].forward, waypoints[nextIndex].position - waypoints[nextIndex].forward, waypoints[nextIndex].position, t);
                index++;
            }
        }
    }

    private Vector3 CalculateBezierPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float u = 1f - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        Vector3 point = uuu * p0 + 3f * uu * t * p1 + 3f * u * tt * p2 + ttt * p3;
        return point;
    }

    private void PlaceObjectsOnPath()
    {
        placedObjects = new GameObject[pathPoints.Length];
        DestroyImmediate(objectsParent);
        objectsParent = new GameObject("Placed Objects");

        for (int i = 0; i < pathPoints.Length; i++)
        {
            Vector3 randomOffset = Random.insideUnitCircle * placementRadius;
            Vector3 objectPosition = pathPoints[i] + randomOffset;

            GameObject selectedPrefab = objectPrefabs[Random.Range(0, objectPrefabs.Length)];
            placedObjects[i] = Instantiate(selectedPrefab, objectPosition, Quaternion.identity);
            placedObjects[i].transform.rotation = Random.rotation; 
            placedObjects[i].transform.SetParent(objectsParent.transform);
        }
    }

    private void ClearPlacedObjects()
    {
        if (placedObjects != null)
        {
            for (int i = 0; i < placedObjects.Length; i++)
            {
                if (placedObjects[i] != null)
                {
                    DestroyImmediate(placedObjects[i]);
                }
            }

            placedObjects = null;
        }
    }

    private bool HasSettingsChanged()
    {
        return resolution != previousResolution || loop != previousLoop || placementRadius != previousPlacementRadius;
    }

    private void UpdatePreviousSettings()
    {
        previousResolution = resolution;
        previousLoop = loop;
        previousPlacementRadius = placementRadius;
    }
}
