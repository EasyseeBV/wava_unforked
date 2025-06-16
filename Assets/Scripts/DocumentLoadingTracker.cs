using System;
using UnityEngine;

public class DocumentLoadingTracker : MonoBehaviour
{
    long _totalDocumentsCount;

    long _loadedDocumentsCount;

    public Action<float> _OnProgressChanged;

    public Action _OnAllDocumentsLoaded;

    private void OnEnable()
    {
        FirebaseLoader.OnDocumentLoaded += OnFirebaseDocumentLoaded;
    }

    private void OnDisable()
    {
        FirebaseLoader.OnDocumentLoaded -= OnFirebaseDocumentLoaded;
    }

    public void ResetTracking()
    {
        // Semaphore for not updated.
        _totalDocumentsCount = -1;

        _loadedDocumentsCount = 0;

        _OnProgressChanged?.Invoke(0);
    }

    void OnFirebaseDocumentLoaded()
    {
        // Check if the total document count had been calculated.
        if (_totalDocumentsCount == -1)
        {
            _totalDocumentsCount = 0;
            _totalDocumentsCount += FirebaseLoader.ArtistCollectionSize - FirebaseLoader.ArtistsMap.Count;
            _totalDocumentsCount += FirebaseLoader.ArtworkCollectionSize - FirebaseLoader.ArtworksMap.Count;
            _totalDocumentsCount += FirebaseLoader.ExhibitionCollectionSize - FirebaseLoader.ExhibitionsMap.Count;
        }

        if (_loadedDocumentsCount == _totalDocumentsCount)
            return;

        _loadedDocumentsCount++;

        var progressPercentage = _loadedDocumentsCount / (float)_totalDocumentsCount;
        _OnProgressChanged?.Invoke(progressPercentage);

        Debug.Log(progressPercentage);

        if (_loadedDocumentsCount == _totalDocumentsCount)
            _OnAllDocumentsLoaded.Invoke();
    }
}
