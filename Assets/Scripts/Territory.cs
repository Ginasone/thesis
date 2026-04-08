using UnityEngine;
using UnityEngine.UI;

public class Territory : MonoBehaviour
{
    // Territory properties
    public string territoryName;
    public int ownerPlayer; // 0 = neutral, 1 = player, 2 = AI
    public int unitCount = 5;

    // Visual components
    private SpriteRenderer spriteRenderer;
    private Text unitCountText;
    private Canvas canvas;

    // Colors for ownership
    private Color neutralColor = new Color(0.7f, 0.7f, 0.7f); // Gray
    private Color playerColor = new Color(0.3f, 0.5f, 1f); // Blue
    private Color aiColor = new Color(1f, 0.3f, 0.3f); // Red

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Create canvas for this territory
        GameObject canvasObj = new GameObject("UnitCanvas");
        canvasObj.transform.SetParent(transform);
        canvasObj.transform.localPosition = Vector3.zero;

        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        // Scale the canvas
        RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(2, 2);
        canvasRect.localScale = new Vector3(0.01f, 0.01f, 1f);

        // Create text for unit count
        GameObject textObj = new GameObject("UnitCountText");
        textObj.transform.SetParent(canvasObj.transform);
        textObj.transform.localPosition = Vector3.zero;

        unitCountText = textObj.AddComponent<Text>();
        unitCountText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        unitCountText.text = unitCount.ToString();
        unitCountText.fontSize = 100;
        unitCountText.fontStyle = FontStyle.Bold;
        unitCountText.alignment = TextAnchor.MiddleCenter;
        unitCountText.color = Color.white;

        // Add outline for better visibility
        Outline outline = textObj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(3, 3);

        // Position text
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 0);
        textRect.anchorMax = new Vector2(1, 1);
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;

        UpdateVisuals();
    }

    void OnMouseDown()
    {
        // When territory is clicked, notify the GameManager
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.OnTerritoryClicked(this);
        }
    }

    void OnMouseEnter()
    {
        // Highlight territory on hover
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.Lerp(spriteRenderer.color, Color.white, 0.3f);
        }
    }

    void OnMouseExit()
    {
        // Remove highlight
        UpdateVisuals();
    }

    public void SetOwner(int player)
    {
        ownerPlayer = player;
        UpdateVisuals();
    }

    public void SetUnits(int count)
    {
        unitCount = Mathf.Max(0, count);
        if (unitCountText != null)
        {
            unitCountText.text = unitCount.ToString();
        }
    }

    public void AddUnits(int count)
    {
        unitCount += count;
        if (unitCountText != null)
        {
            unitCountText.text = unitCount.ToString();
        }
    }

    private void UpdateVisuals()
    {
        if (spriteRenderer != null)
        {
            switch (ownerPlayer)
            {
                case 0:
                    spriteRenderer.color = neutralColor;
                    break;
                case 1:
                    spriteRenderer.color = playerColor;
                    break;
                case 2:
                    spriteRenderer.color = aiColor;
                    break;
            }
        }
    }

    public bool IsAdjacentTo(Territory other)
    {
        // For now, simple distance check
        // In final version, you'll define adjacency explicitly
        float distance = Vector2.Distance(transform.position, other.transform.position);
        return distance < 3f && distance > 0.1f; // Adjacent if close but not same territory
    }
}