using Entity;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PathController : MonoBehaviour
{
    public GameObject waypointPrefab;

    private LineRenderer pathRenderer;
    private LineRenderer previewRenderer;
    private SpriteRenderer previewSprite;
    private EdgeCollider2D edgeCollider;

    private LinkedList<(WaypointViewController, int)> waypointViewControllers;
    private AdversaryViewController adversaryViewController;
    private SnapController snapController;
    private MainController mainController;

    private bool building;

    public Path Path { get; set; }

    private void Awake()
    {
        this.snapController = Camera.main.GetComponent<SnapController>();
        this.mainController = Camera.main.GetComponent<MainController>();

        this.Path = new Path();

        waypointViewControllers = new LinkedList<(WaypointViewController, int)>();

        pathRenderer = gameObject.GetComponent<LineRenderer>();
        pathRenderer.startWidth = pathRenderer.endWidth = 0.1f;

        previewRenderer = gameObject.transform.GetChild(0).gameObject.GetComponent<LineRenderer>();
        previewRenderer.startWidth = previewRenderer.endWidth = 0.1f;

        edgeCollider = gameObject.GetComponent<EdgeCollider2D>();
        previewSprite = gameObject.transform.GetChild(1).gameObject.GetComponent<SpriteRenderer>();
        gameObject.transform.position = HeightUtil.SetZ(gameObject.transform.position, HeightUtil.PATH_SELECTED);

        building = true;

        EventManager.StartListening(typeof(MouseClickAction), x =>
        {
            if (building)
            {
                var action = new MouseClickAction(x);
                AddMoveToWaypoint(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            }
        });
    }

    public bool shouldIgnoreWaypoints()
    {
        return adversaryViewController.shouldIgnoreWaypoints();
    }

    public bool isBuilding()
    {
        return building;
    }

    public void select(bool forward = false)
    {
        adjustHeights(true);
        previewSprite.enabled = true;
        building = true;
        if (forward)
        {
            adversaryViewController.select();
        }
    }

    public void adjustHeights(bool selected)
    {
        for (var i = 0; i < pathRenderer.positionCount; i++)
        {
            pathRenderer.SetPosition(i, HeightUtil.SetZ(pathRenderer.GetPosition(i), selected ? HeightUtil.PATH_SELECTED : HeightUtil.PATH_DESELECTED));
        }
        gameObject.transform.position = HeightUtil.SetZ(gameObject.transform.position, selected ? HeightUtil.PATH_SELECTED : HeightUtil.PATH_DESELECTED);
        foreach (var (waypoint, _) in waypointViewControllers)
        {
            waypoint.gameObject.transform.position = HeightUtil.SetZ(waypoint.gameObject.transform.position, selected ? HeightUtil.WAYPOINT_SELECTED : HeightUtil.WAYPOINT_DESELECTED);
        }
    }

    public void deselect(bool forward = false)
    {
        adjustHeights(false);
        previewRenderer.positionCount = 0;
        previewSprite.enabled = false;
        building = false;
        if (forward)
        {
            adversaryViewController.deselect();
        }
    }

    public void Destroy()
    {
        foreach (var (wp, _) in waypointViewControllers)
        {
            Destroy(wp.gameObject);
        }
        Destroy(gameObject);
    }

    public void SetEntityController(AdversaryViewController controller)
    {
        this.adversaryViewController = controller;
        AddMoveToWaypoint(controller.getEntity().SpawnPoint.Vector3); //init with starting position of car -- should be set in model or on export
        waypointViewControllers.First.Value.Item1.gameObject.SetActive(false);
    }

    public void Update()
    {
        var mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (building)
        {
            AddMoveToWaypoint(mousePosition, preview: true);
        } else
        {
            var waypoint = snapController.FindWaypoint(mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(HeightUtil.SetZ(mousePosition, -10), -Vector2.up);
            if (hit.collider == this.edgeCollider)
            {
                previewSprite.enabled = true;
                previewSprite.transform.position = HeightUtil.SetZ(waypoint.Vector3, -0.1f);
            } else
            {
                previewSprite.enabled = false;
            }
        }
    }

    public void AddMoveToWaypoint(Vector2 mousePosition, bool preview = false)
    {
        Location waypoint;
        if (!this.adversaryViewController.shouldIgnoreWaypoints())
        {
            waypoint = snapController.FindWaypoint(mousePosition);
        } else
        {
            waypoint = new Location(mousePosition);
        }

        if (waypoint == null)
        {
            Debug.Log("Invalid mouse position, mouse probably not on road!");
            return;
        }

        RaycastHit2D waypointHit = Physics2D.Raycast(HeightUtil.SetZ(waypoint.Vector3, -10), -Vector2.up);
        RaycastHit2D mouseHit = Physics2D.Raycast(HeightUtil.SetZ(mousePosition, -10), -Vector2.up);
        if (waypointHit.collider == this.edgeCollider || mouseHit.collider == this.edgeCollider)
        {
            previewSprite.enabled = true;
            previewSprite.transform.position = HeightUtil.SetZ(waypoint.Vector3, -0.1f);
            previewRenderer.positionCount = 0;
            return;
        }
        // allow first waypoint to be placed on top of car, but none afterwards
        if ((waypointHit.collider == adversaryViewController.getCollider() || mouseHit.collider == adversaryViewController.getCollider()) && !Path.IsEmpty())
        {
            previewSprite.enabled = false;
            previewRenderer.positionCount = 0;
            return;
        }

        //if hit collider is above own waypoint, render no preview
        foreach (var entry in waypointViewControllers)
        {
            var waypointController = entry.Item1;
            var collider = waypointController.gameObject.GetComponent<CircleCollider2D>();
            if (waypointHit.collider == collider)
            {
                previewSprite.enabled = false;
                previewRenderer.positionCount = 0;
                return;
            }
        }

        LineRenderer lineRenderer;

        if (preview && !Path.IsEmpty())
        {
            lineRenderer = previewRenderer;
            lineRenderer.positionCount = 1;
            lineRenderer.SetPosition(0, HeightUtil.SetZ(waypointViewControllers.Last.Value.Item1.transform.position, HeightUtil.PATH_SELECTED));
        }
        else
        {
            lineRenderer = pathRenderer;
        }

        //var actionType = new ActionType("MoveToAction");

        var pathLen = 0;

        var laneChanges = new List<int>();
        var path = new List<Vector2>();

        if (Path.IsEmpty())
        {
            lineRenderer.SetPosition(lineRenderer.positionCount++, HeightUtil.SetZ(waypoint.Vector3, HeightUtil.PATH_SELECTED));
        }
        else
        {
            (path, laneChanges) = snapController.FindPath(waypointViewControllers.Last.Value.Item1.transform.position, waypoint.Vector3, this.adversaryViewController.shouldIgnoreWaypoints());
            path.RemoveAt(0);
            pathLen = path.Count;

            if (path is null)
            {
                return; //this kinda shouldnt happen
            }

            foreach (var coord in path)
            {
                lineRenderer.SetPosition(lineRenderer.positionCount++, HeightUtil.SetZ(coord, HeightUtil.PATH_SELECTED));
            }
        }

        if (!preview)
        {
            var used = 0;
            foreach(var laneChange in laneChanges)
            {
                if(laneChange != 0)
                {
                    createWaypointGameObject(path[laneChange].x, path[laneChange].y, laneChange + 1 - used);
                    used += laneChange + 1;
                }
                if (laneChange != path.Count() - 1)
                {
                    createWaypointGameObject(path[laneChange + 1].x, path[laneChange + 1].y, 1);
                    used += 1;
                }
            }
            var viewController = createWaypointGameObject(waypoint.X, waypoint.Y, pathLen - used);
            if (Path.WaypointList.Count() > 1)
            {
                mainController.setSelectedEntity(viewController);
            }
            if(Path.WaypointList.Count() == 2 && adversaryViewController.shouldIgnoreWaypoints())
            {

            }
            resetEdgeCollider();
        } else
        {
            previewSprite.enabled = true;
            previewSprite.transform.position = HeightUtil.SetZ(waypoint.Vector3, -0.1f);
        }
    }

    WaypointViewController createWaypointGameObject(float x, float y, int pathLen)
    {
        GameObject wpGameObject = Instantiate(waypointPrefab, new Vector3(x, y, HeightUtil.WAYPOINT_SELECTED), Quaternion.identity);
        WaypointViewController viewController = wpGameObject.GetComponent<WaypointViewController>();
        viewController.setPathController(this);
        viewController.setColor(pathRenderer.startColor);
        viewController.setIgnoreWaypoints(this.shouldIgnoreWaypoints());
        viewController.waypoint = generateWaypoint(new Location(new Vector3(x, y, 0), 0), new ActionType("MoveToAction"));
        viewController.waypoint.View = viewController;
        this.Path.WaypointList.Add(viewController.waypoint);
        waypointViewControllers.AddLast((viewController, pathLen));
        return viewController;
    }

    Waypoint generateWaypoint(Location loc, ActionType actionType)
    {
        List<TriggerInfo> triggersLaneChange = new List<TriggerInfo>();
        var locationTrigger = new Location(0, 0, 0, 0); // var locationTrigger = nextWaypoint.Location;
        if (!Path.IsEmpty())
        {
            locationTrigger = waypointViewControllers.Last.Value.Item1.waypoint.Location;
        }
        triggersLaneChange.Add(new TriggerInfo("DistanceCondition", null, "lessThan", 20, locationTrigger)); // TODO change 20
        return new Waypoint(loc, actionType, triggersLaneChange);
    }

    void resetEdgeCollider()
    {
        Vector2[] positions = new Vector2[pathRenderer.positionCount];
        for (var i = 0; i < pathRenderer.positionCount; i++)
        {
            positions[i] = pathRenderer.GetPosition(i);
        }
        edgeCollider.SetPoints(positions.ToList());
    }

    public void MoveWaypoint(WaypointViewController waypointController, Location location)
    {
        LinkedListNode<(WaypointViewController, int)> prev = null, next = null, cur = null;
        int prevIndex = 0;
        for (LinkedListNode<(WaypointViewController, int)> wp = waypointViewControllers.First; wp != null; wp = wp.Next)
        {
            if (wp.Value.Item1 == waypointController)
            {
                cur = wp;
                prev = wp.Previous;
                next = wp.Next;
                break;
            }
            else
            {
                prevIndex += wp.Value.Item2;
            }
        }

        waypointController.waypoint.setLocation(location);

        List<Vector2> prevPath = new List<Vector2>();
        List<Vector2> nextPath = new List<Vector2>();

        bool ignoreWaypoints = waypointController.shouldIgnoreWaypoints();

        var offset = 0;
        if (prev != null)
        {
            (prevPath, _) = snapController.FindPath(prev.Value.Item1.waypoint.Location.Vector3, location.Vector3, ignoreWaypoints || prev.Value.Item1.shouldIgnoreWaypoints());
            prev.Value.Item1.waypoint.setLocation(new Location(prevPath[0]));
            prevPath.RemoveAt(prevPath.Count - 1);
            offset = offset + prevPath.Count - cur.Value.Item2;
        }
        if (next != null)
        {
            (nextPath, _) = snapController.FindPath(location.Vector3, next.Value.Item1.waypoint.Location.Vector3, ignoreWaypoints || next.Value.Item1.shouldIgnoreWaypoints());
            next.Value.Item1.waypoint.setLocation(new Location(nextPath[nextPath.Count - 1]));
            nextPath.RemoveAt(0);
            offset = offset + nextPath.Count - next.Value.Item2;
        }

        Vector2[] positions = new Vector2[pathRenderer.positionCount + offset];

        for (var i = 0; i < prevIndex; i++)
        {
            positions[i] = pathRenderer.GetPosition(i);
        }

        for (var i = 0; i < prevPath.Count; i++)
        {
            positions[prevIndex + i] = prevPath[i];
        }

        positions[prevIndex + prevPath.Count] = location.Vector3;

        for (var i = 0; i < nextPath.Count; i++)
        {
            positions[prevIndex + prevPath.Count + i + 1] = nextPath[i];
        }

        for (var i = prevIndex + prevPath.Count + nextPath.Count + 1; i < pathRenderer.positionCount + offset; i++)
        {
            positions[i] = pathRenderer.GetPosition(i - offset);
        }

        pathRenderer.positionCount = pathRenderer.positionCount + offset;
        for (var i = 0; i < pathRenderer.positionCount; i++)
        {
            pathRenderer.SetPosition(i, new Vector3(positions[i].x, positions[i].y, transform.position.z));
        }

        cur.Value = (cur.Value.Item1, prevPath.Count);
        if (next is not null)
        {
            next.Value = (next.Value.Item1, nextPath.Count);
        }

        waypointController.waypoint.setLocation(new Location(location.Vector3, 0));

        mainController.moveActionButtons(location.Vector3);

        resetEdgeCollider();
    }

    public void removeWaypoint(WaypointViewController controller)
    {
        LinkedListNode<(WaypointViewController, int)> prev = null, next = null, cur = null;
        int prevIndex = 0;
        for (LinkedListNode<(WaypointViewController, int)> wp = waypointViewControllers.First; wp != null; wp = wp.Next)
        {
            if (wp.Value.Item1.waypoint == controller.waypoint)
            {
                cur = wp;
                prev = wp.Previous;
                next = wp.Next;
                break;
            }
            else
            {
                prevIndex += wp.Value.Item2;
            }
        }

        // dont allow destruction of first waypoint
        if (prev == null)
        {
            return;
        }

        if (next != null)
        {
            bool ignoreWaypoints = controller.shouldIgnoreWaypoints();
            (var path, _) = snapController.FindPath(prev.Value.Item1.waypoint.Location.Vector3, next.Value.Item1.waypoint.Location.Vector3, ignoreWaypoints || next.Value.Item1.shouldIgnoreWaypoints());
            path.RemoveAt(path.Count - 1);
            var offset = path.Count - cur.Value.Item2 - next.Value.Item2;

            Vector2[] positions = new Vector2[pathRenderer.positionCount + offset];

            for (var i = 0; i < prevIndex; i++)
            {
                positions[i] = pathRenderer.GetPosition(i);
            }

            for (var i = 0; i < path.Count; i++)
            {
                positions[prevIndex + i] = path[i];
            }

            for (var i = prevIndex + path.Count; i < pathRenderer.positionCount + offset; i++)
            {
                positions[i] = pathRenderer.GetPosition(i - offset);
            }

            pathRenderer.positionCount = pathRenderer.positionCount + offset;
            for (var i = 0; i < pathRenderer.positionCount; i++)
            {
                pathRenderer.SetPosition(i, new Vector3(positions[i].x, positions[i].y, transform.position.z));
            }
            next.Value = (next.Value.Item1, path.Count);

        }
        else
        {
            pathRenderer.positionCount -= cur.Value.Item2;
        }

        Path.WaypointList.Remove(controller.waypoint);
        waypointViewControllers.Remove(cur);
        resetEdgeCollider();
    }

    public void MoveFirstWaypoint(Location location)
    {
        this.MoveWaypoint(this.waypointViewControllers.First.Value.Item1, location);
    }

    public void SetColor(Color color)
    {
        this.pathRenderer.startColor = this.pathRenderer.endColor = color;
        color = new Color(color.r, color.g, color.b, 0.5f);
        this.previewRenderer.startColor = this.previewRenderer.endColor = color;
        this.previewSprite.color = color;
        foreach (var (waypoint, _) in waypointViewControllers)
        {
            waypoint.setColor(color);
        }
    }

    public void OnMouseDown()
    {
        if(snapController.IgnoreClicks && !building)
        {
            EventManager.TriggerEvent(new MouseClickAction(Camera.main.ScreenToWorldPoint(Input.mousePosition)));
            return;
        }
        //find closest linerenderer position to the click
        double min = double.MaxValue;
        Vector2 location = Vector2.zero;
        double dist = double.MaxValue;
        int index = 0;
        for (int i = 0; i < pathRenderer.positionCount; i++)
        {
            if ((dist = SnapController.FastEuclideanDistance(Camera.main.ScreenToWorldPoint(Input.mousePosition), pathRenderer.GetPosition(i))) < min)
            {
                min = dist;
                location = pathRenderer.GetPosition(i);
                index = i;
            }
        }

        //check if there is a waypoint at the position of the linerenderer position
        //waypoints visual position and actual location can differ
        //always disregard waypoint indicators from snapController here; rely on linerenderer
        WaypointViewController nearestWaypoint = null;
        foreach (var (waypoint, _) in waypointViewControllers)
        {
            if (waypoint.waypoint.Location.X == location.x && waypoint.waypoint.Location.Y == location.y)
            {
                nearestWaypoint = waypoint;
                break;
            }
        }
        if (nearestWaypoint != null)
        {
            nearestWaypoint.select();
        }
        else
        {
            GameObject wpGameObject = Instantiate(waypointPrefab, new Vector3(location.x, location.y, HeightUtil.WAYPOINT_SELECTED), Quaternion.identity);

            WaypointViewController viewController = wpGameObject.GetComponent<WaypointViewController>();
            viewController.setPathController(this);
            viewController.setColor(pathRenderer.startColor);
            viewController.waypoint = generateWaypoint(new Location(location, 0), new ActionType("MoveToAction"));
            viewController.waypoint.View = viewController;

            var cur = waypointViewControllers.First;
            var curPathIndex = 0;
            while (index - cur.Next.Value.Item2 > 0)
            {
                index -= cur.Next.Value.Item2;
                cur = cur.Next;
                curPathIndex++;
            }
            var next = cur.Next;
            waypointViewControllers.AddAfter(cur, (viewController, index));
            next.Value = (next.Value.Item1, next.Value.Item2 - index);

            this.Path.WaypointList.Insert(curPathIndex + 1, viewController.waypoint);

            MoveWaypoint(viewController, viewController.waypoint.Location); // fix paths / deleting waypoint may make A* necessary
            resetEdgeCollider();

            mainController.setSelectedEntity(viewController);
        }
    }

    //TODO find better solution
    public WaypointViewController getFirstWaypointController()
    {
        return this.waypointViewControllers.First.Value.Item1;
    }
}