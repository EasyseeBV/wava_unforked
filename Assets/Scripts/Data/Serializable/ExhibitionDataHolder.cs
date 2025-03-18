using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ExhibitionDataHolder
{
    public string title;
    public string description;
    public List<ArtistDataHolder> artists;
    public List<ArtworkDataHolder> artworks;
    public int year;
    public string location;
    public List<string> exhibition_image_references;
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
        holder.artists = new List<ArtistDataHolder>();
        if (exhibition.artists != null)
        {
            foreach (var artist in exhibition.artists)
            {
                holder.artists.Add(ArtistDataHolder.FromArtistData(artist));
            }
        }
        holder.artworks = new List<ArtworkDataHolder>();
        if (exhibition.artworks != null)
        {
            foreach (var artwork in exhibition.artworks)
            {
                holder.artworks.Add(ArtworkDataHolder.ToHolder(artwork));
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
        if (holder.artists != null)
        {
            foreach (var artistHolder in holder.artists)
            {
                exhibition.artists.Add(ArtistDataHolder.ToArtistData(artistHolder));
            }
        }
        exhibition.artworks = new List<ArtworkData>();
        if (holder.artworks != null)
        {
            foreach (var artworkHolder in holder.artworks)
            {
                exhibition.artworks.Add(ArtworkDataHolder.FromHolder(artworkHolder));
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
