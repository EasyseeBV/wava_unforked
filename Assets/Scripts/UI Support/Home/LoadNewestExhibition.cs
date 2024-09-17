using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Messy.Definitions;
using UnityEngine;

public class LoadNewestExhibition : MonoBehaviour
{
    [SerializeField] private ExhibitionCard exhibitionCard;
    [SerializeField] private ArtistContainer artistContainer;

    private static List<ArtistSO> cachedArtists;
    
    void Start()
    {
        if (ARInfoManager.ExhibitionsSO == null || ARInfoManager.ExhibitionsSO.Count <= 0) return;
            
        List<ExhibitionSO> sortedExhibitions = ARInfoManager.ExhibitionsSO.OrderByDescending(e => e.Year).ToList();

        var exhibition = sortedExhibitions.FirstOrDefault();
        exhibitionCard.Init(exhibition);

        if (cachedArtists == null || cachedArtists.Count <= 0)
        {
            cachedArtists = new List<ArtistSO>();
            foreach (var exhibitionSO in sortedExhibitions)
            {
                foreach (var artWork in exhibitionSO.ArtWorks)
                {
                    cachedArtists.AddRange(artWork.Artists);
                }
            }
        }

        if (cachedArtists.Count > 0)
        {
            artistContainer.Assign(cachedArtists[Random.Range(0, cachedArtists.Count)]);
        }
    }
}
