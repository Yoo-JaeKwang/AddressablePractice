using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class Ingame : MonoBehaviour
{
    [SerializeField] private Image _image;
    [SerializeField] private Text _textResult;
    [SerializeField] private string _targetSpriteKey;

    private void Start()
    {
        Addressables.LoadAssetAsync<Sprite>(_targetSpriteKey).Completed += (result) =>
        {
            if (result.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
            {
                _textResult.text = "성공 !";
                _image.sprite = result.Result;
            }
            else
            {
                _textResult.text = "실패 !";
                _image.sprite = null;
            }
        };
    }

    public void OnClickGoDownloadScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}
