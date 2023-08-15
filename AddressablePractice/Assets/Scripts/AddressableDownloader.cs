using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AddressableDownloader
{
    public static string DownloadURL;

    private DownloadEvents _events;
    private string _label;
    private long _totalSize;
    private AsyncOperationHandle _downloadHandle;

    public DownloadEvents InitializeSystem(string label, string downloadURL)
    {
        _events = new DownloadEvents();

        Addressables.InitializeAsync().Completed += OnInitialized;

        _label = label;
        DownloadURL = downloadURL;

        ResourceManager.ExceptionHandler += OnException;

        return _events;
    }
   
    public void Update()
    {
        if (_downloadHandle.IsValid()
            && _downloadHandle.IsDone == false
            && _downloadHandle.Status != AsyncOperationStatus.Failed)
        {
            var status = _downloadHandle.GetDownloadStatus();

            _events.NotifyDownloadProgress(
                new DownloadProgressStatus(
                    _totalSize
                    , status.DownloadedBytes
                    , _totalSize - status.DownloadedBytes
                    , status.Percent));
        }
    }    

    public void UpdateCatalog()
    {
        Addressables.CheckForCatalogUpdates().Completed += OnCheckForCatalogUpdates;
    }

    public void DownloadSize()
    {
        Addressables.GetDownloadSizeAsync(_label).Completed += OnSizeDownloaded;
    }    

    public void StartDownload()
    {
        _downloadHandle = Addressables.DownloadDependenciesAsync(_label);
        _downloadHandle.Completed += OnDependenciesDownloaded;
    }    

    private void OnInitialized(AsyncOperationHandle<IResourceLocator> result)
    {
        _events.NotifyInitialized();
    }

    private void OnException(AsyncOperationHandle handle, Exception exp)
    {
        Debug.LogError("CustomExceptionCaught ! :" + exp.Message);

        if (exp is UnityEngine.ResourceManagement.Exceptions.RemoteProviderException)
        {
            // Remote 관련 에러 발생
        }

        if (false == Utils.IsNetworkValid())
        {
            // 네트워크 관련 에러 발생
        }

        if (false == Utils.IsDiskSpaceEnough(_totalSize))
        {
            // 저장공간 관련 에러 발생
        }
    }

    private void OnCheckForCatalogUpdates(AsyncOperationHandle<List<string>> result)
    {
        var catalogToUpdate = result.Result;
        if (catalogToUpdate.Count > 0)
        {
            Addressables.UpdateCatalogs(catalogToUpdate).Completed += OnCatalogUpdate;
        }
        else
        {
            _events.NotifyCatalogUpdated();
        }
    }

    private void OnCatalogUpdate(AsyncOperationHandle<List<IResourceLocator>> result)
    {
        _events.NotifyCatalogUpdated();
    }

    private void OnSizeDownloaded(AsyncOperationHandle<long> result)
    {
        _totalSize = result.Result;
        _events.NotifySizeDownloaded(_totalSize);
    }

    private void OnDependenciesDownloaded(AsyncOperationHandle result)
    {
        _events.NotifyDownloadFinished(result.Status == AsyncOperationStatus.Succeeded);
    }
}
