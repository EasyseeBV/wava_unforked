using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Messy.Definitions;
using UnityEngine;

public class LoadNewestExhibition : MonoBehaviour
{
    [SerializeField] private ExhibitionCard exhibitionCard;
    [SerializeField] private ArtistContainer artistContainer;
    
    void Start()
    {
        if (ARInfoManager.ExhibitionsSO == null || ARInfoManager.ExhibitionsSO.Count <= 0) return;
            
        List<ExhibitionSO> sortedExhibitions = ARInfoManager.ExhibitionsSO.OrderByDescending(e => e.Year).ToList();

        var exhibition = sortedExhibitions.FirstOrDefault();
        exhibitionCard.Init(exhibition);

        if (exhibition.ArtWorks[0].Artists.Count > 0)
        {
            artistContainer.Assign(exhibition.ArtWorks[0].Artists[0]);
        }
        else
        {
            artistContainer.gameObject.SetActive(false);
        }
    }
}
