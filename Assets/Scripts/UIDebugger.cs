
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIDebugger : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("=== CLICK DEBUG ===");

            // Check EventSystem
            EventSystem es = EventSystem.current;
            if (es == null)
            {
                Debug.LogError("❌ NO EVENTSYSTEM!");
                return;
            }
            Debug.Log("✅ EventSystem exists");

            // Check what's under mouse
            PointerEventData pointer = new PointerEventData(es);
            pointer.position = Input.mousePosition;

            var results = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, results);

            Debug.Log($"Found {results.Count} objects under mouse:");

            foreach (var result in results)
            {
                Debug.Log($"  - {result.gameObject.name} (Canvas: {result.gameObject.GetComponentInParent<Canvas>()?.name})");

                Canvas canvas = result.gameObject.GetComponentInParent<Canvas>();
                if (canvas != null)
                {
                    Debug.Log($"    Canvas Sort Order: {canvas.sortingOrder}");
                    Debug.Log($"    Canvas Render Mode: {canvas.renderMode}");
                }
            }

            if (results.Count == 0)
            {
                Debug.LogError("❌ NOTHING DETECTED UNDER MOUSE!");
                Debug.Log("This means UI is not receiving raycasts.");

                // Check all canvases
                Canvas[] allCanvases = FindObjectsOfType<Canvas>();
                Debug.Log($"\nAll Canvases in scene ({allCanvases.Length}):");
                foreach (Canvas c in allCanvases)
                {
                    GraphicRaycaster gr = c.GetComponent<GraphicRaycaster>();
                    Debug.Log($"  - {c.name}: Active={c.gameObject.activeInHierarchy}, SortOrder={c.sortingOrder}, Raycaster={gr != null}");
                }
            }
        }
    }
}