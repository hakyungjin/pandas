using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SelectedUnit : MonoBehaviour
{
    public bool isSelected = false;
    public GameObject selectionBox;
    
    void Start()
    {
        UnitSelector.instance.selectUnit(this);
    }

    public void Select()
    {
        isSelected = !isSelected;
        Debug.Log("Select");
        selectionBox.SetActive(true);
    }

    public void Deselect()
    {
        isSelected = !isSelected;
        selectionBox.SetActive(false);
    }
   
}
