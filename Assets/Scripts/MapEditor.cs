using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.IO;

public class MapEditor : MonoBehaviour
{
    public Gridz grid;
	public Slider elevationSlider, waterSlider;

	int activeTerrainTypeIndex;
	public void SetTerrainTypeIndex(int index)
    {
		activeTerrainTypeIndex = index;
	}

	private int activeElevation;
	public void SetElevation()
    {
		float elevation = elevationSlider.GetComponent<Slider>().value;
		activeElevation = (int)elevation;
    }

	private int activeWaterLevel;
	public void SetWaterLevel()
    {
		float level = waterSlider.GetComponent<Slider>().value;
		activeWaterLevel = (int)level;
    }

	private int activeFeatureTypeIndex;
	public void SetFeatureTypeIndex(int index)
    {
		activeFeatureTypeIndex = index;
    }

	void Update()
	{
		if (Input.GetMouseButton(0) && 
			!EventSystem.current.IsPointerOverGameObject()) {
			HandleInput();
		}
	}

	void HandleInput()
	{
		Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast(inputRay, out hit)) {
			EditCell(grid.GetCell(hit.point));
		}
	}

	void EditCell(Cell cell)
    {
		cell.Elevation = activeElevation;
		cell.TerrainTypeIndex = activeTerrainTypeIndex;
		cell.WaterLevel = activeWaterLevel;
		cell.FeatureTypeIndex = activeFeatureTypeIndex;
    }

	public void Save()
	{
		Debug.Log(Application.persistentDataPath);
		string path = Path.Combine(Application.persistentDataPath, "test.map");
		using (BinaryWriter writer = 
			new BinaryWriter(File.Open(path, FileMode.Create))) {
			grid.Save(writer);
		}
	}

	public void Load()
	{
		string path = Path.Combine(Application.persistentDataPath, "test.map");
		using (BinaryReader reader =
				new BinaryReader(File.OpenRead(path))) {
			grid.Load(reader);
		}
	}
}
