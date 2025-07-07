using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;

public class AStarGridManager : MonoBehaviour
{
    public Tilemap groundTilemap;            // walkable tiles
    public Tilemap collisionTilemap;         // unwalkable tiles
    public LayerMask collisionLayer;         // unwalkable objects (colliders)
    public Vector2Int gridSize;              // in cells
    public Vector3 gridOrigin;               // bottom-left cell of grid
    public float nodeCheckRadius = 0.25f;    // radius used for physics check

    private Node[,] grid;

    public static AStarGridManager Instance { get; private set; }

    private void Awake()
	{
		// Skip if on MainMenu scene
		if (SceneManager.GetActiveScene().name == "MainMenu") return;

		Instance = this;

		// Auto-find tilemaps if not manually assigned
		if (groundTilemap == null)
			groundTilemap = GameObject.Find("Ground")?.GetComponent<Tilemap>();

		if (collisionTilemap == null)
			collisionTilemap = GameObject.Find("Collision")?.GetComponent<Tilemap>();

		if (groundTilemap == null || collisionTilemap == null)
		{
			Debug.LogWarning("Tilemaps not found! AStarGridManager will not create the grid.");
			return;
		}

		// Set origin and size from ground tilemap
		var bounds = groundTilemap.cellBounds;
		gridOrigin = bounds.min;
		gridSize = new Vector2Int(bounds.size.x, bounds.size.y);

		StartCoroutine(CreateGridCoroutine());

	}

	private void OnEnable()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void OnDisable()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if (scene.name == "MainMenu") return;

		Instance = this;

		groundTilemap = GameObject.Find("Ground")?.GetComponent<Tilemap>();
		collisionTilemap = GameObject.Find("Collision")?.GetComponent<Tilemap>();

		if (groundTilemap == null || collisionTilemap == null)
		{
			Debug.LogWarning("Tilemaps not found! AStarGridManager will not create the grid.");
			return;
		}

		var bounds = groundTilemap.cellBounds;
		gridOrigin = bounds.min;
		gridSize = new Vector2Int(bounds.size.x, bounds.size.y);

		StartCoroutine(CreateGridCoroutine());

	}

	IEnumerator CreateGridCoroutine()
	{
		grid = new Node[gridSize.x, gridSize.y];
    
		for (int x = 0; x < gridSize.x; x++)
		{
			for (int y = 0; y < gridSize.y; y++)
			{
				Vector3Int tilePos = new Vector3Int(x + (int)gridOrigin.x, y + (int)gridOrigin.y, 0);
				Vector3 worldPos = groundTilemap.CellToWorld(tilePos) + groundTilemap.cellSize / 2f;

				bool hasTileCollision = collisionTilemap.GetTile(tilePos) != null;
				bool hasPhysicsCollision = Physics2D.OverlapCircle(worldPos, nodeCheckRadius, collisionLayer);
				bool walkable = !hasTileCollision && !hasPhysicsCollision;

				grid[x, y] = new Node(walkable, worldPos, x, y);
			}

			// Yield after each column or some number of nodes to spread out workload
			if (x % 10 == 0) yield return null; 
		}

		Debug.Log("Grid creation complete");
	}


    public Node NodeFromWorldPoint(Vector3 worldPos)
    {
        Vector3Int cell = groundTilemap.WorldToCell(worldPos);
        int x = cell.x - (int)gridOrigin.x;
        int y = cell.y - (int)gridOrigin.y;
        x = Mathf.Clamp(x, 0, gridSize.x - 1);
        y = Mathf.Clamp(y, 0, gridSize.y - 1);
        return grid[x, y];
    }

	public List<Node> GetNeighbours(Node node)
	{
		List<Node> neighbours = new();

		for (int dx = -1; dx <= 1; dx++)
		{
			for (int dy = -1; dy <= 1; dy++)
			{
				if (dx == 0 && dy == 0) continue;

				int checkX = node.gridX + dx;
				int checkY = node.gridY + dy;

				if (checkX >= 0 && checkX < gridSize.x && checkY >= 0 && checkY < gridSize.y)
				{
					Node neighbor = grid[checkX, checkY];

					if (!neighbor.walkable)
						continue;

					// Prevent diagonal moves through corners
					if (dx != 0 && dy != 0)
					{
						Node nodeX = grid[node.gridX + dx, node.gridY];    // horizontal neighbor
						Node nodeY = grid[node.gridX, node.gridY + dy];    // vertical neighbor

						if (!nodeX.walkable || !nodeY.walkable)
							continue; // one of the adjacent nodes is blocked, so skip this diagonal neighbor
					}

					neighbours.Add(neighbor);
				}
			}
		}

		return neighbours;
	}

}
