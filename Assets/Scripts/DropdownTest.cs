
using TMPro;
using UnityEngine;

public class DropdownTest : MonoBehaviour
{
    public TMP_Dropdown testDropdown; // Connect CountryDropdown here

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("🔑 SPACE pressed - forcing dropdown open");

            if (testDropdown != null)
            {
                Debug.Log($"Dropdown interactable: {testDropdown.interactable}");
                Debug.Log($"Dropdown active: {testDropdown.gameObject.activeInHierarchy}");
                Debug.Log($"Dropdown enabled: {testDropdown.enabled}");
                Debug.Log($"Dropdown options: {testDropdown.options.Count}");

                // Force show dropdown
                testDropdown.Show();
            }
        }
    }
}