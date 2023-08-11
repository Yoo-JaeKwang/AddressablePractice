using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class BasicAPITest : MonoBehaviour
{
    private IEnumerator Start()
    {
        // 초기화
        yield return Addressables.InitializeAsync();

        // 에셋 로드
        // Reference Count + 1
        var loadHandle = Addressables.LoadAssetAsync<GameObject>("MyCube");
        yield return loadHandle;

        // 인스턴스 생성
        // Reference Count + 1
        var instantiateHandle = Addressables.InstantiateAsync("MyCube");
        GameObject createdObject = null;
        instantiateHandle.Completed += (result) =>
        {
            createdObject = result.Result;
        };
        yield return instantiateHandle;

        yield return new WaitForSeconds(3);

        // 인스턴스 삭제
        // Reference Count - 1
        Addressables.ReleaseInstance(createdObject);

        // 에셋 언로드
        // Reference Count - 1
        Addressables.Release(loadHandle);
    }
}
