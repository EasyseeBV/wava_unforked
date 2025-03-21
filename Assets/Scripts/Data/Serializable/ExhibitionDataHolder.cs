using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class ExhibitionDataHolder
{
    public string title;
    public string description;
    public List<string> artist_ids = new List<string>();
    public List<string> artwork_ids = new List<string>();
    public int year;
    public string location;
    public List<string> exhibition_image_references = new List<string>();
    public string creation_date_time; // ISO string
    public string update_date_time;   // ISO string
    public string exhibition_id;
    public bool published;
    public string color;
    public List<string> cache = new List<string>();

    public static ExhibitionDataHolder FromExhibitionData(ExhibitionData exhibition)
    {
        ExhibitionDataHolder holder = new ExhibitionDataHolder();
        holder.title = exhibition.title;
        holder.description = exhibition.description;
        holder.artist_ids = new List<string>();
        if (exhibition.artists != null)
        {
            foreach (var artist in exhibition.artists)
            {
                holder.artist_ids.Add(artist.id);
            }
        }
        holder.artwork_ids = new List<string>();
        if (exhibition.artworks != null)
        {
            foreach (var artwork in exhibition.artworks)
            {
                holder.artwork_ids.Add(artwork.id);
            }
        }
        holder.year = exhibition.year;
        holder.location = exhibition.location;
        holder.exhibition_image_references = new List<string>(exhibition.exhibition_image_references);
        holder.creation_date_time = exhibition.creation_date_time.ToString("O");
        holder.update_date_time = exhibition.update_date_time.ToString("o");
        holder.exhibition_id = exhibition.id;
        holder.published = exhibition.published;
        holder.color = exhibition.color;
        holder.cache = new List<string>(exhibition.cached);
        return holder;
    }

    public static ExhibitionData FromHolder(ExhibitionDataHolder holder)
    {
        ExhibitionData exhibition = new ExhibitionData();
        exhibition.title = holder.title;
        exhibition.description = holder.description;
        exhibition.artists = new List<ArtistData>();
        if (holder.artist_ids != null)
        {
            foreach (var artistData in holder.artist_ids.SelectMany(artistID => FirebaseLoader.Artists.Where(artistData => artistData.id == artistID && !exhibition.artists.Contains(artistData))))
            {
                exhibition.artists.Add(artistData);
            }
        }
        exhibition.artworks = new List<ArtworkData>();
        if (holder.artwork_ids != null)
        {
            foreach (var artworkData in holder.artwork_ids.SelectMany(artworkStr => FirebaseLoader.Artworks.Where(artworkData => artworkData.id == artworkStr && !exhibition.artworks.Contains(artworkData))))
            {
                exhibition.artworks.Add(artworkData);
            }
        }
        exhibition.year = holder.year;
        exhibition.location = holder.location;
        exhibition.exhibition_image_references = new List<string>(holder.exhibition_image_references);
        exhibition.creation_date_time = DateTime.Parse(holder.creation_date_time);
        exhibition.update_date_time = DateTime.Parse(holder.update_date_time);
        exhibition.id = holder.exhibition_id;
        exhibition.published = holder.published;
        exhibition.color = holder.color;
        exhibition.cached = new List<string>(holder.cache);
        return exhibition;
    }
}
