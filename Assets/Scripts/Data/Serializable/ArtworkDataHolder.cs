using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ArtworkDataHolder
{
    public string title;
    public string description;
    public List<ArtistDataHolder> artists;
    public int year;
    public string location;
    public List<string> artwork_image_references;
    public double latitude;
    public double longitude;
    public float max_distance;
    public bool place_right;
    public string creation_time; // stored as ISO string
    public string update_time;   // stored as ISO string
    public string media_content;
    public TransformsDataHolder transforms;
    public string artwork_id;

    public static ArtworkDataHolder ToHolder(ArtworkData artwork)
    {
        ArtworkDataHolder holder = new ArtworkDataHolder();
        holder.title = artwork.title;
        holder.description = artwork.description;
        holder.artists = new List<ArtistDataHolder>();
        if (artwork.artists != null)
        {
            foreach (var artist in artwork.artists)
            {
                holder.artists.Add(ArtistDataHolder.FromArtistData(artist));
            }
        }
        holder.year = artwork.year;
        holder.location = artwork.location;
        holder.artwork_image_references = new List<string>(artwork.artwork_image_references);
        holder.latitude = artwork.latitude;
        holder.longitude = artwork.longitude;
        holder.max_distance = artwork.max_distance;
        holder.place_right = artwork.place_right;
        holder.creation_time = artwork.creation_date_time.ToString("o");
        holder.update_time = artwork.update_date_time.ToString("o");
        holder.media_content = artwork.media_content;
        holder.transforms = TransformsDataHolder.FromTransformsData(artwork.transforms);
        holder.artwork_id = artwork.artwork_id;
        return holder;
    }

    public static ArtworkData FromHolder(ArtworkDataHolder holder)
    {
        ArtworkData artwork = new ArtworkData();
        artwork.title = holder.title;
        artwork.description = holder.description;
        artwork.artists = new List<ArtistData>();
        if (holder.artists != null)
        {
            foreach (var artistHolder in holder.artists)
            {
                artwork.artists.Add(ArtistDataHolder.ToArtistData(artistHolder));
            }
        }
        artwork.year = holder.year;
        artwork.location = holder.location;
        artwork.artwork_image_references = new List<string>(holder.artwork_image_references);
        artwork.latitude = holder.latitude;
        artwork.longitude = holder.longitude;
        artwork.max_distance = holder.max_distance;
        artwork.place_right = holder.place_right;
        artwork.creation_date_time = DateTime.Parse(holder.creation_time);
        artwork.update_date_time = DateTime.Parse(holder.update_time);
        artwork.media_content = holder.media_content;
        artwork.transforms = TransformsDataHolder.ToTransformsData(holder.transforms);
        artwork.artwork_id = holder.artwork_id;
        return artwork;
    }
}

[Serializable]
public class TransformsDataHolder
{
    public PositionOffsetHolder position_offset;
    public float rotation;
    public ScaleHolder scale;

    public static TransformsDataHolder FromTransformsData(TransformsData transforms)
    {
        if (transforms == null) return null;
        TransformsDataHolder holder = new TransformsDataHolder();
        holder.position_offset = PositionOffsetHolder.FromPositionOffset(transforms.position_offset);
        holder.rotation = transforms.rotation;
        holder.scale = ScaleHolder.FromScale(transforms.scale);
        return holder;
    }

    public static TransformsData ToTransformsData(TransformsDataHolder holder)
    {
        if (holder == null) return null;
        TransformsData transforms = new TransformsData();
        transforms.position_offset = PositionOffsetHolder.ToPositionOffset(holder.position_offset);
        transforms.rotation = holder.rotation;
        transforms.scale = ScaleHolder.ToScale(holder.scale);
        return transforms;
    }
}

[Serializable]
public class PositionOffsetHolder
{
    public float x_offset;
    public float y_offset;
    public float z_offset;

    public static PositionOffsetHolder FromPositionOffset(PositionOffset offset)
    {
        if (offset == null) return null;
        return new PositionOffsetHolder { x_offset = offset.x_offset, y_offset = offset.y_offset, z_offset = offset.z_offset };
    }

    public static PositionOffset ToPositionOffset(PositionOffsetHolder holder)
    {
        if (holder == null) return null;
        return new PositionOffset { x_offset = holder.x_offset, y_offset = holder.y_offset, z_offset = holder.z_offset };
    }
}

[Serializable]
public class ScaleHolder
{
    public float x_scale;
    public float y_scale;
    public float z_scale;

    public static ScaleHolder FromScale(Scale scale)
    {
        if (scale == null) return null;
        return new ScaleHolder { x_scale = scale.x_scale, y_scale = scale.y_scale, z_scale = scale.z_scale };
    }

    public static Scale ToScale(ScaleHolder holder)
    {
        if (holder == null) return null;
        return new Scale { x_scale = holder.x_scale, y_scale = holder.y_scale, z_scale = holder.z_scale };
    }
}
