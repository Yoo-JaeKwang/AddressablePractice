using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DownloadPopup : MonoBehaviour
{
    public enum State
    {
        None,

        CalculatingSize,
        NothingToDownload,

        AskingDownload,
        Downloading,
        DownloadFinished
    }

    [Serializable]
    public class Root
    {
        public State state;
        public GameObject root;
    }

    [SerializeField] private List<Root> _roots;

    [SerializeField] private Text _textTitle;

    [SerializeField] private Text _textDesc;

    [SerializeField] private Text _textDownloadingBarStatus;

    [SerializeField] private Slider _downloadProgressBar;

    [SerializeField] private DownloadController _downloader;

    private DownloadProgressStatus _progressInfo;
    private SizeUnits _sizeUnit;
    private long _curDownloadedSizeInUnit;
    private long _totalSizeInUnit;

    public State CurrentState { get; private set; } = State.None;

    private IEnumerator Start()
    {
        SetState(State.CalculatingSize, true);

        yield return _downloader.StartDownloadRoutine((events) =>
        {
            events.SystemInitializedListener += OnInitialized;
            events.CatalogUpdatedListener += OnCatalogUpdated;
            events.SizeDownloadedListener += OnSizeDownloaded;
            events.DownloadProgressListener += OnDownloadProgress;
            events.DownloadFinished += OnDownloadFinished;
        });
    }

    private void SetState(State newState, bool updateUI)
    {
        var prevRoot = _roots.Find(root => root.state == CurrentState);
        var newRoot = _roots.Find(root => root.state == newState);

        CurrentState = newState;

        if (prevRoot != null)
        {
            prevRoot.root.gameObject.SetActive(false);
        }

        if (newRoot != null)
        {
            newRoot.root.gameObject.SetActive(true);
        }

        if (updateUI)
        {
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        switch (CurrentState)
        {
            case State.CalculatingSize:
                _textTitle.text = "알림";
                _textDesc.text = "다운로드 정보를 가져오고 있습니다. 잠시만 기다려주세요.";
                break;
            case State.NothingToDownload:
                _textTitle.text = "완료";
                _textDesc.text = "다운로드 받을 데이터가 없습니다.";
                break;
            case State.AskingDownload:
                _textTitle.text = "주의";
                _textDesc.text = $"다운로드를 받으시겠습니까 ? 데이터가 많이 사용될 수 있습니다. <color=green>({_totalSizeInUnit}{_sizeUnit})</color>";
                break;
            case State.Downloading:
                _textTitle.text = "다운로드 중";
                _textDesc.text = $"다운로드 중입니다. 잠시만 기다려주세요. {(_progressInfo.TotalProgress * 100).ToString("0.00")}% 완료";
                _downloadProgressBar.value = _progressInfo.TotalProgress;
                _textDownloadingBarStatus.text = $"{_curDownloadedSizeInUnit}/{_totalSizeInUnit}{_sizeUnit}";
                break;
            case State.DownloadFinished:
                _textTitle.text = "완료";
                _textDesc.text = "다운로드가 완료되었습니다. 게임을 진행하시겠습니까?";
                break;
        }
    }

    public void OnClickStartDownload()
    {
        SetState(State.Downloading, true);
        _downloader.GoNext();
    }

    public void OnClickCancelButton()
    {
#if UNITY_EDITOR
        if (Application.isEditor)
        {
            UnityEditor.EditorApplication.isPlaying = false;
        }
#else
        Application.Quit();
#endif
    }

    public void OnClickEnterGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }

    private void OnInitialized()
    {
        _downloader.GoNext();
    }

    private void OnCatalogUpdated()
    {
        _downloader.GoNext();
    }

    private void OnSizeDownloaded(long size)
    {
        if (size == 0)
        {
            SetState(State.NothingToDownload, true);
        }
        else
        {
            _sizeUnit = Utils.GetProperByteUnit(size);
            _totalSizeInUnit = Utils.ConvertByteByUnit(size, _sizeUnit);

            SetState(State.AskingDownload, true);
        }
    }

    private void OnDownloadProgress(DownloadProgressStatus newInfo)
    {
        bool changed = _progressInfo.DownloadedBytes != newInfo.DownloadedBytes;

        _progressInfo = newInfo;

        if (changed)
        {
            UpdateUI();

            _curDownloadedSizeInUnit = Utils.ConvertByteByUnit(newInfo.DownloadedBytes, _sizeUnit);
        }
    }

    private void OnDownloadFinished(bool isSuccess)
    {
        SetState(State.DownloadFinished, true);
        _downloader.GoNext();
    }
}
