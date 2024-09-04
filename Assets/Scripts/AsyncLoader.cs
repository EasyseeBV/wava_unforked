using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Messy
{
    public class AsyncLoader : MonoBehaviour
    {
        [SerializeField] private AssetReferenceGameObject assetReferenceGameObject;

        public void LoadAssetFromReference()
        {
            assetReferenceGameObject.LoadAssetAsync<GameObject>().Completed += (asyncOperationHandle) =>
            {
                if (asyncOperationHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    Instantiate(asyncOperationHandle.Result);
                }
                else
                {
                    Debug.Log("Asset not found!");
                }
            };
        }
    }
}