using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ArtistDataHolder
{
    public string title;
    public string description;
    public string location;
    public string link;
    public string icon;
    public string creation_time; // ISO string
    public string update_time;   // ISO string
    public string artist_id;
    public List<string> cache = new List<string>();
    public bool published;

    public static ArtistDataHolder FromArtistData(ArtistData artist)
    {
        ArtistDataHolder holder = new ArtistDataHolder();
        holder.title = artist.title;
        holder.description = artist.description;
        holder.location = artist.location;
        holder.link = artist.link;
        holder.icon = artist.icon;
        holder.creation_time = artist.creation_time.ToDateTime().ToString("o");
        holder.update_time = artist.update_time.ToDateTime().ToString("o");
        holder.artist_id = artist.id;
        holder.cache = new List<string>(artist.cached);
        holder.published = artist.published;
        return holder;
    }

    public static ArtistData ToArtistData(ArtistDataHolder holder)
    {
        ArtistData artist = new ArtistData();
        artist.title = holder.title;
        artist.description = holder.description;
        artist.location = holder.location;
        artist.link = holder.link;
        artist.icon = holder.icon;
        artist.creation_date_time = DateTime.Parse(holder.creation_time);
        artist.update_date_time = DateTime.Parse(holder.update_time);
        artist.id = holder.artist_id;
        artist.cached = new List<string>(holder.cache);
        artist.published = holder.published;
        return artist;
    }
}