using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class RessourceInstance : BuildingInstance {

    public void Start() {
        base.Start();
        StartCoroutine(ProduceResources());
    }
    
    private IEnumerator ProduceResources()
    {
        while (currentHealth > 0)
        {
            yield return new WaitForSeconds(60f);
            //AddResourcesServerRpc(ResourceData.resourcePerMinute);
        }
    }
}
