using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PathFinderManager : MonoBehaviour
{
    [SerializeField] Spot _hq;
    [SerializeField] Image _linePrefab;
    [SerializeField] Transform _lineParent;
    [SerializeField] Color _defaultLineColor = new Color(1f, 1f, 1f, 0.3f);
    [SerializeField] Color _highlightLineColor = Color.yellow;

    Dictionary<(Spot, Spot), Image> _connectionLines;

    void Start()
    {
        _connectionLines = new Dictionary<(Spot, Spot), Image>();
        var allSpots = FindObjectsByType<Spot>(FindObjectsSortMode.None);
        var created = new HashSet<(Spot, Spot)>();

        foreach (var spot in allSpots)
        {
            foreach (var neighbor in spot.Neighbors)
            {
                var key = MakeKey(spot, neighbor);
                if (created.Contains(key)) continue;
                created.Add(key);
                var line = CreateConnectionLine(spot, neighbor);
                _connectionLines[key] = line;
            }
        }
    }

    Image CreateConnectionLine(Spot a, Spot b)
    {
        var ra = a.GetComponent<RectTransform>();
        var rb = b.GetComponent<RectTransform>();
        Vector2 posA = ra.anchoredPosition;
        Vector2 posB = rb.anchoredPosition;
        Vector2 mid = (posA + posB) * 0.5f;
        float dist = Vector2.Distance(posA, posB);
        float angle = Mathf.Atan2(posB.y - posA.y, posB.x - posA.x) * Mathf.Rad2Deg;

        var line = Instantiate(_linePrefab, _lineParent);
        var rt = line.GetComponent<RectTransform>();
        rt.anchoredPosition = mid;
        rt.sizeDelta = new Vector2(dist, rt.sizeDelta.y);
        rt.localEulerAngles = new Vector3(0f, 0f, angle);
        line.color = _defaultLineColor;
        return line;
    }

    public List<Spot> GetPath(Spot from, Spot to)
    {
        var prev = new Dictionary<Spot, Spot>();
        var queue = new Queue<Spot>();
        queue.Enqueue(from);
        prev[from] = null;

        while (queue.Count > 0)
        {
            var cur = queue.Dequeue();
            if (cur == to) break;
            foreach (var nb in cur.Neighbors)
            {
                if (!prev.ContainsKey(nb))
                {
                    prev[nb] = cur;
                    queue.Enqueue(nb);
                }
            }
        }

        if (!prev.ContainsKey(to)) return new List<Spot>();

        var path = new List<Spot>();
        for (var n = to; n != null; n = prev[n])
            path.Insert(0, n);
        return path;
    }

    public void HighlightPath(List<Spot> path)
    {
        ClearHighlight();
        for (int i = 0; i < path.Count - 1; i++)
        {
            var key = MakeKey(path[i], path[i + 1]);
            if (_connectionLines.TryGetValue(key, out var img))
                img.color = _highlightLineColor;
        }
    }

    public void ClearHighlight()
    {
        foreach (var img in _connectionLines.Values)
            img.color = _defaultLineColor;
    }

    (Spot, Spot) MakeKey(Spot a, Spot b)
        => a.GetInstanceID() < b.GetInstanceID() ? (a, b) : (b, a);
}
