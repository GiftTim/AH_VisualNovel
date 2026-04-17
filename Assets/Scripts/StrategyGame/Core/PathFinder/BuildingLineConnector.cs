using UnityEngine;

[System.Serializable]
public class RoadPath
{
    public Spot buildingSpot;
    public LineRenderer lineRenderer;
    public Transform[] waypoints;
}

[ExecuteAlways]
public class BuildingLineConnector : MonoBehaviour
{
    [SerializeField] private Spot _hqSpot;
    [SerializeField] private RoadPath[] _roads;
    [SerializeField] private float _lineZ = -1f;

    private void Update()
    {
        ApplyLines();
    }

    private void ApplyLines()
    {
        if (_hqSpot == null || _roads == null) return;

        Vector3 hqPos = _hqSpot.transform.position;
        hqPos.z = _lineZ;

        foreach (RoadPath road in _roads)
        {
            if (road.lineRenderer == null || road.buildingSpot == null) continue;
            if (road.waypoints == null) continue;

            int count = 2 + road.waypoints.Length;
            road.lineRenderer.useWorldSpace = true;
            road.lineRenderer.positionCount = count;

            road.lineRenderer.SetPosition(0, hqPos);

            for (int i = 0; i < road.waypoints.Length; i++)
            {
                if (road.waypoints[i] == null) continue;
                Vector3 wp = road.waypoints[i].position;
                wp.z = _lineZ;
                road.lineRenderer.SetPosition(i + 1, wp);
            }

            Vector3 buildingPos = road.buildingSpot.transform.position;
            buildingPos.z = _lineZ;
            road.lineRenderer.SetPosition(count - 1, buildingPos);
        }
    }
}
