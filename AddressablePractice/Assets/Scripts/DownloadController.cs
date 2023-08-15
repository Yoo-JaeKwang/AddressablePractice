using System;
using System.Collections;
using UnityEngine;

public class DownloadController : MonoBehaviour
{
    public enum State
    {
        Idle,

        Initialize,
        UpdateCatalog,
        DownloadSize,
        DownloadDependencies,
        Downloading,

        Finished
    }

    private AddressableDownloader _downloader;

    [SerializeField] private string _label;
    [SerializeField] private string _downloadURL;

    public State CurrentState { get; private set; } = State.Idle;
    public State LastValidState { get; private set; } = State.Idle;

    private Action<DownloadEvents> _onEventObtained;

    public IEnumerator StartDownloadRoutine(Action<DownloadEvents> onEventObtained)
    {
        _downloader = new AddressableDownloader();
        _onEventObtained = onEventObtained;

        LastValidState = CurrentState = State.Initialize;

        while (CurrentState != State.Finished)
        {
            OnExecute();
            yield return null;
        }
    }

    private void OnExecute()
    {
        if (CurrentState == State.Idle) { return; }

        switch (CurrentState)
        {
            case State.Initialize:
                var events = _downloader.InitializeSystem(_label, _downloadURL);
                _onEventObtained?.Invoke(events);
                CurrentState = State.Idle;
                break;
            case State.UpdateCatalog:
                _downloader.UpdateCatalog();
                CurrentState = State.Idle;
                break;
            case State.DownloadSize:
                _downloader.DownloadSize();
                CurrentState = State.Idle;
                break;
            case State.DownloadDependencies:
                _downloader.StartDownload();
                CurrentState = State.Downloading;
                break;
            case State.Downloading:
                _downloader.Update();
                break;
        }
    }

    public void GoNext()
    {
        switch (LastValidState)
        {
            case State.Initialize:
                CurrentState = State.UpdateCatalog;
                break;
            case State.UpdateCatalog:
                CurrentState = State.DownloadSize;
                break;
            case State.DownloadSize:
                CurrentState = State.DownloadDependencies;
                break;
            case State.DownloadDependencies or State.Downloading:
                CurrentState = State.Finished;
                break;
        }

        LastValidState = CurrentState;
    }


}
