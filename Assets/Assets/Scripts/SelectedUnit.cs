using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SelectedUnit : MonoBehaviour
{
    public bool isSelected = false;
    public GameObject selectionBox;

    InstalledUnit installedUnit;
    
    void Start()
    {
        installedUnit = GetComponent<InstalledUnit>();
        UnitSelector.instance.selectUnit(this);
    }

    void Update()
{
     if (isSelected) 
     {
        if (Input.GetMouseButtonDown(1))
        {
            installedUnit.moveUnit(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            Debug.Log("Move");
        }



     }


}

    public void Select()
    {
        isSelected = true;
        Debug.Log("Select");
        selectionBox.SetActive(true);
    }

    public void Deselect()
    {
        isSelected = false;
        selectionBox.SetActive(false);
    }
   
}
