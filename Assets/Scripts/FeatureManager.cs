using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeatureManager : MonoBehaviour
{
    public Transform[] featurePrefabs;
    Transform container;
    public void Init() {
        if (container) {
            Destroy(container.gameObject);
        }
        container = new GameObject("Feature Container").transform;
        container.SetParent(transform, false);
    }
    public void Apply() { }

    public void AddFeature(Cell cell) {
        if (cell.FeatureTypeIndex == 0) return;
        Transform instance = Instantiate(featurePrefabs[cell.FeatureTypeIndex - 1]);
        instance.localPosition = Metrics.Perturb(cell.Position);
        instance.localRotation = Quaternion.Euler(0f, 360f * Metrics.GetRand(cell.Position), 0f);
        instance.SetParent(container, false);
    }
}
